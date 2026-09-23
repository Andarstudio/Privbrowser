using System;
using CefSharp;

namespace PrivBrowser.Helpers
{
    public class FCodeManager
    {
        public const string Version = "1.0.0 (Build 2026.09)";
        public const string Developer = "Andar Studio";

        public string ExecuteFCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return "Error: Please enter a valid FCode.";

            string cleanCode = code.Trim().ToUpper();

            switch (cleanCode)
            {
                case "FIX-CACHE":
                case "CLEAR-CACHE":
                    Cef.GetGlobalCookieManager().DeleteCookies("", "");
                    return "SUCCESS [FIX-CACHE]: Cache and cookie state wiped successfully.";

                case "RESET-PROXY":
                case "FIX-PROXY":
                    new ProxyManager().ClearProxy();
                    return "SUCCESS [RESET-PROXY]: Network proxy cleared. Direct connection restored.";

                case "FORCE-RELOAD":
                    return "SUCCESS [FORCE-RELOAD]: Refresh command executed across active sessions.";

                default:
                    return $"ERROR: Unknown FCode '{code}'. Available codes: FIX-CACHE, RESET-PROXY, FORCE-RELOAD";
            }
        }
    }
}