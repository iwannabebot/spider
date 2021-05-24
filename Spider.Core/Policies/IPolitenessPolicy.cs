using Com.Bekijkhet.RobotsTxt;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;

namespace Spider
{
    public interface IPolitenessPolicy
    {
        bool RestrictToIP { get; set; }
        bool RobotEnabled { get; set; }
        bool Parallelization { get; set; }
        bool CanIGoThere(UrlItem url, out long crawlDelay);
    }
    public class PolitenessPolicy : IPolitenessPolicy
    {
        readonly ConcurrentDictionary<string, bool> _allowedUrl = ThreadSafeFactory.Map<bool>();
        readonly ConcurrentDictionary<string, Robots> _robots = ThreadSafeFactory.Map<Robots>();
        public bool RestrictToIP { get; set; }
        public bool RobotEnabled { get; set; }
        public bool Parallelization { get; set; }

        public bool CanIGoThere(UrlItem url, out long crawlDelay)
        {
            crawlDelay = 0;

            if (!RobotEnabled)
                return true;
            
            if (!_allowedUrl.GetOrAdd(url.Url, false))
            {
                var content = _robots.GetOrAdd(url.Host, default(Robots));
                if (content == default)
                {
                    try
                    {
                        var response = WebRequest.Create("https://" + url.Host + "/robots.txt").GetResponse() as HttpWebResponse;
                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.OK:
                                using (Stream dataStream = response.GetResponseStream())
                                {
                                    StreamReader reader = new StreamReader(dataStream);
                                    string responseFromServer = reader.ReadToEnd();
                                    Robots robots = Robots.Load(responseFromServer);
                                    _robots.AddOrUpdate(url.Host, robots, (k, v) => robots);
                                }
                                break;
                            case HttpStatusCode.NotFound:
                            default:                                                               
                                Console.WriteLine($"Robots file cannot be read for {url.Host}");
                                _allowedUrl.AddOrUpdate(url.Url, true, (k, v) => true);
                                return true;
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Robots file cannot be read for {url.Host}, Reason {ex.Message}");
                        return true;
                    }
                    
                }
                crawlDelay = _robots[url.Host].CrawlDelay("*");
                var s = url.Url.Split(new string[] { url.Host }, StringSplitOptions.None);
                if(s.Length > 1 && !string.IsNullOrWhiteSpace(s[1]))
                {
                    return _robots[url.Host].IsPathAllowed("*", s[1]);
                }
                else
                {

                    return _robots[url.Host].IsPathAllowed("*", url.Url);
                }
            }
            return true;
        }
    }
}
