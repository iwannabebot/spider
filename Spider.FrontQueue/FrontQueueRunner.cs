using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace Spider.FrontQueue
{
    class FrontQueueRunner
    {

        static readonly ConcurrentDictionary<string, bool> _urlVisiting = ThreadSafeFactory.Map<bool>();
        static readonly ConcurrentQueue<UrlQueueItem> _urlQueue = ThreadSafeFactory.UrlQueue();
        static readonly IPolitenessPolicy _politenessPolicy = PolicyFactory.DefaultPolitenessPolicy();
        static readonly int maxThread = Environment.ProcessorCount * 3;
        static void Main(string[] seed)
        {

            Console.WriteLine("====== FRONT QUEUE ======");
            using var toWorkers = new PushSocket($"@{ThreadSafeFactory.FrontQueue}");
            using var fromBackQueue = new PullSocket($">{ThreadSafeFactory.BackQueue}");

            Console.WriteLine("Socket initialized");

            foreach (var s in seed)
            {
                _urlQueue.Enqueue(new UrlQueueItem { Url = s });
            }
            var backQueueProcessor = Task.Run(() =>
            {
                // Receive from backqueue
                while (true)
                {
                    Console.WriteLine($"Waiting for backqueue");
                    var workload = fromBackQueue.ReceiveFrameString(); 
                    Console.WriteLine($"Got message from backqueue");
                    UrlQueueItem queueItem = new UrlQueueItem
                    {
                        Url = workload
                    };
                    if (!_urlVisiting.ContainsKey(queueItem.Url))
                    {
                        _urlQueue.Enqueue(queueItem);
                    }
                    else
                    {
                        Console.WriteLine($"Url {queueItem.Url} already sent to processing");
                    }


                }
            });

            var frontQueueProcessor = Task.Run(() =>
            {
                // send to worker
                while (true)
                {
                    do
                    {
                        Parallel.For(0, maxThread, (threadIndex) =>
                        {
                            //while (_urlQueue.IsEmpty)
                            //{
                            //    Task.Delay(5000).Wait();
                            //}
                            if (_urlQueue.TryDequeue(out UrlQueueItem url))
                            {
                                if (_politenessPolicy.CanIGoThere(url, out long crawlDelay))
                                {
                                    Task.Delay((int)crawlDelay).Wait();
                                    url.QueuedOn = DateTime.UtcNow;
                                    Console.WriteLine($"Sending to worker [{url.Url}]");
                                    toWorkers.SendFrame(url.ToJson());
                                    Console.WriteLine($"Sent to worker");
                                    _urlVisiting.AddOrUpdate(url.Url.ToLower(), false, (key, val) => false);
                                }
                            }
                        });
                    } while (!_urlQueue.IsEmpty);

                }
            });

            Task.WaitAll(backQueueProcessor, frontQueueProcessor);
            Console.WriteLine("====== FRONT QUEUE ENDED ======");
        }
    }
}
