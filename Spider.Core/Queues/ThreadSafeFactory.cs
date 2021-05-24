using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Spider
{
    public static class ThreadSafeFactory
    {
        public static string FrontQueue = "tcp://localhost:5557";
        public static string BackUrlQueue = "tcp://localhost:5558";
        public static string BackPosionQueue = "tcp://localhost:5559";
        public static string BackQueue = "tcp://localhost:5560";
        private static Mutex _mutex;
        private readonly static ConcurrentDictionary<string, Mutex> _mutexes = new ConcurrentDictionary<string, Mutex>();

        static ThreadSafeFactory()
        {
            CreateMutex();
        }

        private static ConcurrentQueue<UrlQueueItem> _urlQueue;
        public static ConcurrentQueue<UrlQueueItem> UrlQueue()
        {
            try
            {
                _mutex.WaitOne();
                if (_urlQueue == null)
                    _urlQueue = new ConcurrentQueue<UrlQueueItem>();
                return _urlQueue;
            }
            finally
            {
                _mutex.ReleaseMutex();
            }


        }

        private static ConcurrentQueue<UrlPosionQueueItem> _poisonQueue;


        public static ConcurrentQueue<UrlPosionQueueItem> PosionQueue()
        {

            try
            {
                _mutex.WaitOne();
                if (_poisonQueue == null)
                    _poisonQueue = new ConcurrentQueue<UrlPosionQueueItem>();
                return _poisonQueue;
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        public static ConcurrentDictionary<string, TKey> Map<TKey>()
        {
            return new ConcurrentDictionary<string, TKey>();
        }

        private static void CreateMutex()
        {
            string key = "SpiderNet.Mutex." + Guid.NewGuid().ToString();
            try
            {
                _mutex = Mutex.OpenExisting(key);
            }
            catch
            {
                //the specified mutex doesn't exist, we should create it
                _mutex = new System.Threading.Mutex(false, key); //these names need to match.
            }
        }

        public static Mutex GetMutex(string key)
        {
            
            try
            {
                key = $"SpiderNet.Mutex.{key}";
                var mutex = Mutex.OpenExisting(key);
                _mutexes.TryAdd(key, mutex);
            }
            catch
            {
                //the specified mutex doesn't exist, we should create it
                var mutex = new System.Threading.Mutex(false, key); //these names need to match.
                _mutexes.TryAdd(key, mutex);
            }
            return _mutexes[key];
        }
    }
}
