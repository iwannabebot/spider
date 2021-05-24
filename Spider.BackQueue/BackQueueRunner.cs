using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Spider.BackQueue
{
    class BackQueueRunner
    {
        static readonly ConcurrentQueue<UrlPosionQueueItem> _posionQueue = ThreadSafeFactory.PosionQueue();
        static void Main(string[] args)
        {
                                                                                     
            Console.WriteLine("====== Back Queue STARTED ======");
            using var fromWorkerForPosion = new PullSocket($"@{ThreadSafeFactory.BackPosionQueue}");
            using var fromWorkerForUrl = new PullSocket($"@{ThreadSafeFactory.BackUrlQueue}");        
            using var forFrontForUrl = new PushSocket($"@{ThreadSafeFactory.BackQueue}");
            Console.WriteLine("Socket initialized");

            var fromWorkerProcessorForFrontQueue = Task.Run(() =>
            {
                do
                {
                    try
                    {
                        Console.WriteLine($"Waiting for worker link");
                        var queueItem = fromWorkerForUrl.ReceiveFrameString();
                        Console.WriteLine($"Received [{queueItem}] from worker");
                        Console.WriteLine($"Sending to front queue");
                        forFrontForUrl.SendFrame(queueItem);
                        Console.WriteLine($"Sent to front queue");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in Front Queue Processor {ex.Message}");
                    }

                } while (true);
            });
            var fromWorkerProcessorForPoison = Task.Run(() =>
            {
                do
                {
                    Console.WriteLine($"Waiting for poison items");
                    var workload = fromWorkerForPosion.ReceiveFrameString();
                    UrlPosionQueueItem queueItem = workload.FromJson<UrlPosionQueueItem>();
                    _posionQueue.Enqueue(queueItem);
                    Console.WriteLine($"Received poison [{queueItem.ToJson()}] from worker");

                } while (true);
            });

            var poisonQueueProcessor = Task.Run(() =>
            {

                do
                {
                    //while (_posionQueue.IsEmpty)
                    //{
                    //    Task.Delay(5000).Wait();
                    //}
                    if (_posionQueue.TryDequeue(out UrlPosionQueueItem queueItem))
                    {
                        Console.WriteLine($"Processing poison {queueItem.ToJson()}");
                    }
                    // TODO: Persist all posion queue items with failure reasons in db
                } while (true);
            });

            Task.WaitAll(fromWorkerProcessorForFrontQueue,/* fromWorkerProcessorForPoison,*/ poisonQueueProcessor); 
            Console.WriteLine("====== FRONT QUEUE ======");
        }
    }
}
