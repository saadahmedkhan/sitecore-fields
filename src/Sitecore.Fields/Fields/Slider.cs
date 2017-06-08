using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Text;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using System;
using System.IO;
using System.Web.UI;

namespace Sitecore.Shell.Applications.ContentEditor
{
    public class Slider : Input, IContentField
    {
        private const string IS_DEBUG_FIELD_NAME = "IsDebug";
        private const string FROM_FIELD_NAME = "From";
        private const string TO_FIELD_NAME = "To";
        private const string ASSETS_FOLDER_NAME = "Fields\\Slider\\";

        public Slider()
        {
            Class = "scContentControl";
            Activation = true;
        }


        public string Source { get; set; }
        protected bool IsDebug { get; private set; }
        protected int From { get; private set; }
        protected int To { get; private set; }


        public override void HandleMessage(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            base.HandleMessage(message);
            if (message["id"] == this.ID)
            {
                string name = message.Name;
                string str = name;
                if (name != null)
                {
                    if (str == "contentslider:move")
                    {
                        this.Value = message["value"];
                        Sitecore.Context.ClientPage.Modified = true;
                        //Sitecore.Context.ClientPage.Dispatch("contenteditor:save");
                    }
                    else if (str == "contentslider:reset")
                    {
                        this.Value = message["value"];
                        Sitecore.Context.ClientPage.Dispatch("contenteditor:save");
                    }
                }
            }

            base.HandleMessage(message);
        }


        public string GetValue()
        {
            return Value;
        }

        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, "value");
            this.Value = value;
        }


        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            base.ServerProperties["Value"] = base.ServerProperties["Value"];
        }

        protected override void SetModified()
        {
            base.SetModified();
            if (base.TrackModified)
            {
                Sitecore.Context.ClientPage.Modified = true;
            }
        }

        protected override void Render(HtmlTextWriter output)
        {
            ParseParameters(Source);

            var appDomain = AppDomain.CurrentDomain;
            var basePath = appDomain.BaseDirectory;
            var path = Path.Combine(basePath, ASSETS_FOLDER_NAME, "template.html");
            var html = System.IO.File.ReadAllText(path)
                .Replace("{CONTROL_ID}", this.ID)
                .Replace("{FROM}", this.From.ToString())
                .Replace("{TO}", this.To.ToString())
                .Replace("{VALUE}", this.Value)
                .Replace("{IS_DEBUG}", IsDebug.ToString().ToLower());

            output.Write(html);
        }


        private void ParseParameters(string source)
        {
            var parameters = new UrlString(source);

            if (!string.IsNullOrEmpty(parameters.Parameters[FROM_FIELD_NAME]))
            {
                From = MainUtil.GetInt(parameters.Parameters[FROM_FIELD_NAME], 0);
            }
            else
            {
                From = 1;
            }
            if (!string.IsNullOrEmpty(parameters.Parameters[TO_FIELD_NAME]))
            {
                To = MainUtil.GetInt(parameters.Parameters[TO_FIELD_NAME], 100);
            }
            else
            {
                To = 100;
            }

            // WHETHER TO SHOW RAW VALUES
            if (!string.IsNullOrEmpty(parameters.Parameters[IS_DEBUG_FIELD_NAME]))
            {
                IsDebug = MainUtil.GetBool(parameters.Parameters[IS_DEBUG_FIELD_NAME], false);
            }
        }
    }
}