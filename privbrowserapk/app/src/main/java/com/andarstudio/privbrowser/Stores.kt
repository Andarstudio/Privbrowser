package com.andarstudio.privbrowser

import android.content.Context
import org.json.JSONArray
import org.json.JSONObject

data class Bookmark(val title: String, val url: String)
data class HistoryEntry(val title: String, val url: String, val visitedAt: Long)
data class VpnConfig(
    val enabled: Boolean = false,
    val protocol: String = "socks5", // "socks5" or "http"
    val host: String = "",
    val port: Int = 1080,
    val username: String = "",
    val password: String = ""
)

class Stores(context: Context) {
    private val prefs = context.getSharedPreferences("privbrowser", Context.MODE_PRIVATE)

    // ---------------- Allowlist ----------------
    fun isAllowed(host: String?): Boolean {
        if (host.isNullOrEmpty()) return false
        val set = prefs.getStringSet("allowlist", emptySet()) ?: emptySet()
        return set.contains(host)
    }
    fun setAllowed(host: String, allowed: Boolean) {
        if (host.isEmpty()) return
        val set = (prefs.getStringSet("allowlist", emptySet()) ?: emptySet()).toMutableSet()
        if (allowed) set.add(host) else set.remove(host)
        prefs.edit().putStringSet("allowlist", set).apply()
    }

    // ---------------- Bookmarks ----------------
    fun getBookmarks(): List<Bookmark> {
        val arr = JSONArray(prefs.getString("bookmarks", "[]"))
        return (0 until arr.length()).map {
            val o = arr.getJSONObject(it)
            Bookmark(o.getString("title"), o.getString("url"))
        }
    }
    fun isBookmarked(url: String) = getBookmarks().any { it.url == url }
    fun addBookmark(title: String, url: String) {
        if (isBookmarked(url)) return
        val list = getBookmarks().toMutableList()
        list.add(Bookmark(title.ifBlank { url }, url))
        saveBookmarks(list)
    }
    fun removeBookmark(url: String) {
        saveBookmarks(getBookmarks().filter { it.url != url })
    }
    private fun saveBookmarks(list: List<Bookmark>) {
        val arr = JSONArray()
        list.forEach { arr.put(JSONObject().put("title", it.title).put("url", it.url)) }
        prefs.edit().putString("bookmarks", arr.toString()).apply()
    }

    // ---------------- History ----------------
    fun getHistory(): List<HistoryEntry> {
        val arr = JSONArray(prefs.getString("history", "[]"))
        return (0 until arr.length()).map {
            val o = arr.getJSONObject(it)
            HistoryEntry(o.getString("title"), o.getString("url"), o.getLong("visitedAt"))
        }
    }
    fun recordHistory(title: String, url: String) {
        if (url.isBlank() || url.startsWith("about:")) return
        val list = getHistory().toMutableList()
        list.add(0, HistoryEntry(title.ifBlank { url }, url, System.currentTimeMillis()))
        val capped = list.take(500)
        val arr = JSONArray()
        capped.forEach {
            arr.put(JSONObject().put("title", it.title).put("url", it.url).put("visitedAt", it.visitedAt))
        }
        prefs.edit().putString("history", arr.toString()).apply()
    }
    fun clearHistory() { prefs.edit().putString("history", "[]").apply() }

    // ---------------- Session ----------------
    fun saveSession(urls: List<String>) {
        val arr = JSONArray()
        urls.forEach { arr.put(it) }
        prefs.edit().putString("session", arr.toString()).apply()
    }
    fun loadSession(): List<String> {
        val arr = JSONArray(prefs.getString("session", "[]"))
        return (0 until arr.length()).map { arr.getString(it) }
    }

    // ---------------- VPN / proxy ----------------
    fun getVpnConfig(): VpnConfig {
        val o = JSONObject(prefs.getString("vpn", "{}") ?: "{}")
        return VpnConfig(
            enabled = o.optBoolean("enabled", false),
            protocol = o.optString("protocol", "socks5"),
            host = o.optString("host", ""),
            port = o.optInt("port", 1080),
            username = o.optString("username", ""),
            password = o.optString("password", "")
        )
    }
    fun saveVpnConfig(config: VpnConfig) {
        val o = JSONObject()
            .put("enabled", config.enabled)
            .put("protocol", config.protocol)
            .put("host", config.host)
            .put("port", config.port)
            .put("username", config.username)
            .put("password", config.password)
        prefs.edit().putString("vpn", o.toString()).apply()
    }
}
