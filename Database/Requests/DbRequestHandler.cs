using Microsoft.Data.Sqlite;
using SCCPP1.Session;
using SCCPP1.User;
using System.Collections.Concurrent;
using System.Data;

namespace SCCPP1.Database.Requests
{
    public class DbRequestHandler
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly SqliteConnection _connection;
        protected internal SqliteConnection Connection { get { return _connection; } }


        private readonly SqliteCommand _command;
        protected internal SqliteCommand Command { get { return _command; } }

        protected internal PriorityQueue<DbRequest, int> Requests { get; set; }

        internal int HandlingCount;

        private int Count => Requests.Count;
        public bool IsBusy => _semaphore.CurrentCount == 0;




        public DbRequestHandler(SqliteConnection connection)
        {
#if DEBUG_HANDLER
            Console.WriteLine($"Creating new handler with connection: {connection?.State}");
#endif
            Requests = new PriorityQueue<DbRequest, int>();

            _semaphore = new SemaphoreSlim(1);
            _cancellationTokenSource = new CancellationTokenSource();

            _connection = connection;
            OpenConnection();
            _command = _connection.CreateCommand();
            ResetCommand();
        }


        ~DbRequestHandler()
        {
            //use discard to finish shutdown after garbage collector is called
            _ = ShutDownAsync();
        }

        public async Task ProcessRequestsAsync()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                DbRequest request = null;
                await _semaphore.WaitAsync();
                try
                {
                    if (Count > 0)
                        request = NextRequest();
                }
                finally
                {
                    _semaphore.Release();
                }


                if (request != null)
                    await HandleRequestAsync(request);
                else
                    await Task.Delay(DbConstants.HANDLER_SLEEP_TIME);
            }
        }

        private DbRequest NextRequest()
        {
            return Requests.Dequeue();
        }

        internal async Task<bool> HandleRequestAsync(DbRequest request)
        {

#if DEBUG_HANDLER
            Console.WriteLine($"[{GetType().Name}] waiting for semaphore unlock for request: {request.GetType().Name}");
#endif
            bool success = false;
            try
            {
                await _semaphore.WaitAsync(_cancellationTokenSource.Token);


#if DEBUG_HANDLER
                Console.WriteLine($"[{GetType().Name}] semaphore acquired and locked for request: {request.GetType().Name}");
#endif

                //handle request if needed
                //request.SetHandler(this);

                success = ProcessRequest(request);

            }
            finally
            {
                _semaphore.Release();

#if DEBUG_HANDLER
                Console.WriteLine($"[{GetType().Name}] semaphore unlocked and released for request: {request.GetType().Name}");
#endif
                /*                if (success)
                                    await CompleteRequestAsync(request);
                                else
                                    await FailRequestAsync(request);*/
            }
            return success;
        }

        private bool ProcessRequest(DbRequest request)
        {
            return request.Execute(this);
        }

        private async Task CompleteRequestAsync(DbRequest request)
        {
            //set request's PostOperationStatus flag
            await Task.Run(() => { request.Status = DbRequest.RequestStatus.COMPLETED; });
        }

        private async Task FailRequestAsync(DbRequest request)
        {
            //set request's PostOperationStatus flag
            await Task.Run(() => { request.Status = DbRequest.RequestStatus.COMPLETED; });
        }


        internal async Task<bool> CancelRequest(DbRequest request)
        {
            bool result = false;
            try
            {
                //wait for token to access request
                await _semaphore.WaitAsync(_cancellationTokenSource.Token);

                if (Requests.TryDequeue(out request, out int priority))
                    result = true;
            }
            finally
            {
                _semaphore.Release();
            }

            return result;
        }


        internal async Task<bool> ShutDownAsync()
        {
            //set the token to request cancellation
            _cancellationTokenSource.Cancel();

            //wait for semaphore to be released by the ProcessRequestsAsync() method
            await _semaphore.WaitAsync();

            //dequeue and mark all requests as failed
            while (Requests.Count > 0)
            {
                DbRequest request = Requests.Dequeue();
                await FailRequestAsync(request);
            }

            //release token(s) and dispose resources
            _semaphore.Release();
            _cancellationTokenSource.Dispose();
            _connection.Dispose();

            return true;
        }

        internal void CloseConnection()
        {
            if (_connection != null)
                _connection.Close();
        }

        internal void OpenConnection()
        {
            if (_connection != null)
                _connection.Open();
        }

        internal void DisposeConnection()
        {
            if (_connection != null)
                _connection.Dispose();
        }


        internal void ResetCommand()
        {
            //clear command text and params
            Command.CommandText = string.Empty;
            Command.Parameters.Clear();

            //set properties to defaults
            Command.CommandType = CommandType.Text;
            Command.CommandTimeout = 30;
            Command.DesignTimeVisible = true;
            Command.UpdatedRowSource = UpdateRowSource.None;
        }

    }

}
