using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Resources.Media;
using System.Web;

namespace Sitecore.Resources.Media
{
    /// <summary>
    /// This class ensures that additional custom parameters for focus cropping are cached in .ini file inside App_Data folder.
    /// </summary>
    public class CropMediaRequest : MediaRequest
    {
        private HttpRequest innerRequest;
        private MediaUrlOptions mediaQueryString;
        private MediaUri mediaUri;
        private MediaOptions options;

        protected override MediaOptions GetOptions()
        {
            var queryString = this.InnerRequest.QueryString;

            if (queryString != null && !string.IsNullOrEmpty(queryString.Get("cx")))
            {
                options = new MediaOptions();

                this.ProcessCustomParameters(options);

                if (!options.CustomOptions.ContainsKey("cx") && !string.IsNullOrEmpty(queryString.Get("cx")))
                {
                    options.CustomOptions.Add("cx", queryString.Get("cx"));
                }

                if (!options.CustomOptions.ContainsKey("cy") && !string.IsNullOrEmpty(queryString.Get("cy")))
                {
                    options.CustomOptions.Add("cy", queryString.Get("cy"));
                }

                if (!options.CustomOptions.ContainsKey("cw") && !string.IsNullOrEmpty(queryString.Get("cw")))
                {
                    options.CustomOptions.Add("cw", queryString.Get("cw"));
                }

                if (!options.CustomOptions.ContainsKey("ch") && !string.IsNullOrEmpty(queryString.Get("ch")))
                {
                    options.CustomOptions.Add("ch", queryString.Get("ch"));
                }
            }
            else
            {
                MediaUrlOptions mediaQueryString = this.GetMediaQueryString();
                options = new MediaOptions
                {
                    AllowStretch = mediaQueryString.AllowStretch,
                    BackgroundColor = mediaQueryString.BackgroundColor,
                    IgnoreAspectRatio = mediaQueryString.IgnoreAspectRatio,
                    Scale = mediaQueryString.Scale,
                    Width = mediaQueryString.Width,
                    Height = mediaQueryString.Height,
                    MaxWidth = mediaQueryString.MaxWidth,
                    MaxHeight = mediaQueryString.MaxHeight,
                    Thumbnail = mediaQueryString.Thumbnail
                };
                if (mediaQueryString.DisableMediaCache)
                {
                    options.UseMediaCache = false;
                }
                string[] strArray = queryString.AllKeys;
                for (int i = 0; i < strArray.Length; i = (int)(i + 1))
                {
                    string str = strArray[i];
                    if ((str != null) && (queryString.Get(str) != null))
                    {
                        options.CustomOptions[str] = queryString.Get(str);
                    }
                }
            }
            if (!this.IsRawUrlSafe)
            {
                if (Settings.Media.RequestProtection.LoggingEnabled)
                {
                    string urlReferrer = this.GetUrlReferrer();
                    Log.SingleError(string.Format("MediaRequestProtection: An invalid/missing hash value was encountered. The expected hash value: {0}. Media URL: {1}, Referring URL: {2}",
                        HashingUtils.GetAssetUrlHash(this.InnerRequest.Path),
                        this.InnerRequest.Path,
                        string.IsNullOrEmpty(urlReferrer) ? ((object)"(empty)") : ((object)urlReferrer)),
                        this);
                }
                options = new MediaOptions();
            }


            return options;
        }

        public override MediaRequest Clone()
        {
            Assert.IsTrue((bool)(base.GetType() == typeof(CropMediaRequest)), "The Clone() method must be overridden to support prototyping.");
            return new CropMediaRequest
            {
                innerRequest = this.innerRequest,
                mediaUri = this.mediaUri,
                options = this.options,
                mediaQueryString = this.mediaQueryString
            };
        }
    }
}