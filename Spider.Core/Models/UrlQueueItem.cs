using System;
using System.Collections.Generic;
using System.Text;

namespace Spider
{
    public class UrlItem : ICloneable
    {
        public string Url { get; set; }
        public string Host
        {
            get
            {
                return Url == null ? string.Empty : new Uri(Url).Host;
            }
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    public class UrlQueueItem : UrlItem, ICloneable
    {
        public DateTime? QueuedOn { get; set; }
        public DateTime? CrawlStart { get; set; }
        public DateTime? CrawlFinished { get; set; }
    }
}
