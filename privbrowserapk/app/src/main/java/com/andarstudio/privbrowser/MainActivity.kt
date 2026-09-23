package com.andarstudio.privbrowser

import android.annotation.SuppressLint
import android.net.Uri
import android.os.Bundle
import android.view.View
import android.view.inputmethod.EditorInfo
import android.webkit.WebResourceRequest
import android.webkit.WebResourceResponse
import android.webkit.WebView
import android.webkit.WebViewClient
import android.widget.Button
import android.widget.LinearLayout
import android.widget.PopupMenu
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import androidx.core.content.ContextCompat
import androidx.webkit.ProxyConfig
import androidx.webkit.ProxyController
import androidx.webkit.WebViewFeature
import java.io.File
import java.util.concurrent.Executor

private const val HOME_URL = "https://duckduckgo.com"
private const val SEARCH_URL = "https://duckduckgo.com/?q="

class Tab(val webView: WebView) {
    var title: String = "New tab"
    var blockedCount: Int = 0
    val blockedDomainCounts: MutableMap<String, Int> = mutableMapOf()
    var shieldDisabled: Boolean = false
    var currentHost: String? = null
    lateinit var tabButton: Button
}

class MainActivity : AppCompatActivity() {

    private lateinit var stores: Stores
    private lateinit var tabStripLayout: LinearLayout
    private lateinit var webviewContainer: android.widget.FrameLayout
    private lateinit var addressBar: android.widget.EditText

    private val tabs = mutableListOf<Tab>()
    private var activeTab: Tab? = null

    @SuppressLint("SetJavaScriptEnabled")
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        stores = Stores(this)
        tabStripLayout = findViewById(R.id.tabStripLayout)
        webviewContainer = findViewById(R.id.webviewContainer)
        addressBar = findViewById(R.id.addressBar)

        TrackerBlockList.initialize(File(cacheDir, "FilterLists"))
        applyProxyIfConfigured()

        findViewById<android.widget.ImageButton>(R.id.backBtn).setOnClickListener {
            activeTab?.webView?.let { if (it.canGoBack()) it.goBack() }
        }
        findViewById<android.widget.ImageButton>(R.id.forwardBtn).setOnClickListener {
            activeTab?.webView?.let { if (it.canGoForward()) it.goForward() }
        }
        findViewById<android.widget.ImageButton>(R.id.menuBtn).setOnClickListener { showMainMenu(it) }

        addressBar.setOnEditorActionListener { _, actionId, _ ->
            if (actionId == EditorInfo.IME_ACTION_GO) {
                navigateFromAddressBar()
                true
            } else false
        }

