using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Channels;

namespace ConnectionPool
{
    public class Service : IService
    {
        private readonly int _connectionsCount;
        private readonly Channel<Request> _requests = Channel.CreateUnbounded<Request>();
        private readonly List<Task<int>> _workerTasks = new List<Task<int>>();

        public Service(int connectionsCount)
        {
            _connectionsCount = connectionsCount;
            for (var i = 0; i < _connectionsCount; i++)
            {
                var workerTask = Task.Run(() => HandleRequests(new Connection()));
                _workerTasks.Add(workerTask);
            }
        }
        
        public void sendRequest(Request request)
        {
            bool success = _requests.Writer.TryWrite(request);
            if (!success) {
                // since our channel is unbounded, this is the only reason for failure
                throw new InvalidOperationException("Tried writing a request to a closed channel");
            }
        }

        public void notifyFinishedLoading()
        {
            _requests.Writer.Complete();
        }

        public int getSummary()
        {
            Task.WaitAll(_workerTasks.ToArray());
            return _workerTasks.Select(t => t.Result).Sum();
        }

        private async Task<int> HandleRequests(Connection connection)
        {
            int sum = 0;
            while(!_requests.Reader.Completion.IsCompleted) {
                var request = await _requests.Reader.ReadAsync();
                var result = await connection.runCommandAsync(request.Command);
                sum += result;
            }
            return sum;
        }
        
    }
}
