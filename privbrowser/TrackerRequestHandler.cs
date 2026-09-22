using System;
using CefSharp;
using CefSharp.Handler;

namespace PrivBrowser
{
    public class TrackerResourceRequestHandler : ResourceRequestHandler
    {
        private readonly BrowserTab _tab;

        public TrackerResourceRequestHandler(BrowserTab tab)
        {
            _tab = tab;
        }

        protected override CefReturnValue OnBeforeResourceLoad(
            IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
            IRequest request, IRequestCallback callback)
        {
            if (!_tab.ShieldDisabledForCurrentSite && TrackerBlockList.IsBlocked(request.Url))
            {
                _tab.BlockedCount++;
                if (Uri.TryCreate(request.Url, UriKind.Absolute, out var blockedUri))
                {
                    _tab.BlockedDomainCounts.TryGetValue(blockedUri.Host, out var current);
                    _tab.BlockedDomainCounts[blockedUri.Host] = current + 1;
                }
                _tab.OnBlockedCountChanged?.Invoke();
                return CefReturnValue.Cancel;
            }
            return CefReturnValue.Continue;
        }
    }

    public class TrackerRequestHandler : RequestHandler
    {
        private readonly BrowserTab _tab;

        public TrackerRequestHandler(BrowserTab tab)
        {
            _tab = tab;
        }

        protected override IResourceRequestHandler GetResourceRequestHandler(
            IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request,
            bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return new TrackerResourceRequestHandler(_tab);
        }

        protected override bool OnBeforeBrowse(
            IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
            IRequest request, bool userGesture, bool isRedirect)
        {
            if (request.Url.StartsWith("http://"))
            {
                var httpsUrl = "https://" + request.Url.Substring("http://".Length);
                frame.LoadUrl(httpsUrl);
                return true;
            }
            return false;
        }

        protected override bool GetAuthCredentials(
            IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl,
            bool isProxy, string host, int port, string realm, string scheme,
            IAuthCallback callback)
        {
            if (!isProxy)
            {
                callback.Dispose();
                return false;
            }

            var config = VpnSettingsStore.Load();
            if (config.Enabled && !string.IsNullOrEmpty(config.Username))
            {
                callback.Continue(config.Username, config.Password);
                return true;
            }

            callback.Dispose();
            return false;
        }
    }
}
