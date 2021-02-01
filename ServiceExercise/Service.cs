using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectionPool
{
    public class Service : IService
    {
        private const int WorkersCount = 10;
        private readonly int _connectionsCount;
        private int _sum;
        private int _count;
        private readonly object _lockObject = new object();
        private readonly BlockingCollection<Request> _requests = new BlockingCollection<Request>();
        private readonly BlockingCollection<Connection> _connections = new BlockingCollection<Connection>();

        public Service(int connectionsCount)
        {
            _connectionsCount = connectionsCount;
            for (var i = 1; i <= _connectionsCount; i++)
            {
                _connections.Add(new Connection());
            }

            for (var i = 0; i < WorkersCount; i++)
            {
                Task.Factory.StartNew(HandleRequests);
            }
            
        }
        
        public void sendRequest(Request request)
        {
            _requests.Add(request);
            Console.WriteLine($"{request.Command} request added");
        }

        public void notifyFinishedLoading()
        {
            _requests.CompleteAdding();
        }

        public int getSummary()
        {
            var processingFinished = _requests.IsCompleted && _connections.Count == _connectionsCount;
            while (!processingFinished)
            {
                Thread.Sleep(1000);
                processingFinished = _requests.IsCompleted && _connections.Count == _connectionsCount;
            }
            Console.WriteLine($"Processed {_count} requests");
            return _sum;
        }

        private async Task HandleRequests()
        {
            while(!_requests.IsCompleted)
            {
                _requests.TryTake(out var request);
                if(request == null) continue;
                var connection = _connections.Take();
                var result = await connection.runCommandAsync(request.Command);
                lock (_lockObject)
                {
                    _count++;
                    _sum += result;
                }
                _connections.Add(connection);
            }
        }
        
    }
}
