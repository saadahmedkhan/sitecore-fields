using Glass.Mapper.Sc.Configuration;
using Glass.Mapper.Sc.Fields;
using Sitecore.Data.Fields;
using Sitecore.Resources.Media;
using Sitecore.Shell.Applications.ContentEditor;
using System;
using System.Xml;

namespace Glass.Mapper.Sc.DataMappers
{
    public class SitecoreFieldAdvanceImageMapper : AbstractSitecoreFieldMapper
    {
        public SitecoreFieldAdvanceImageMapper()
            : base(typeof(AdvanceImage))
        {
        }

        public override object GetField(Field field, SitecoreFieldConfiguration config, SitecoreDataMappingContext context)
        {
            var scImg = new ImageField(field);
            var img = new AdvanceImageField();

            var xml = new XmlDocument();
            xml.LoadXml(scImg.Value);
            var id = xml.DocumentElement.GetAttribute("mediaid");
            var cropx = xml.DocumentElement.HasAttribute("cropx") ? xml.DocumentElement.GetAttribute("cropx") : string.Empty;
            var cropy = xml.DocumentElement.HasAttribute("cropy") ? xml.DocumentElement.GetAttribute("cropy") : string.Empty;
            var focusx = xml.DocumentElement.HasAttribute("focusx") ? xml.DocumentElement.GetAttribute("focusx") : string.Empty;
            var focusy = xml.DocumentElement.HasAttribute("focusy") ? xml.DocumentElement.GetAttribute("focusy") : string.Empty;

            float cx, cy, fx, fy;
            float.TryParse(cropx, out cx);
            float.TryParse(cropy, out cy);
            float.TryParse(focusx, out fx);
            float.TryParse(focusy, out fy);

            img.CropX = cx;
            img.CropY = cy;
            img.FocusX = fx;
            img.FocusY = fy;
            img.Alt = scImg.Alt;
            img.Border = scImg.Border;
            img.Class = scImg.Class;
            img.Width = Convert.ToInt32(string.IsNullOrEmpty(scImg.Width) ? "0" : scImg.Width);
            img.Height = Convert.ToInt32(string.IsNullOrEmpty(scImg.Height) ? "0" : scImg.Height);
            img.HSpace = Convert.ToInt32(string.IsNullOrEmpty(scImg.HSpace) ? "0" : scImg.HSpace);
            img.Language = scImg.MediaLanguage;
            img.MediaId = scImg.MediaID.ToGuid();
            img.Src = MediaManager.GetMediaUrl(scImg.MediaItem);
            img.VSpace = Convert.ToInt32(string.IsNullOrEmpty(scImg.VSpace) ? "0" : scImg.VSpace);

            return img;
        }

        public override void SetField(Field field, object value, SitecoreFieldConfiguration config, SitecoreDataMappingContext context)
        {
            base.SetField(field, value, config, context);
        }

        public override string SetFieldValue(object value, SitecoreFieldConfiguration config, SitecoreDataMappingContext context)
        {
            throw new NotImplementedException();
        }

        public override object GetFieldValue(string fieldValue, SitecoreFieldConfiguration config, SitecoreDataMappingContext context)
        {
            throw new NotImplementedException();
        }
    }
}