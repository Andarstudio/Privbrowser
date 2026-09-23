namespace PrivBrowser.Models
{
    public class DownloadItemModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public long ReceivedBytes { get; set; }
        public long TotalBytes { get; set; }
        public bool IsInProgress { get; set; }
        public bool IsComplete { get; set; }
    }
}