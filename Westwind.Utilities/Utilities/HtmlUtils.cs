using System;
using System.Text.RegularExpressions;


namespace Westwind.Utilities
{
    /// <summary>
    /// Html string and formatting utilities
    /// </summary>
    public static class HtmlUtils
    {
        /// <summary>
        /// Replaces and  and Quote characters to HTML safe equivalents.
        /// </summary>
        /// <param name="html">HTML to convert</param>
        /// <returns>Returns an HTML string of the converted text</returns>
        public static string FixHTMLForDisplay(string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            html = html.Replace("<", "&lt;");
            html = html.Replace(">", "&gt;");
            html = html.Replace("\"", "&quot;");
            return html;
        }

        /// <summary>
        /// Strips HTML tags out of an HTML string and returns just the text.
        /// </summary>
        /// <param name="html">Html String</param>
        /// <returns></returns>
        public static string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            html = Regex.Replace(html, @"<(.|\n)*?>", string.Empty);
            html = html.Replace("\t", " ");
            html = html.Replace("\r\n", string.Empty);
            html = html.Replace("   ", " ");
            return html.Replace("  ", " ");
        }

        /// <summary>
        /// Fixes a plain text field for display as HTML by replacing carriage returns 
        /// with the appropriate br and p tags for breaks.
        /// </summary>
        /// <param name="htmlText">Input string</param>
        /// <returns>Fixed up string</returns>
        public static string DisplayMemo(string htmlText)
        {
            if (htmlText == null)
                return string.Empty;

            htmlText = htmlText.Replace("\r\n", "\r");
            htmlText = htmlText.Replace("\n", "\r");
            //HtmlText = HtmlText.Replace("\r\r","<p>");
            htmlText = htmlText.Replace("\r", "<br />\r\n");
            return htmlText;
        }

        /// <summary>
        /// Method that handles handles display of text by breaking text.
        /// Unlike the non-encoded version it encodes any embedded HTML text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string DisplayMemoEncoded(string text)
        {
            if (text == null)
                return string.Empty;

            bool PreTag = false;
            if (text.Contains("<pre>"))
            {
                text = text.Replace("<pre>", "__pre__");
                text = text.Replace("</pre>", "__/pre__");
                PreTag = true;
            }

            // fix up line breaks into <br><p>
            text = DisplayMemo(System.Net.WebUtility.HtmlEncode(text)); //HttpUtility.HtmlEncode(Text));

            if (PreTag)
            {
                text = text.Replace("__pre__", "<pre>");
                text = text.Replace("__/pre__", "</pre>");
            }

            return text;
        }

        /// <summary>
        /// HTML-encodes a string and returns the encoded string.
        /// </summary>
        /// <param name="text">The text string to encode. </param>
        /// <returns>The HTML-encoded text.</returns>
        [Obsolete("Use System.Net.WebUtility.HtmlEncode() instead.")]
        public static string HtmlEncode(string text)
        {
            return System.Net.WebUtility.HtmlEncode(text);
            //if (text == null)
            //    return string.Empty;

            //StringBuilder sb = new StringBuilder(text.Length);

            //int len = text.Length;
            //for (int i = 0; i < len; i++)
            //{
            //    switch (text[i])
            //    {

            //        case '<':
            //            sb.Append("&lt;");
            //            break;
            //        case '>':
            //            sb.Append("&gt;");
            //            break;
            //        case '"':
            //            sb.Append("&quot;");
            //            break;
            //        case '&':
            //            sb.Append("&amp;");
            //            break;
            //        case '\'':
            //            sb.Append("&#39;");
            //            break;				
            //        default:
            //            if (text[i] > 159)
            //            {
            //                // decimal numeric entity
            //                sb.Append("&#");
            //                sb.Append(((int)text[i]).ToString(CultureInfo.InvariantCulture));
            //                sb.Append(";");
            //            }
            //            else
            //                sb.Append(text[i]);
            //            break;
            //    }
            //}
            //return sb.ToString();
        }

