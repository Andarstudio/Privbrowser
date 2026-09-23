using System.IO;
using System.Text;
using CefSharp;
using PrivBrowser.Helpers;

namespace PrivBrowser.Handlers
{
    public class PrivBrowserSchemeHandlerFactory : ISchemeHandlerFactory
    {
        public IResourceHandler Create(IWebBrowser chromiumWebBrowser, IBrowser browser, string schemeName, IRequest request)
        {
            var uri = new System.Uri(request.Url);
            string html = uri.Host.ToLower() switch
            {
                "settings" => LocalPageGenerator.GetSettingsPageHtml(),
                _ => LocalPageGenerator.GetHomePageHtml()
            };

            byte[] bytes = Encoding.UTF8.GetBytes(html);
            var stream = new MemoryStream(bytes);
            return ResourceHandler.FromStream(stream, "text/html");
        }
    }
}