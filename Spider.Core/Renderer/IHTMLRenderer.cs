using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Spider
{
    public interface IHTMLRenderer
    {
        // TODO: Javascript renderer for async pages
        string[] FindChildLinks(UrlQueueItem url);
    }
}