        /// <summary>
        /// Create an embedded image url for binary data like images and media
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        public static string BinaryToEmbeddedBase64(byte[] imageBytes, string mimeType = "image/png")
        {
            var data = $"data:{mimeType};base64," + Convert.ToBase64String(imageBytes, 0, imageBytes.Length);
            return data;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Decoded an embedded base64 resource string into its binary content and mime type
        /// </summary>
        /// <param name="base64Data">Embedded Base64 data (data:mime/type;b64data) </param>
        /// <returns></returns>
        public static (byte[] bytes, string mimeType) EmbeddedBase64ToBinary(string base64Data)
        {
            if (string.IsNullOrEmpty(base64Data))
                return (null, null);

            var parts = base64Data.Split(',');
            if (parts.Length != 2)
                return (null, null);

            var mimeType = parts[0].Replace("data:", "").Replace(";base64", "");
            var data = parts[1];

            var bytes = Convert.FromBase64String(data);

            return (bytes, mimeType);
        }
#endif        

        /// <summary>
        /// Creates an Abstract from an HTML document. Strips the 
        /// HTML into plain text, then creates an abstract.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string HtmlAbstract(string html, int length)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;    
            return StringUtils.TextAbstract(StripHtml(html), length);
        }


        static string DefaultHtmlSanitizeTagBlackList { get; } = "script|iframe|object|embed|form";

        static Regex _RegExScript = new Regex($@"(<({DefaultHtmlSanitizeTagBlackList})\b[^<]*(?:(?!<\/({DefaultHtmlSanitizeTagBlackList}))<[^<]*)*<\/({DefaultHtmlSanitizeTagBlackList})>)",
        RegexOptions.IgnoreCase | RegexOptions.Multiline);