        val savedUrls = stores.loadSession()
        if (savedUrls.isNotEmpty()) savedUrls.forEach { createTab(it) }
        else createTab(HOME_URL)
    }

    override fun onStop() {
        super.onStop()
        stores.saveSession(tabs.map { it.webView.url ?: "" }.filter { it.isNotEmpty() })
    }

    private fun navigateFromAddressBar() {
        val tab = activeTab ?: return
        val input = addressBar.text.toString().trim()
        if (input.isEmpty()) return
        val target = if (looksLikeUrl(input)) upgradeToHttps(normalizeUrl(input))
                     else SEARCH_URL + Uri.encode(input)
        tab.webView.loadUrl(target)
    }

    private fun looksLikeUrl(input: String): Boolean {
        if (input.contains(' ')) return false
        if (input.startsWith("http://") || input.startsWith("https://")) return true
        return input.contains('.')
    }
    private fun normalizeUrl(input: String) =
        if (input.startsWith("http://") || input.startsWith("https://")) input else "https://$input"
    private fun upgradeToHttps(url: String) =
        if (url.startsWith("http://")) "https://" + url.substring("http://".length) else url

    @SuppressLint("SetJavaScriptEnabled")
    private fun createTab(url: String) {
        val webView = WebView(this)
        webView.settings.javaScriptEnabled = true
        webView.settings.domStorageEnabled = true
        webView.layoutParams = android.widget.FrameLayout.LayoutParams(
            android.widget.FrameLayout.LayoutParams.MATCH_PARENT,
            android.widget.FrameLayout.LayoutParams.MATCH_PARENT
        )

        val tab = Tab(webView)

        webView.webViewClient = object : WebViewClient() {
            override fun shouldInterceptRequest(
                view: WebView, request: WebResourceRequest
            ): WebResourceResponse? {
                if (!tab.shieldDisabled && TrackerBlockList.isBlocked(request.url.toString())) {
                    tab.blockedCount++
                    val host = request.url.host
                    if (host != null) {
                        tab.blockedDomainCounts[host] = (tab.blockedDomainCounts[host] ?: 0) + 1
                    }
                    return WebResourceResponse("text/plain", "utf-8", null)
                }
                return null
            }

            override fun shouldOverrideUrlLoading(view: WebView, request: WebResourceRequest): Boolean {
                val url = request.url.toString()
                if (url.startsWith("http://")) {
                    view.loadUrl(upgradeToHttps(url))
                    return true
                }
                return false
            }

            override fun onPageFinished(view: WebView, url: String?) {
                super.onPageFinished(view, url)
                if (url == null) return
                val host = try { Uri.parse(url).host } catch (e: Exception) { null }
                if (host != tab.currentHost) {
                    tab.currentHost = host
                    tab.shieldDisabled = stores.isAllowed(host)
                    tab.blockedCount = 0
                    tab.blockedDomainCounts.clear()
                }
                stores.recordHistory(view.title ?: url, url)
                tab.title = view.title ?: "New tab"
                tab.tabButton.text = truncate(tab.title, 14)
                if (tab == activeTab) {
                    addressBar.setText(url)
                }
            }
        }

        val tabButton = Button(this)
        tabButton.text = "New tab"
        tabButton.setTextColor(ContextCompat.getColor(this, android.R.color.white))
        tabButton.setBackgroundColor(android.graphics.Color.parseColor("#26302B"))
        tabButton.textSize = 12f
        tabButton.setPadding(24, 8, 24, 8)
        val lp = LinearLayout.LayoutParams(LinearLayout.LayoutParams.WRAP_CONTENT, LinearLayout.LayoutParams.MATCH_PARENT)
        lp.marginEnd = 6
        tabButton.layoutParams = lp
        tabButton.setOnClickListener { activateTab(tab) }
        tabButton.setOnLongClickListener { closeTab(tab); true }
        tab.tabButton = tabButton

        tabStripLayout.addView(tabButton)
        webviewContainer.addView(webView)
        tabs.add(tab)

        webView.loadUrl(upgradeToHttps(url))
        activateTab(tab)
    }

    private fun activateTab(tab: Tab) {
        activeTab = tab
        for (t in tabs) {
            t.webView.visibility = if (t == tab) View.VISIBLE else View.GONE
            t.tabButton.setBackgroundColor(
                android.graphics.Color.parseColor(if (t == tab) "#3A473F" else "#26302B")
            )
        }
        addressBar.setText(tab.webView.url ?: "")
    }

    private fun closeTab(tab: Tab) {
        if (tabs.size == 1) { finish(); return }
        val idx = tabs.indexOf(tab)
        tabs.remove(tab)
        tabStripLayout.removeView(tab.tabButton)
        webviewContainer.removeView(tab.webView)
        tab.webView.destroy()
        if (activeTab == tab) activateTab(tabs[maxOf(0, idx - 1)])
    }

    private fun truncate(s: String, max: Int) = if (s.length <= max) s else s.substring(0, max) + "…"

    // ---------------- Menu ----------------

    private fun showMainMenu(anchor: View) {
        val popup = PopupMenu(this, anchor)
        val tab = activeTab
        val blockedText = if (tab != null) "🛡 Shields (${tab.blockedCount} blocked)" else "🛡 Shields"
        val bookmarkText = if (tab != null && stores.isBookmarked(tab.webView.url ?: "")) "★ Remove bookmark" else "☆ Bookmark this page"
        popup.menu.add("➕ New tab")
        popup.menu.add("✕ Close tab")
        popup.menu.add(blockedText)
        popup.menu.add(bookmarkText)
        popup.menu.add("📑 Bookmarks")
        popup.menu.add("🕘 History")
        popup.menu.add("🌐 VPN / Proxy")
        popup.menu.add("⟳ Reload")

        popup.setOnMenuItemClickListener { item ->
            when {
                item.title == "➕ New tab" -> createTab(HOME_URL)
                item.title == "✕ Close tab" -> activeTab?.let { closeTab(it) }
                item.title!!.startsWith("🛡") -> showShieldsDialog()
                item.title!!.contains("bookmark") || item.title!!.contains("Bookmark") -> toggleBookmark()
                item.title == "📑 Bookmarks" -> showBookmarksDialog()
                item.title == "🕘 History" -> showHistoryDialog()
                item.title == "🌐 VPN / Proxy" -> showVpnDialog()
                item.title == "⟳ Reload" -> activeTab?.webView?.reload()
            }
            true
        }
        popup.show()
    }

    private fun toggleBookmark() {
        val tab = activeTab ?: return
        val url = tab.webView.url ?: return
        if (stores.isBookmarked(url)) stores.removeBookmark(url)
        else stores.addBookmark(tab.title, url)
    }

    // ---------------- Shields dialog ----------------

    private fun showShieldsDialog() {
        val tab = activeTab ?: return
        val host = tab.currentHost ?: "this site"

        val container = LinearLayout(this)
        container.orientation = LinearLayout.VERTICAL
        container.setPadding(48, 32, 48, 32)

        container.addView(TextView(this).apply { text = host; textSize = 16f; setTypeface(typeface, android.graphics.Typeface.BOLD) })
        container.addView(TextView(this).apply { text = "${tab.blockedCount} trackers & ads blocked on this page"; setPadding(0, 8, 0, 16) })

        val checkbox = android.widget.CheckBox(this)
        checkbox.text = "Disable blocking on this site"
        checkbox.isChecked = stores.isAllowed(tab.currentHost)
        container.addView(checkbox)

        container.addView(TextView(this).apply { text = "\nBlocked by domain:"; setTypeface(typeface, android.graphics.Typeface.BOLD) })
        if (tab.blockedDomainCounts.isEmpty()) {
            container.addView(TextView(this).apply { text = "Nothing blocked yet on this page." })
        } else {
            tab.blockedDomainCounts.entries.sortedByDescending { it.value }.forEach { (domain, count) ->
                container.addView(TextView(this).apply { text = "$domain — $count" })
            }
        }

        android.app.AlertDialog.Builder(this)
            .setTitle("Shields")
            .setView(android.widget.ScrollView(this).apply { addView(container) })
            .setPositiveButton("Save") { _, _ ->
                tab.currentHost?.let { stores.setAllowed(it, checkbox.isChecked) }
                tab.shieldDisabled = checkbox.isChecked
                tab.webView.reload()
            }
            .setNegativeButton("Close", null)
            .show()
    }

    // ---------------- Bookmarks dialog ----------------

    private fun showBookmarksDialog() {
        val bookmarks = stores.getBookmarks()
        if (bookmarks.isEmpty()) {
            android.app.AlertDialog.Builder(this)
                .setTitle("Bookmarks")
                .setMessage("No bookmarks yet. Use the menu → Bookmark this page.")
                .setPositiveButton("Close", null)
                .show()
            return
        }
        val labels = bookmarks.map { it.title.ifBlank { it.url } }.toTypedArray()
        android.app.AlertDialog.Builder(this)
            .setTitle("Bookmarks")
            .setItems(labels) { _, which ->
                activeTab?.webView?.loadUrl(bookmarks[which].url)
            }
            .setNegativeButton("Close", null)
            .show()
    }

    // ---------------- History dialog ----------------

    private fun showHistoryDialog() {
        val history = stores.getHistory()
        if (history.isEmpty()) {
            android.app.AlertDialog.Builder(this)
                .setTitle("History")
                .setMessage("No history yet.")
                .setPositiveButton("Close", null)
                .show()
            return
        }
        val labels = history.map {
            val when_ = java.text.DateFormat.getDateTimeInstance().format(java.util.Date(it.visitedAt))
            "$when_ — ${it.title.ifBlank { it.url }}"
        }.toTypedArray()
        android.app.AlertDialog.Builder(this)
            .setTitle("History")
            .setItems(labels) { _, which ->
                activeTab?.webView?.loadUrl(history[which].url)
            }
            .setNeutralButton("Clear") { _, _ -> stores.clearHistory() }
            .setNegativeButton("Close", null)
            .show()
    }

    // ---------------- VPN dialog ----------------

    private fun showVpnDialog() {
        val config = stores.getVpnConfig()

        val container = LinearLayout(this)
        container.orientation = LinearLayout.VERTICAL
        container.setPadding(48, 24, 48, 8)

        container.addView(TextView(this).apply {
            text = "PrivBrowser doesn't operate its own VPN servers — enter your own VPN/proxy provider's server below.\n\n" +
                   "Note: proxy access is usually a paid-tier feature. For a genuinely free VPN, install a provider's own Android app " +
                   "(e.g. Proton VPN Free) and connect it system-wide — PrivBrowser is covered automatically, no setup needed here."
            textSize = 12f
            setPadding(0, 0, 0, 16)
        })

        val enabledBox = android.widget.CheckBox(this).apply { text = "Enable proxy"; isChecked = config.enabled }
        container.addView(enabledBox)

        val hostInput = android.widget.EditText(this).apply { hint = "Server host"; setText(config.host) }
        container.addView(hostInput)

        val portInput = android.widget.EditText(this).apply {
            hint = "Port"; setText(config.port.toString()); inputType = android.text.InputType.TYPE_CLASS_NUMBER
        }
        container.addView(portInput)

        val userInput = android.widget.EditText(this).apply { hint = "Username (optional)"; setText(config.username) }
        container.addView(userInput)

        val passInput = android.widget.EditText(this).apply {
            hint = "Password (optional)"; setText(config.password)
            inputType = android.text.InputType.TYPE_CLASS_TEXT or android.text.InputType.TYPE_TEXT_VARIATION_PASSWORD
        }
        container.addView(passInput)

        android.app.AlertDialog.Builder(this)
            .setTitle("VPN / Proxy")
            .setView(android.widget.ScrollView(this).apply { addView(container) })
            .setPositiveButton("Save") { _, _ ->
                val newConfig = VpnConfig(
                    enabled = enabledBox.isChecked,
                    protocol = "socks5",
                    host = hostInput.text.toString().trim(),
                    port = portInput.text.toString().toIntOrNull() ?: config.port,
                    username = userInput.text.toString().trim(),
                    password = passInput.text.toString()
                )
                stores.saveVpnConfig(newConfig)
                applyProxyIfConfigured()
                android.widget.Toast.makeText(this, "Proxy settings applied.", android.widget.Toast.LENGTH_SHORT).show()
            }
            .setNegativeButton("Cancel", null)
            .show()
    }

    private fun applyProxyIfConfigured() {
        if (!WebViewFeature.isFeatureSupported(WebViewFeature.PROXY_OVERRIDE)) return
        val config = stores.getVpnConfig()
        val executor = Executor { it.run() }
        if (!config.enabled || config.host.isBlank()) {
            try { ProxyController.getInstance().clearProxyOverride(executor) {} } catch (e: Exception) { }
            return
        }
        val scheme = if (config.protocol == "http") "http" else "socks"
        val proxyRule = "$scheme://${config.host}:${config.port}"
        val proxyConfig = ProxyConfig.Builder().addProxyRule(proxyRule).build()
        try {
            ProxyController.getInstance().setProxyOverride(proxyConfig, executor) {}
        } catch (e: Exception) {
            // Proxy override not supported on this device's WebView — silently ignore.
        }
    }
}
