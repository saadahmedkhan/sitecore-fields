using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Globalization;
using Sitecore.IO;
using Sitecore.Links;
using Sitecore.Resources.Media;
using Sitecore.Shell.Applications.Dialogs.MediaBrowser;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.XamlSharp;
using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Sitecore.Shell.Applications.ContentEditor
{
    public class AdvanceImage : LinkBase
    {
        private const string THUMBNAIL_FOLDER_FIELD_NAME = "ThumbnailsFolderID";
        private const string IS_DEBUG_FIELD_NAME = "IsDebug";
        private const string ASSETS_FOLDER_NAME = "Fields\\AdvanceImage\\";

        public string ItemVersion
        {
            get
            {
                return base.GetViewStateString("Version");
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                base.SetViewStateString("Version", value);
            }
        }

        protected XmlValue XmlValue
        {
            get
            {
                XmlValue viewStateProperty = base.GetViewStateProperty("XmlValue", null) as XmlValue;
                if (viewStateProperty == null)
                {
                    viewStateProperty = new XmlValue(string.Empty, "image");
                    this.XmlValue = viewStateProperty;
                }
                return viewStateProperty;
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                base.SetViewStateProperty("XmlValue", value, null);
            }
        }

        protected string ThumbnailsFolderID { get; private set; }

        protected string IsDebug { get; private set; }


        public AdvanceImage()
        {
            this.Class = "scContentControlImage";
            base.Change = "#";
            base.Activation = true;
        }


        protected void Browse()
        {
            if (this.Disabled)
            {
                return;
            }
            Sitecore.Context.ClientPage.Start(this, "BrowseImage");
        }

        protected void BrowseImage(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (!args.IsPostBack)
            {
                //string[] source = new string[] { this.Source, "/sitecore/media library" };
                string[] source = new string[] { "/sitecore/media library" };
                string str = StringUtil.GetString(source);
                string str1 = str;
                string attribute = this.XmlValue.GetAttribute("mediaid");
                string str2 = attribute;
                if (str.StartsWith("~", StringComparison.InvariantCulture))
                {
                    str1 = StringUtil.Mid(str, 1);
                    if (string.IsNullOrEmpty(attribute))
                    {
                        attribute = str1;
                    }
                    str = "/sitecore/media library";
                }
                Language language = Language.Parse(this.ItemLanguage);
                MediaBrowserOptions mediaBrowserOption = new MediaBrowserOptions();
                Item item = Client.ContentDatabase.GetItem(str, language);
                if (item == null)
                {
                    throw new ClientAlertException("The source of this Image field points to an item that does not exist.");
                }
                mediaBrowserOption.Root = item;
                if (!string.IsNullOrEmpty(attribute))
                {
                    Item item1 = Client.ContentDatabase.GetItem(attribute, language);
                    if (item1 != null)
                    {
                        mediaBrowserOption.SelectedItem = item1;
                    }
                }
                UrlHandle urlHandle = new UrlHandle();
                urlHandle["ro"] = str;
                urlHandle["fo"] = str1;
                urlHandle["db"] = Client.ContentDatabase.Name;
                urlHandle["la"] = this.ItemLanguage;
                urlHandle["va"] = str2;
                UrlString urlString = mediaBrowserOption.ToUrlString();
                urlHandle.Add(urlString);
                SheerResponse.ShowModalDialog(urlString.ToString(), "1200px", "700px", string.Empty, true);
                args.WaitForPostBack();
            }
            else if (!string.IsNullOrEmpty(args.Result) && args.Result != "undefined")
            {
                MediaItem mediaItem = Client.ContentDatabase.Items[args.Result];
                if (mediaItem == null)
                {
                    SheerResponse.Alert("Item not found.", new string[0]);
                    return;
                }
                TemplateItem template = mediaItem.InnerItem.Template;
                if (template != null && !this.IsImageMedia(template))
                {
                    SheerResponse.Alert("The selected item does not contain an image.", new string[0]);
                    return;
                }
                this.XmlValue.SetAttribute("mediaid", mediaItem.ID.ToString());
                this.Value = mediaItem.MediaPath;
                this.Update(false);
                this.SetModified();
                return;
            }
        }

        protected override void DoChange(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            base.DoChange(message);
            if (!string.IsNullOrEmpty(this.Value))
            {
                string value = this.Value;
                if (!value.StartsWith("/sitecore", StringComparison.InvariantCulture))
                {
                    value = string.Concat("/sitecore/media library", value);
                }
                MediaItem item = Client.ContentDatabase.GetItem(value, Language.Parse(this.ItemLanguage));
                if (item == null)
                {
                    this.SetValue(string.Empty);
                }
                else
                {
                    this.SetValue(item);
                }
                this.Update();
                this.SetModified();
            }
            else
            {
                this.ClearImage();
            }
            SheerResponse.SetReturnValue(true);
        }

        protected override void DoRender(HtmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, "output");
            base.DoRender(output);

            ParseParameters(Source);

            string src;
            Item mediaItem = this.GetMediaItem();
            this.GetSrc(out src);
            string str = string.Concat(" src=\"", src, "\"");
            string str1 = string.Concat(" id=\"", this.ID, "_image\"");
            string str2 = string.Concat(" alt=\"", (mediaItem != null ? HttpUtility.HtmlEncode(mediaItem["Alt"]) : string.Empty), "\"");
            string[] strArrays = new string[] { str1, str, str2 };

            var appDomain = AppDomain.CurrentDomain;
            var basePath = appDomain.BaseDirectory;
            var path = Path.Combine(basePath, ASSETS_FOLDER_NAME, "template.html");
            var imagesHtml = GetThumbnails();
            var html = System.IO.File.ReadAllText(path)
                .Replace("{CONTROL_ID}", this.ID)
                .Replace("{IMAGE_SRC}", src)
                .Replace("{IMAGE_ATTRS}", string.Concat(strArrays))
                .Replace("{IMAGE_DETAILS}", this.GetDetails())
                .Replace("{THUMBNAILS}", imagesHtml)
                .Replace("{IS_DEBUG}", IsDebug)
                .Replace("{CROP_FOCUS}", string.Format("{0},{1},{2},{3}",
                    XmlValue.GetAttribute("cropx"),
                    XmlValue.GetAttribute("cropy"),
                    XmlValue.GetAttribute("focusx"),
                    XmlValue.GetAttribute("focusy")));

            output.Write(html);
        }

        protected void Edit()
        {
            string attribute = this.XmlValue.GetAttribute("mediaid");
            if (string.IsNullOrEmpty(attribute))
            {
                SheerResponse.Alert("Select an image from the Media Library first.", new string[0]);
                return;
            }
            Item item = Client.ContentDatabase.GetItem(attribute, Language.Parse(this.ItemLanguage));
            if (item == null)
            {
                SheerResponse.Alert("Select an image from the Media Library first.", new string[0]);
                return;
            }
            if ((new MediaItem(item)).MimeType.ToLower() == "image/svg+xml")
            {
                SheerResponse.Alert("Editing SVG images is unsupported.", new string[0]);
                return;
            }
            if (!this.Disabled)
            {
                Sitecore.Context.ClientPage.Start(this, "EditImage");
            }
        }

        protected void EditImage(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            string attribute = this.XmlValue.GetAttribute("mediaid");
            if (string.IsNullOrEmpty(attribute))
            {
                SheerResponse.Alert("Select an image from the Media Library first.", new string[0]);
                return;
            }
            if (!args.IsPostBack)
            {
                Item item = Client.ContentDatabase.GetItem(attribute);
                if (item == null)
                {
                    return;
                }
                ItemLink[] referrers = Globals.LinkDatabase.GetReferrers(item);
                if (referrers != null && (int)referrers.Length > 1)
                {
                    SheerResponse.Confirm(string.Format("This media item is referenced by {0} other items.\n\nEditing the media item will change it for all the referencing items.\n\nAre you sure you want to continue?", (int)referrers.Length));
                    args.WaitForPostBack();
                    return;
                }
            }
            else if (args.Result != "yes")
            {
                args.AbortPipeline();
                return;
            }
            Item item1 = Client.ContentDatabase.GetItem(attribute);
            if (item1 == null)
            {
                Sitecore.Shell.Framework.Windows.RunApplication("Media/Imager", string.Concat("id=", attribute, "&la=", this.ItemLanguage));
            }
            string str = "webdav:compositeedit";
            Command command = CommandManager.GetCommand(str);
            if (command == null)
            {
                SheerResponse.Alert(Translate.Text("Edit command not found."), new string[0]);
                return;
            }
            CommandState commandState = CommandManager.QueryState(str, item1);
            if (commandState == CommandState.Disabled || commandState == CommandState.Hidden)
            {
                Sitecore.Shell.Framework.Windows.RunApplication("Media/Imager", string.Concat("id=", attribute, "&la=", this.ItemLanguage));
            }
            command.Execute(new CommandContext(item1));
        }


        public override string GetValue()
        {
            return this.XmlValue.ToString();
        }

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
                    if (str == "contentimage:open")
                    {
                        this.Browse();
                        return;
                    }
                    if (str == "contentimage:properties")
                    {
                        Sitecore.Context.ClientPage.Start(this, "ShowProperties");
                        return;
                    }
                    if (str == "contentimage:edit")
                    {
                        this.Edit();
                        return;
                    }
                    if (str == "contentimage:load")
                    {
                        this.LoadImage();
                        return;
                    }
                    if (str == "contentimage:clear")
                    {
                        this.ClearImage();
                        return;
                    }
                    if (str == "contentimage:crop" && !string.IsNullOrEmpty(message["cx"]) && !string.IsNullOrEmpty(message["cy"]))
                    {
                        this.XmlValue.SetAttribute("cropx", message["cx"]);
                        this.XmlValue.SetAttribute("cropy", message["cy"]);

                        this.XmlValue.SetAttribute("focusx", message["fx"]);
                        this.XmlValue.SetAttribute("focusy", message["fy"]);
                    }
                    if (str == "contentimage:refresh")
                    {
                        //string src;
                        //this.GetSrc(out src);

                        //if (string.IsNullOrEmpty(src))
                        //{
                        //    this.Update(false);
                        //}
                        //else
                        //{
                        //    this.Update();
                        //}
                    }
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnPreRender(e);
            base.ServerProperties["Value"] = base.ServerProperties["Value"];
            base.ServerProperties["XmlValue"] = base.ServerProperties["XmlValue"];
            base.ServerProperties["Language"] = base.ServerProperties["Language"];
            base.ServerProperties["Version"] = base.ServerProperties["Version"];
            base.ServerProperties["Source"] = base.ServerProperties["Source"];
        }

        protected override void SetModified()
        {
            base.SetModified();
            if (base.TrackModified)
            {
                Sitecore.Context.ClientPage.Modified = true;
            }
        }

        public override void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, "value");
            this.XmlValue = new XmlValue(value, "image");
            this.Value = this.GetMediaPath();
        }


        protected void SetValue(MediaItem item)
        {
            Assert.ArgumentNotNull(item, "item");
            this.XmlValue.SetAttribute("mediaid", item.ID.ToString());
            this.Value = this.GetMediaPath();
        }

        protected void LoadImage()
        {
            string attribute = this.XmlValue.GetAttribute("mediaid");
            if (string.IsNullOrEmpty(attribute))
            {
                SheerResponse.Alert("Select an image from the Media Library first.", new string[0]);
                return;
            }
            if (!UserOptions.View.ShowEntireTree)
            {
                Item item = Client.CoreDatabase.GetItem("/sitecore/content/Applications/Content Editor/Applications/MediaLibraryForm");
                if (item != null)
                {
                    Item item1 = Client.ContentDatabase.GetItem(attribute);
                    if (item1 != null)
                    {
                        UrlString urlString = new UrlString(item["Source"]);
                        urlString["pa"] = "1";
                        urlString["pa0"] = WebUtil.GetQueryString("pa0", string.Empty);
                        urlString["la"] = WebUtil.GetQueryString("la", string.Empty);
                        urlString["pa1"] = HttpUtility.UrlEncode(item1.Uri.ToString());
                        SheerResponse.SetLocation(urlString.ToString());
                        return;
                    }
                }
            }
            Language language = Language.Parse(this.ItemLanguage);
            ClientPage clientPage = Sitecore.Context.ClientPage;
            string[] name = new string[] { "item:load(id=", attribute, ",language=", language.Name, ")" };
            clientPage.SendMessage(this, string.Concat(name));
        }

        protected void ShowProperties(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (this.Disabled)
            {
                return;
            }
            string attribute = this.XmlValue.GetAttribute("mediaid");
            if (string.IsNullOrEmpty(attribute))
            {
                SheerResponse.Alert("Select an image from the Media Library first.", new string[0]);
                return;
            }
            if (!args.IsPostBack)
            {
                string str = FileUtil.MakePath("/sitecore/shell", ControlManager.GetControlUrl(new ControlName("Sitecore.Shell.Applications.Media.ImageProperties")));
                UrlString urlString = new UrlString(str);
                Item item = Client.ContentDatabase.GetItem(attribute, Language.Parse(this.ItemLanguage));
                if (item == null)
                {
                    SheerResponse.Alert("Select an image from the Media Library first.", new string[0]);
                    return;
                }
                item.Uri.AddToUrlString(urlString);
                UrlHandle urlHandle = new UrlHandle();
                urlHandle["xmlvalue"] = this.XmlValue.ToString();
                urlHandle.Add(urlString);
                SheerResponse.ShowModalDialog(urlString.ToString(), true);
                args.WaitForPostBack();
            }
            else if (args.HasResult)
            {
                this.XmlValue = new XmlValue(args.Result, "image");
                this.Value = this.GetMediaPath();
                this.SetModified();
                this.Update();
                return;
            }
        }

        protected void Update(bool showCropper = true)
        {
            string str;
            this.GetSrc(out str);
            SheerResponse.SetAttribute(string.Concat(this.ID, "_image"), "src", str);
            //SheerResponse.SetInnerHtml(string.Concat(this.ID, "_details"), this.GetDetails());

            var appDomain = AppDomain.CurrentDomain;
            var basePath = appDomain.BaseDirectory;
            var detailPath = Path.Combine(basePath, ASSETS_FOLDER_NAME, "detail.html");

            var imagesHtml = GetThumbnails();
            var html = System.IO.File.ReadAllText(detailPath)
                .Replace("{CONTROL_ID}", this.ID)
                .Replace("{IMAGE_DETAILS}", this.GetDetails())
                .Replace("{THUMBNAILS}", showCropper ? imagesHtml : string.Empty);

            SheerResponse.SetInnerHtml(string.Concat(this.ID, "_details"), html);
            SheerResponse.Eval("scContent.startValidators()");
        }


        private void ClearImage()
        {
            if (this.Disabled)
            {
                return;
            }
            if (this.Value.Length > 0)
            {
                this.SetModified();
            }
            this.XmlValue = new XmlValue(string.Empty, "image");
            this.Value = string.Empty;
            this.Update();
        }

        private string GetDetails()
        {
            string empty = string.Empty;
            MediaItem mediaItem = this.GetMediaItem();
            if (mediaItem != null)
            {
                Item innerItem = mediaItem.InnerItem;
                StringBuilder stringBuilder = new StringBuilder();
                XmlValue xmlValue = this.XmlValue;
                stringBuilder.Append("<div>");
                string item = innerItem["Dimensions"];
                string str = HttpUtility.HtmlEncode(xmlValue.GetAttribute("width"));
                string str1 = HttpUtility.HtmlEncode(xmlValue.GetAttribute("height"));
                if (!string.IsNullOrEmpty(str) || !string.IsNullOrEmpty(str1))
                {
                    object[] objArray = new object[] { str, str1, item };
                    stringBuilder.Append(Translate.Text("Dimensions: {0} x {1} (Original: {2})", objArray));
                }
                else
                {
                    object[] objArray1 = new object[] { item };
                    stringBuilder.Append(Translate.Text("Dimensions: {0}", objArray1));
                }
                stringBuilder.Append("</div>");
                stringBuilder.Append("<div style=\"padding:2px 0px 0px 0px\">");
                string str2 = HttpUtility.HtmlEncode(innerItem["Alt"]);
                string str3 = HttpUtility.HtmlEncode(xmlValue.GetAttribute("alt"));
                if (!string.IsNullOrEmpty(str3) && !string.IsNullOrEmpty(str2))
                {
                    object[] objArray2 = new object[] { str3, str2 };
                    stringBuilder.Append(Translate.Text("Alternate Text: \"{0}\" (Default Alternate Text: \"{1}\")", objArray2));
                }
                else if (!string.IsNullOrEmpty(str3))
                {
                    object[] objArray3 = new object[] { str3 };
                    stringBuilder.Append(Translate.Text("Alternate Text: \"{0}\"", objArray3));
                }
                else if (string.IsNullOrEmpty(str2))
                {
                    stringBuilder.Append(Translate.Text("Warning: Alternate Text is missing."));
                }
                else
                {
                    object[] objArray4 = new object[] { str2 };
                    stringBuilder.Append(Translate.Text("Default Alternate Text: \"{0}\"", objArray4));
                }
                stringBuilder.Append("</div>");
                empty = stringBuilder.ToString();
            }
            if (empty.Length == 0)
            {
                empty = Translate.Text("This media item has no details.");
            }
            return empty;
        }
        
        private Item GetMediaItem()
        {
            string attribute = this.XmlValue.GetAttribute("mediaid");
            if (attribute.Length <= 0)
            {
                return null;
            }
            Language language = Language.Parse(this.ItemLanguage);
            return Client.ContentDatabase.GetItem(attribute, language);
        }

        private string GetMediaPath()
        {
            MediaItem mediaItem = this.GetMediaItem();
            if (mediaItem == null)
            {
                return string.Empty;
            }
            return mediaItem.MediaPath;
        }

        private void GetSrc(out string src)
        {
            int num;
            src = string.Empty;
            MediaItem mediaItem = this.GetMediaItem();
            if (mediaItem == null)
            {
                return;
            }
            MediaUrlOptions thumbnailOptions = MediaUrlOptions.GetThumbnailOptions(mediaItem);
            if (!int.TryParse(mediaItem.InnerItem["Height"], out num))
            {
                num = 128;
            }
            thumbnailOptions.Height = Math.Min(128, num);
            thumbnailOptions.MaxWidth = 640;
            thumbnailOptions.UseDefaultIcon = true;
            src = MediaManager.GetMediaUrl(mediaItem, thumbnailOptions);
        }

        private bool IsImageMedia(TemplateItem template)
        {
            Assert.ArgumentNotNull(template, "template");
            if (template.ID == TemplateIDs.VersionedImage || template.ID == TemplateIDs.UnversionedImage)
            {
                return true;
            }
            TemplateItem[] baseTemplates = template.BaseTemplates;
            for (int i = 0; i < (int)baseTemplates.Length; i++)
            {
                if (this.IsImageMedia(baseTemplates[i]))
                {
                    return true;
                }
            }
            return false;
        }

        private void ParseParameters(string source)
        {
            var parameters = new UrlString(source);

            // SET THUMBNAIL FOLDER ID BY DEFAULT FROM SETTINGS, OR TAKE FROM FIELD SOURCE (IF DEFINED).
            ThumbnailsFolderID = Settings.GetSetting("Sitecore.Framework.Fields.AdvanceImageField.DefaultThumbnailFolderId");

            if (!Sitecore.Data.ID.IsID(ThumbnailsFolderID) &&
                !string.IsNullOrEmpty(parameters.Parameters[THUMBNAIL_FOLDER_FIELD_NAME]) &&
                Sitecore.Data.ID.IsID(parameters.Parameters[THUMBNAIL_FOLDER_FIELD_NAME]))
            {
                ThumbnailsFolderID = parameters.Parameters[THUMBNAIL_FOLDER_FIELD_NAME];
            }

            // WHETHER TO SHOW RAW VALUES
            if (!string.IsNullOrEmpty(parameters.Parameters[IS_DEBUG_FIELD_NAME]))
            {
                IsDebug = parameters.Parameters[IS_DEBUG_FIELD_NAME];
            }
        }

        private string GetThumbnails()
        {
            var html = string.Empty;
            var src = string.Empty;

            this.GetSrc(out src);
            ParseParameters(Source);

            if (!string.IsNullOrEmpty(ThumbnailsFolderID) && !string.IsNullOrEmpty(src))
            {
                var thumbnailFolderItem = Client.ContentDatabase.GetItem(new Sitecore.Data.ID(ThumbnailsFolderID));
                if (thumbnailFolderItem != null && thumbnailFolderItem.HasChildren)
                {
                    foreach (Item item in thumbnailFolderItem.Children)
                    {
                        if (item.Fields["Width"] != null && item.Fields["Height"] != null)
                        {
                            var width = item["Width"];
                            var height = item["Height"];

                            int w, h;

                            if (Int32.TryParse(width, out w) && Int32.TryParse(height, out h) && w > 0 && h > 0)
                            {
                                html += string.Format("<li id=\"Frame_{0}_{1}\" class=\"focuspoint focal-frame\" style=\"width: {2}px; height: {3}px;\"><img /><span>{2}x{3}</span></li>", this.ID, item.ID.ToShortID(), w, h);
                            }
                        }
                    }
                }
            }

            return html;
        }
    }
}