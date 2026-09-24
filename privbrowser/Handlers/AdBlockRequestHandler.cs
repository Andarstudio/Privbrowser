using System;
using CefSharp;
using CefSharp.Handler;

namespace PrivBrowser.Handlers
{
    public class AdBlockRequestHandler : RequestHandler
    {
        public bool TrackerProtectionEnabled { get; set; } = true;

        protected override IResourceRequestHandler GetResourceRequestHandler(
            IWebBrowser chromiumWebBrowser,
            IBrowser browser,
            IFrame frame,
            IRequest request,
            bool isNavigation,
            bool isDownload,
            string requestInitiator,
            ref bool disableDefaultHandling)
        {
            return new AdBlockResourceRequestHandler(TrackerProtectionEnabled);
        }

        private class AdBlockResourceRequestHandler : ResourceRequestHandler
        {
            private readonly bool _enabled;
            private static readonly string[] BlockedDomains = new[] { "analytics.", "telemetry.", "doubleclick.net", "adservice." };

            public AdBlockResourceRequestHandler(bool enabled)
            {
                _enabled = enabled;
            }

            protected override CefReturnValue OnBeforeResourceLoad(
                IWebBrowser chromiumWebBrowser,
                IBrowser browser,
                IFrame frame,
                IRequest request,
                IRequestCallback callback)
            {
                if (_enabled)
                {
                    string url = request.Url.ToLowerInvariant();
                    foreach (var domain in BlockedDomains)
                    {
                        if (url.Contains(domain))
                        {
                            return CefReturnValue.Cancel;
                        }
                    }
                }
                return CefReturnValue.Continue;
            }
        }
    }
}