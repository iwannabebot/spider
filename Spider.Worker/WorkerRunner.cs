using HtmlAgilityPack;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Spider.Worker
{
    class WorkerRunner
    {
        static readonly ConcurrentQueue<UrlQueueItem> _urlQueue = ThreadSafeFactory.UrlQueue();
        static readonly ConcurrentDictionary<string, bool> _visitedUrls = ThreadSafeFactory.Map<bool>();
        static readonly ConcurrentQueue<UrlPosionQueueItem> _urlPoisonQueue = ThreadSafeFactory.PosionQueue();
        static readonly IHTMLRenderer _htmlRenderer = RendererFactory.DefaultRenderer(PolicyFactory.DefaultSelectionPolicy());
        static readonly int maxThread = System.Environment.ProcessorCount* 3;
        static void Main(string[] args)
        {
            const int POISON_QUEUE_MAX_TRY = 5;
            Console.WriteLine("====== WORKER ======");

            using var fromFrontQueue = new PullSocket($">{ThreadSafeFactory.FrontQueue}");
            using var toBackQueue = new PushSocket($">{ThreadSafeFactory.BackUrlQueue}");
            using var toPoisonQueue = new PushSocket($">{ThreadSafeFactory.BackPosionQueue}");

            Console.WriteLine("Initialized Socket");
            //process tasks forever
            var frontQueueProcessor = Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        Console.WriteLine($"Waiting for front queue");
                        var workload = fromFrontQueue.ReceiveFrameString();
                        UrlQueueItem queueItem = workload.FromJson<UrlQueueItem>();
                        Console.WriteLine($"Request from frontqueue {queueItem.Url}");
                        _urlQueue.Enqueue(queueItem);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Error in Front Queue Processor {ex.Message}");
                    }
                    
                }

            });

            var processUrlTask = Task.Run(() =>
            {
                while (true)
                {
                    Parallel.For(0, 2 * maxThread / 3, (threadIndex) =>
                    {
                        //while (_urlQueue.IsEmpty)
                        //{
                        //    Task.Delay(5000).Wait();
                        //}
                        if (_urlQueue.TryDequeue(out UrlQueueItem url))
                        {
                            Console.WriteLine($"Processing {url.Url}");
                            url.CrawlStart = DateTime.UtcNow;
                            try
                            {
                                foreach (var childUrl in _htmlRenderer.FindChildLinks(url))
                                {
                                    if (!_visitedUrls.ContainsKey(childUrl))
                                    {
                                        _visitedUrls.TryAdd(childUrl, false);
                                        Console.WriteLine($"Sending to backqueue");
                                        toBackQueue.SendFrame(childUrl);
                                        Console.WriteLine($"Sending to backqueue");

                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                var u = url.Clone() as UrlPosionQueueItem;
                                u.TryCount = 0;
                                u.CrawlFinished = DateTime.UtcNow;
                                u.Errors = new List<string>
                                {
                                    ex.Message + "|" + ex?.InnerException?.Message
                                };
                                _urlPoisonQueue.Enqueue(u);
                                Console.WriteLine($"Error in Url Processor {ex.Message}");
                            }
                            url.CrawlFinished = DateTime.UtcNow;
                        }
                    });
                }

            });

            var poisonUrlTask = Task.Run(() =>
            {
                while (true)
                {
                    Parallel.For(0, maxThread / 3, async (threadIndex) =>
                    {
                        //while (_urlPoisonQueue.IsEmpty)
                        //{
                        //    await Task.Delay(5000);
                        //}
                        if (_urlPoisonQueue.TryDequeue(out UrlPosionQueueItem url))
                        {
                            if (url.TryCount < POISON_QUEUE_MAX_TRY)
                            {
                                Console.WriteLine($"Processing Posion {url.Url} Iteration: #{url.TryCount}");
                                try
                                {
                                    foreach (var childUrl in _htmlRenderer.FindChildLinks(url))
                                    {
                                        if (!_visitedUrls.ContainsKey(childUrl))
                                        {
                                            Console.WriteLine($"Sending to poison [{url.Url}]");
                                            toBackQueue.SendFrame(childUrl);
                                            _visitedUrls.TryAdd(childUrl, false);
                                            Console.WriteLine($"Sent to poison [{url.Url}]");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    url.TryCount++;
                                    url.Errors.Add(ex.Message + "|" + ex?.InnerException?.Message);
                                    _urlPoisonQueue.Enqueue(url);
                                    Console.WriteLine($"Error in Poison Queue Processor {ex.Message}");
                                }
                            }
                            else
                            {
                                toPoisonQueue.SendFrame(_urlPoisonQueue.ToJson());
                            }


                            url.CrawlFinished = DateTime.UtcNow;
                        }
                    });
                }

            });
            Task.WaitAll(frontQueueProcessor, processUrlTask, poisonUrlTask);
            Console.WriteLine("====== WORKER FINISHED ======");
        }

        
    }
}
