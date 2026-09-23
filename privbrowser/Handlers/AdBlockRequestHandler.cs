using System;
using System.Collections.Generic;
using System.Linq;
using CefSharp;

namespace PrivBrowser.Handlers
{
    public class AdBlockRequestHandler : IRequestHandler
    {
        private static readonly HashSet<string> BlockedDomains = new(StringComparer.OrdinalIgnoreCase)
        {
            "google-analytics.com",
            "analytics.google.com",
            "doubleclick.net",
            "googlesyndication.com",
            "adservice.google.com",
            "facebook.net/connect",
            "pixel.facebook.com",
            "scorecardresearch.com",
            "quantserve.com",
            "outbrain.com",
            "taboola.com",
            "adnxs.com",
            "criteo.com",
            "telemetry.microsoft.com"
        };

        public bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect) => false;
        public bool OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture) => false;

        public IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            if (Uri.TryCreate(request.Url, UriKind.Absolute, out Uri? uri))
            {
                if (BlockedDomains.Any(domain => uri.Host.EndsWith(domain, StringComparison.OrdinalIgnoreCase)))
                {
                    return new BlockResourceRequestHandler();
                }
            }
            return null!;
        }

        public bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback) => true;
        public bool OnQuotaRequest(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, long newSize, IRequestCallback callback) => true;
        public bool OnCertificateError(IWebBrowser chromiumWebBrowser, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback) => false;
        public void OnRenderProcessTerminated(IWebBrowser chromiumWebBrowser, IBrowser browser, CefTerminationStatus status) { }
        public void OnDocumentAvailableInMainFrame(IWebBrowser chromiumWebBrowser, IBrowser browser) { }
    }

    public class BlockResourceRequestHandler : ResourceRequestHandler
    {
        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            return CefReturnValue.Cancel;
        }
    }
}