        // strip javascript: and unicode representation of javascript:
        // href='javascript:alert(\"gotcha\")'
        // href='&#106;&#97;&#118;&#97;&#115;&#99;&#114;&#105;&#112;&#116;:alert(\"gotcha\");'
        static Regex _RegExJavaScriptHref = new Regex(
            @"<[^>]*?\s(href|src|dynsrc|lowsrc)=.{0,20}((javascript:)|(&#)).*?>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        static Regex _RegExOnEventAttributes = new Regex(
            @"<[^>]*?\s(on[^\s\\]{0,20}=([""].*?[""]|['].*?['])).*?(>|\/>)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Sanitizes HTML to some of the most of 
        /// </summary>
        /// <remarks>
        /// This provides rudimentary HTML sanitation catching the most obvious
        /// XSS script attack vectors. For mroe complete HTML Sanitation please look into
        /// a dedicated HTML Sanitizer.
        /// </remarks>
        /// <param name="html">input html</param>
        /// <param name="htmlTagBlacklist">A list of HTML tags that are stripped.</param>
        /// <returns>Sanitized HTML</returns>
        public static string SanitizeHtml(string html, string htmlTagBlacklist = "script|iframe|object|embed|form")
        {
            if (string.IsNullOrEmpty(html))
                return html;

            if (string.IsNullOrEmpty(htmlTagBlacklist) || htmlTagBlacklist == DefaultHtmlSanitizeTagBlackList)
            {
                // Use the default list of tags Replace Script tags - reused expr is more efficient
                html = _RegExScript.Replace(html, string.Empty);
            }
            else
            {
                // create a custom list including provided tags
                html = Regex.Replace(html,
                                        $@"(<({htmlTagBlacklist})\b[^<]*(?:(?!<\/({DefaultHtmlSanitizeTagBlackList}))<[^<]*)*<\/({htmlTagBlacklist})>)",
                                        "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            }

            // Remove javascript: directives
            var matches = _RegExJavaScriptHref.Matches(html);
            foreach (Match match in matches)
            {
                if (match.Groups.Count > 2)
                {
                    var txt = match.Value.Replace(match.Groups[2].Value, "unsupported:");
                    html = html.Replace(match.Value, txt);
                }
            }

            // Remove onEvent handlers from elements
            matches = _RegExOnEventAttributes.Matches(html);
            foreach (Match match in matches)
            {
                var txt = match.Value;
                if (match.Groups.Count > 1)
                {
                    var onEvent = match.Groups[1].Value;
                    txt = txt.Replace(onEvent, string.Empty);
                    if (!string.IsNullOrEmpty(txt))
                        html = html.Replace(match.Value, txt);
                }
            }

            return html;
        }

        /// <summary>
        /// Create an Href HTML link.
        /// 
        /// 
        /// <remarks>
        /// For NetFx in ASPX applications ~ resolves to the application root
        /// </remarks>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="url"></param>
        /// <param name="target"></param>
        /// <param name="additionalMarkup"></param>
        /// <returns></returns>
        public static string Href(string text, string url, string target = null, string additionalMarkup = null)
        {

#if NETFULL
            if (url.StartsWith("~"))
                url = ResolveUrl(url);
#endif

            return "<a href=\"" + url + "\" " +
                   (string.IsNullOrEmpty(target) ? string.Empty : "target=\"" + target + "\" ") +
                   (string.IsNullOrEmpty(additionalMarkup) ? string.Empty : additionalMarkup) +
                   ">" + text + "</a>";
        }

        /// <summary>
        /// Creates an HREF HTML Link
        /// </summary>
        /// <param name="url"></param>
        public static string Href(string url)
        {
            return Href(url, url, null, null);
        }

        /// <summary>
        /// Returns an IMG link as a string. If the image is null
        /// or empty a blank string is returned.
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <param name="additionalMarkup">any additional attributes added to the element</param>
        /// <returns></returns>
        public static string ImgRef(string imageUrl, string additionalMarkup = null)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return string.Empty;

#if NETFULL
            if (imageUrl.StartsWith("~"))
                imageUrl = ResolveUrl(imageUrl);
#endif

            string img = "<img src=\"" + imageUrl + "\" ";

            if (!string.IsNullOrEmpty("additionalMarkup"))
                img += additionalMarkup + " ";

            img += "/>";
            return img;
        }

#if NETFULL
        /// <summary>
        /// Resolves a URL based on the current HTTPContext
        /// 
        /// Note this method is added here internally only
        /// to support the HREF() method and ~ expansion
        /// on urls.
        /// </summary>
        /// <param name="originalUrl"></param>
        /// <returns></returns>
        public static string ResolveUrl(string originalUrl)
        {
            if (string.IsNullOrEmpty(originalUrl))
                return string.Empty;

            // Absolute path - just return
            if (originalUrl.IndexOf("://") != -1)
                return originalUrl;

            // Fix up image path for ~ root app dir directory
            if (originalUrl.StartsWith("~"))
            {
                //return VirtualPathUtility.ToAbsolute(originalUrl);
                string newUrl = "";

                // Avoid pulling in System.Web reference
                dynamic context = ReflectionUtils.GetStaticProperty( "System.Web.HttpContext", "Current");
                if (context != null)
                {
                    newUrl = context.Request.ApplicationPath +
                          originalUrl.Substring(1);
                    newUrl = newUrl.Replace("//", "/"); // must fix up for root path
                }
                else
                    // Not context: assume current directory is the base directory
                    throw new ArgumentException("Invalid URL: Relative Urls are only available in classic ASP.NET Web applications.");

                // Just to be sure fix up any double slashes
                return newUrl;
            }

            return originalUrl;
        }
#endif
        #region Url Parsing

        /// <summary>
        /// Returns the base URL of a site from a full URL
        /// </summary>
        /// <param name="url">An absolute scheme path (http:// or file:// or ftp:// etc) </param>
        /// <returns></returns>
        public static string GetSiteBasePath(string url)
        {
            var uri = new Uri(url);
            return $"{uri.Scheme}://{uri.Authority}/";
        }

        /// <summary>
        /// Returns the path portion of an absolute URL optionally with the query string and fragment
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pathOptions"></param>
        /// <returns></returns>
        public static string GetRelativeUrlPath(string url, PathReturnOptions pathOptions = PathReturnOptions.PathOnly )
        {
            var uri = new Uri(url);

            switch (pathOptions)
            {
                case PathReturnOptions.PathOnly:
                    return uri.AbsolutePath;
                case PathReturnOptions.PathAndQuery:
                    return uri.PathAndQuery;
                case PathReturnOptions.PathAndHash:
                    return uri.AbsolutePath + uri.Fragment;
                case PathReturnOptions.PathAndQueryAndHash:
                    return uri.PathAndQuery + uri.Fragment;
            }

            return uri.PathAndQuery;
        }

        #endregion
    }

    public enum PathReturnOptions
    {
        PathOnly,
        PathAndQuery,
        PathAndHash,
        PathAndQueryAndHash,        
    }
}