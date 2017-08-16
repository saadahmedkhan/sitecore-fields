using Glass.Mapper.Sc.Configuration.Attributes;
using Glass.Mapper.Sc.Fields;

namespace Sitecore.Framework.Fields.Models
{
    public class PageModel
    {
        [SitecoreField("Title")]
        public virtual string Title { get; set; }
        
        [SitecoreField("Background Image")]
        public virtual AdvanceImageField BackgroundImage { get; set; }
    }
}