using ConnectionPool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceExercise
{
    public class Service : IService, IDisposable
    {
        private List<Task<int>> tasks;
        int sum;
        private SemaphoreSlim semaphore;
        private Connection c;
        BlockingCollection<Request> q = new BlockingCollection<Request>();
        bool produceRunning = false;
        private readonly object produceLock = new object();
        public Service(int CONNETION_COUNT)
        {
            sum = 0;
            c = new Connection();
            tasks = new List<Task<int>>();
            semaphore = new SemaphoreSlim(CONNETION_COUNT);
        }

        /// <summary>
        /// returns the sum in the current time of the service
        /// </summary>
        /// <returns></returns>
        public int getSummary()
        {
            return sum;
        }

        /// <summary>
        /// Double check if the queue is empty when we notify that no more loading is done. and dispose the connection
        /// </summary>
        public void notifyFinishedLoading()
        {
            if (produceRunning == false)
            {
                lock (produceLock)
                {
                    produceRunning = true;
                    Produce();
                    produceRunning = false;
                }
            }
            while (q.Count != 0) ;
            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("Finished loading");
            Dispose();
        }

        /// <summary>
        /// Adds requests to the queue, and runs the produce function one at a time
        /// </summary>
        /// <param name="request"></param>
        public void sendRequest(Request request)
        {
            Console.WriteLine(request.Command + " has entered the q");
            q.Add(request);


            if (produceRunning == false)
            {
                Task.Run(() =>
                {
                    lock (produceLock)
                    {
                        Console.WriteLine("------------------------------------------- this is hapaning ---------------------------------------");
                        produceRunning = true;
                        Produce();
                        produceRunning = false;
                        Console.WriteLine("||||||||||||||||||||||||||||||||||||||||||| this is hapaning |||||||||||||||||||||||||||||||||||||||||||");
                    }
                });
            }
            

        }

        /// <summary>
        /// Go over all the requests in the queue, using semaphore so it will use only 4 requests at a time.
        /// </summary>
        public void Produce()
        {   
            Task<int> t = null;
            while (q.Count > 0)
            {
                var request = q.Take();
                Console.WriteLine(request.Command + " has entered the Produce function");
                semaphore.Wait();

                Console.WriteLine(request.Command + " has entered the semaphore");
                t = c.runCommandAsync(request.Command);
                tasks.Add(t);

                t.ContinueWith((x) =>
                {
                    semaphore.Release();
                    sum += x.Result;
                    Console.WriteLine(request.Command + " has left the semaphore");
                });
            }
        }


        public void Dispose()
        {
            
            semaphore.Dispose();
            q.Dispose();
            c.Dispose();
        }
    }
}
