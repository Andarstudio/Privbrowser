package com.andarstudio.privbrowser

import java.io.File
import java.net.HttpURLConnection
import java.net.URL
import java.util.concurrent.ConcurrentHashMap

object TrackerBlockList {
    private val domains = ConcurrentHashMap.newKeySet<String>()

    private val sources = listOf(
        "https://easylist.to/easylist/easylist.txt" to "easylist.txt",
        "https://easylist.to/easylist/easyprivacy.txt" to "easyprivacy.txt"
    )

    private val fallbackDomains = listOf(
        "doubleclick.net", "googlesyndication.com", "googleadservices.com",
        "google-analytics.com", "googletagmanager.com", "facebook.net",
        "connect.facebook.net", "scorecardresearch.com", "adnxs.com",
        "outbrain.com", "taboola.com", "criteo.com", "hotjar.com",
        "mixpanel.com", "segment.io", "amplitude.com", "quantserve.com",
        "adsrvr.org", "moatads.com"
    )

    fun initialize(cacheDir: File) {
        cacheDir.mkdirs()
        var loadedFromDisk = false
        for ((_, fileName) in sources) {
            val file = File(cacheDir, fileName)
            if (file.exists()) {
                parseInto(file.readText())
                loadedFromDisk = true
            }
        }
        if (!loadedFromDisk) domains.addAll(fallbackDomains)

        // Refresh in the background; never block app startup on network.
        Thread { refreshIfStale(cacheDir) }.start()
    }

    private fun refreshIfStale(cacheDir: File) {
        for ((url, fileName) in sources) {
            try {
                val file = File(cacheDir, fileName)
                val isStale = !file.exists() ||
                    (System.currentTimeMillis() - file.lastModified()) > 7L * 24 * 60 * 60 * 1000
                if (!isStale) continue

                val text = fetch(url)
                file.writeText(text)
                parseInto(text)
            } catch (e: Exception) {
                // Offline is fine — keep whatever's cached or the fallback list.
            }
        }
    }

    private fun fetch(urlString: String): String {
        val conn = URL(urlString).openConnection() as HttpURLConnection
        conn.connectTimeout = 15000
        conn.readTimeout = 15000
        conn.inputStream.use { stream ->
            return stream.bufferedReader().readText()
        }
    }

    private fun parseInto(text: String) {
        for (rawLine in text.lineSequence()) {
            val line = rawLine.trim()
            if (line.isEmpty() || line.startsWith("!") || line.startsWith("[")) continue
            if (!line.startsWith("||")) continue

            var endIdx = line.length
            for (i in 2 until line.length) {
                if (line[i] == '^' || line[i] == '/' || line[i] == '$') { endIdx = i; break }
            }
            val domain = line.substring(2, endIdx).trim()
            if (domain.isNotEmpty() && domain.contains('.')) domains.add(domain)
        }
    }

    fun isBlocked(urlString: String): Boolean {
        return try {
            val host = URL(urlString).host ?: return false
            if (domains.contains(host)) return true
            domains.any { host.endsWith(".$it") }
        } catch (e: Exception) {
            false
        }
    }
}
