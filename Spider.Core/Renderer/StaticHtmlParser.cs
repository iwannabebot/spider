using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Spider
{
    public class StaticHtmlParser : IHTMLRenderer
    {
        private readonly ISelectionPolicy _selectionPolicy;

        public StaticHtmlParser(ISelectionPolicy selectionPolicy)
        {
            _selectionPolicy = selectionPolicy;
        }
        public string[] FindChildLinks(UrlQueueItem url)
        {
            List<string> urls = new List<string>();
            HtmlWeb web = new HtmlWeb();
            HtmlDocument htmlDoc = web.LoadFromWebAsync(url.Url).Result;
            if (htmlDoc.DocumentNode != null)
            {

                try
                {
                    foreach (var a in htmlDoc.DocumentNode.Descendants("a"))
                    {
                        var u = a.GetAttributeValue("href", null);
                        if (string.IsNullOrWhiteSpace(u))
                        {
                            continue;
                        }
                        if (!u.StartsWith("//") && u.StartsWith("/"))
                        {
                            u = Flurl.Url.Combine($"https://{url.Host}", u);
                        }

                        if(_selectionPolicy.CanIGoThere(url, u))
                        {
                            urls.Add(u);
                        }
                    }
                }
                catch
                {
                    throw;
                }
            }
            return urls.ToArray();
        }
    }
}
