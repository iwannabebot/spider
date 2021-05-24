using System;
using System.Collections.Generic;
using System.Text;

namespace Spider
{
    public class UrlPosionQueueItem : UrlQueueItem , ICloneable
    {
        public int TryCount { get; set; }

        public List<string> Errors { get; set; }
    }
}
