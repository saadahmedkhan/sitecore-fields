using Glass.Mapper.Sc.Fields;
using Sitecore.Resources.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Framework.Fields.Extensions
{
    public static class GlassMapperExtensions
    {
        public static string GetUrl(this AdvanceImageField imageField, int width = 0, int height = 0)
        {
            if (imageField == null)
            {
                return string.Empty;
            }

            if (width <= 0 || height <= 0)
            {
                return string.Empty;
            }

            var src = string.Format("{0}?cx={1}&cy={2}&cw={3}&ch={4}",
                imageField.Src, imageField.CropX, imageField.CropY, width, height);

            var hash = HashingUtils.GetAssetUrlHash(src);

            return string.Format("{0}&hash={1}", src, hash);
        }
    }
}