namespace PrivBrowser.Helpers
{
    public static class LocalPageGenerator
    {
        public static string GetHomePageHtml()
        {
            return @"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <title>PrivBrowser - Home</title>
    <style>
        body { background-color: #121212; color: #E0E0E0; font-family: 'Segoe UI', Tahoma, sans-serif; display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100vh; margin: 0; }
        .logo { font-size: 42px; font-weight: bold; margin-bottom: 8px; color: #4FC3F7; }
        .sub { font-size: 14px; color: #888; margin-bottom: 30px; }
        .search-box { display: flex; width: 550px; background: #1E1E1E; border: 1px solid #333; border-radius: 24px; padding: 6px 16px; box-shadow: 0 4px 12px rgba(0,0,0,0.3); }
        .search-box input { flex: 1; background: transparent; border: none; color: #FFF; font-size: 16px; outline: none; padding: 8px; }
        .search-box button { background: #0288D1; border: none; color: white; border-radius: 16px; padding: 8px 18px; cursor: pointer; font-weight: bold; }
        .search-box button:hover { background: #039BE5; }
        .shortcuts { display: flex; gap: 20px; margin-top: 40px; }
        .card { background: #1E1E1E; border: 1px solid #2C2C2C; padding: 16px 24px; border-radius: 12px; text-decoration: none; color: #E0E0E0; text-align: center; width: 100px; transition: 0.2s; }
        .card:hover { background: #2A2A2A; border-color: #0288D1; transform: translateY(-2px); }
        .footer { position: absolute; bottom: 20px; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class='logo'>PrivBrowser</div>
    <div class='sub'>Private & Lightweight Browsing by Andar Studio</div>
    
    <form class='search-box' action='https://duckduckgo.com/' method='get'>
        <input type='text' name='q' placeholder='Search privately or enter URL...' autofocus required>
        <button type='submit'>Search</button>
    </form>

    <div class='shortcuts'>
        <a class='card' href='https://duckduckgo.com'>DuckDuckGo</a>
        <a class='card' href='https://wikipedia.org'>Wikipedia</a>
        <a class='card' href='https://github.com/Andarstudio/Privbrowser'>GitHub</a>
    </div>

    <div class='footer'>PrivBrowser reduces tracking where practical. Absolute anonymity is not guaranteed on the web.</div>
</body>
</html>";
        }

        public static string GetSettingsPageHtml()
        {
            return @"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>PrivBrowser - Settings & About</title>
    <style>
        body { background: #121212; color: #E0E0E0; font-family: 'Segoe UI', sans-serif; padding: 40px; max-width: 800px; margin: auto; }
        h1 { border-bottom: 1px solid #333; padding-bottom: 10px; color: #4FC3F7; }
        .section { background: #1E1E1E; padding: 20px; border-radius: 8px; margin-bottom: 20px; border: 1px solid #2A2A2A; }
        h2 { margin-top: 0; font-size: 18px; color: #FFF; }
        p { color: #AAA; font-size: 14px; line-height: 1.5; }
        .form-group { display: flex; gap: 10px; margin-bottom: 12px; align-items: center; }
        label { width: 110px; font-size: 14px; color: #CCC; }
        input, select { background: #2A2A2D; border: 1px solid #444; color: #FFF; padding: 8px 12px; border-radius: 4px; outline: none; }
        input[type='text'] { flex: 1; }
        .btn { background: #0288D1; border: none; color: white; padding: 8px 18px; border-radius: 4px; cursor: pointer; font-weight: bold; }
        .btn:hover { background: #039BE5; }
        .btn-fix { background: #7B1FA2; }
        .btn-fix:hover { background: #8E24AA; }
        .status { margin-top: 10px; font-size: 13px; font-weight: bold; }
        .badge { background: #2A2A2D; padding: 2px 8px; border-radius: 4px; border: 1px solid #444; font-family: monospace; color: #4FC3F7; }
    </style>
</head>
<body>
    <h1>PrivBrowser Settings</h1>

    <div class='section'>
        <h2>About PrivBrowser</h2>
        <p><strong>Version:</strong> <span class='badge'>1.0.0 (Build 2026.09)</span></p>
        <p><strong>Developer:</strong> Andar Studio</p>
        <p><strong>GitHub:</strong> <a href='https://github.com/Andarstudio/Privbrowser' style='color: #4FC3F7;'>github.com/Andarstudio/Privbrowser</a></p>
        <p><strong>Contact:</strong> erdenemandakhamartuvshin@outlook.com</p>
    </div>

    <div class='section'>
        <h2>Diagnostic FCode (Fix Tool)</h2>
        <p>Enter an FCode to fix browser state issues, reset network routes, or clear cache overrides.</p>
        <div class='form-group'>
            <label>Enter FCode:</label>
            <input type='text' id='fcodeInput' placeholder='e.g., FIX-CACHE, RESET-PROXY, FORCE-RELOAD'>
            <button class='btn btn-fix' onclick='runFCode()'>Execute FCode</button>
        </div>
        <div id='fcodeStatus' class='status'></div>
    </div>

    <div class='section'>
        <h2>Proxy / Custom VPN Route</h2>
        <p>Route browser traffic through a custom HTTP, HTTPS, or SOCKS5 proxy server.</p>
        <div class='form-group'>
            <label>Protocol:</label>
            <select id='proxyProtocol'>
                <option value='http'>HTTP</option>
                <option value='https'>HTTPS</option>
                <option value='socks5'>SOCKS5</option>
            </select>
        </div>
        <div class='form-group'>
            <label>Address / IP:</label>
            <input type='text' id='proxyHost' placeholder='e.g., 127.0.0.1'>
        </div>
        <div class='form-group'>
            <label>Port:</label>
            <input type='text' id='proxyPort' placeholder='e.g., 8080' style='width: 120px;'>
        </div>

        <div style='margin-top: 15px;'>
            <button class='btn' onclick='saveProxy()'>Apply Proxy</button>
        </div>
    </div>

    <script>
        async function runFCode() {
            const input = document.getElementById('fcodeInput').value.trim();
            const status = document.getElementById('fcodeStatus');
            if (!input) {
                status.style.color = '#FF8A80';
                status.innerText = 'Please enter an FCode.';
                return;
            }
            await CefSharp.BindObjectAsync('fcodeBoundObject');
            const result = await fcodeBoundObject.executeFCode(input);
            status.style.color = result.startsWith('SUCCESS') ? '#81C784' : '#FF8A80';
            status.innerText = result;
        }

        async function saveProxy() {
            const host = document.getElementById('proxyHost').value.trim();
            const port = document.getElementById('proxyPort').value.trim();
            const protocol = document.getElementById('proxyProtocol').value;
            await CefSharp.BindObjectAsync('proxyBoundObject');
            await proxyBoundObject.setProxy(host, port, protocol);
        }
    </script>
</body>
</html>";
        }
    }
}