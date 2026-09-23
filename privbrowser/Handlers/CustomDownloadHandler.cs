using System;
using System.Collections.ObjectModel;
using System.Windows;
using CefSharp;
using PrivBrowser.Models;

namespace PrivBrowser.Handlers
{
    public class CustomDownloadHandler : IDownloadHandler
    {
        public ObservableCollection<DownloadItemModel> Downloads { get; } = new();

        public bool CanDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, string url, string requestMethod) => true;

        public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    callback.Continue(downloadItem.SuggestedFileName, showDialog: true);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Downloads.Add(new DownloadItemModel
                    {
                        Id = downloadItem.Id,
                        FileName = downloadItem.SuggestedFileName,
                        FullPath = downloadItem.FullPath,
                        TotalBytes = downloadItem.TotalBytes,
                        IsInProgress = true
                    });
                });
            }
        }

        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadResponseBodyCallback callback)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var item in Downloads)
                {
                    if (item.Id == downloadItem.Id)
                    {
                        item.ReceivedBytes = downloadItem.ReceivedBytes;
                        item.TotalBytes = downloadItem.TotalBytes;
                        item.IsInProgress = downloadItem.IsInProgress;
                        item.IsComplete = downloadItem.IsComplete;
                        item.FullPath = downloadItem.FullPath;
                        break;
                    }
                }
            });
        }
    }
}