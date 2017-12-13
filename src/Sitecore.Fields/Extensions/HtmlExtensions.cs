namespace Sitecore.Framework.Fields.Extensions
{
  using System.Web;
  using System.Web.Mvc;
  using System.Xml;
  using Data.Fields;
  using Data.Items;
  using Mvc.Helpers;
  using Resources.Media;

  public static class HtmlHelperExtensions
  {
    public static HtmlString AdvancedImageField(this SitecoreHelper helper, string fieldName, Item item, int height = 100, int width = 100, string cssClass = null, bool disableWebEditing = false)
    {
      /*
       Example usage in view @Html.Sitecore().AdvancedImageField("Cropped", Model.Item, 100, 400)

       * */
      if (item == null)
        return new HtmlString("No datasource set");

      var imageField = new ImageField(item.Fields[fieldName]);

      var xml = new XmlDocument();
      xml.LoadXml(imageField.Value);

      if (xml.DocumentElement == null) return new HtmlString("No cropping image parameters found.");

      var cropx = xml.DocumentElement.HasAttribute("cropx") ? xml.DocumentElement.GetAttribute("cropx") : string.Empty;
      var cropy = xml.DocumentElement.HasAttribute("cropy") ? xml.DocumentElement.GetAttribute("cropy") : string.Empty;
      var focusx = xml.DocumentElement.HasAttribute("focusx") ? xml.DocumentElement.GetAttribute("focusx") : string.Empty;
      var focusy = xml.DocumentElement.HasAttribute("focusy") ? xml.DocumentElement.GetAttribute("focusy") : string.Empty;

      float cx, cy, fx, fy;
      float.TryParse(cropx, out cx);
      float.TryParse(cropy, out cy);
      float.TryParse(focusx, out fx);
      float.TryParse(focusy, out fy);

      var imageSrc = MediaManager.GetMediaUrl(imageField.MediaItem);

      var src = $"{imageSrc}?cx={cx}&cy={cy}&cw={width}&ch={height}";

      var hash = HashingUtils.GetAssetUrlHash(src);

      var builder = new TagBuilder("img");
      builder.Attributes.Add("src", $"{src}&hash={hash}");
      builder.Attributes.Add("alt", "");

      if (!string.IsNullOrEmpty(cssClass))
        builder.AddCssClass(cssClass);

      return MvcHtmlString.Create(builder.ToString(TagRenderMode.Normal));

    }
  }
}
