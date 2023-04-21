using Microsoft.Data.Sqlite;
using SCCPP1.Pages;
using SCCPP1.User;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace SCCPP1.Database.Requests
{
    public static class DbRequestManager
    {
        private const string _connectionString = "CPPDatabase.db";


        private static readonly object lockObject;



        private static ConcurrentBag<DbRequestHandler> _handlers;
        private static ConcurrentDictionary<Account, DbRequestHandler> _accountHandlerMap;
        private static ConcurrentQueue<DbRequest> _requests;



        static DbRequestManager()
        {
            lockObject = new object();
            _handlers = new ConcurrentBag<DbRequestHandler>();

            //for (int i = 0; i < CONNECTION_POOL_SIZE; i++)
            //    RegisterHandler(new DbRequestHandler(new SqliteConnection(_connectionString)));


            _requests = new ConcurrentQueue<DbRequest>();
            _accountHandlerMap = new ConcurrentDictionary<Account, DbRequestHandler>();

            Task.Run(HandlerCleanupAsync);
            Task.Run(ProcessRequestsAsync);
        }


        private static async Task HandlerCleanupAsync()
        {
            while (true)
            {
                await Task.Delay(DbConstants.HANDLER_CLEANUP_INTERVAL);

                lock (lockObject)
                {
                    //do some handler clean up.
                    //(may want to add stopwatch to check how long it has been since it last did requests)
                    foreach (DbRequestHandler handler in _handlers)
                        if (!handler.IsBusy && handler.HandlingCount == 0)
                            UnregisterHandler(handler);
                }
            }
        }



        private static DbRequestHandler? RegisterHandler()
        {
            lock (lockObject)
            {
                if (_handlers.Count >= DbConstants.CONNECTION_POOL_SIZE)
                    return null;

                DbRequestHandler handler;

                _handlers.Add(handler = new DbRequestHandler(new SqliteConnection(_connectionString)));
                Task.Run(handler.ProcessRequestsAsync);

                return handler;
            }
        }

        private static bool UnregisterHandler(DbRequestHandler handler)
        {
            lock (lockObject)
            {
                return _handlers.TryTake(out handler);
            }
        }



        private static async Task ProcessRequestsAsync()
        {
            Stopwatch sw = Stopwatch.StartNew();


            while (true)
            {

                //check queue for requests
                if (_requests.Count == 0)
                {
                    sw.Restart();
                    lock (lockObject)
                    {
                        //do some account cleanup
                        foreach (KeyValuePair<Account, DbRequestHandler> pair in _accountHandlerMap)
                        {
                            if (pair.Key.Data.IsAuthenticated())
                                continue;

                            if (_accountHandlerMap.TryRemove(pair.Key, out DbRequestHandler handler))
                                handler.HandlingCount--;

                        }

                    }
                    sw.Stop();

                    //no requests found, wait for rest of SLEEP_TIME
                    await Task.Delay(DbConstants.SLEEP_TIME - (int)sw.ElapsedMilliseconds);
                    continue;
                }

                //requests found, attempt dequeue
                if (_requests.TryDequeue(out DbRequest request))
                {
                    bool success = await HandleRequestAsync(request);

                    //if the request failed, add it back to the queue to be processed again
                    if (!success && request.Status != DbRequest.RequestStatus.COMPLETED)
                    {
                        _requests.Enqueue(request);
                    }
                }
            }
        }



        private static async Task<bool> HandleRequestAsync(DbRequest request)
        {
            //user logged out, cancel request
            if (!request.GetAccount().Data.IsAuthenticated())
            {
                //might not want, since user can log out
                //but data still needs to be manipulated
                request.Status = DbRequest.RequestStatus.COMPLETED;
                return false;
            }

            DbRequestHandler? handler = GetHandlerForAccount(request.GetAccount());

            if (handler == null)
                return false;

            //update request status
            request.Status = DbRequest.RequestStatus.PROCESSING;

            //enqueue request then return immeadiately
            return await handler.HandleRequestAsync(request);
        }




        /// <summary>
        /// Attempts to get a handler for an account. 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        private static DbRequestHandler? GetHandlerForAccount(Account account)
        {
            if (_accountHandlerMap.TryGetValue(account, out DbRequestHandler? handler))
                return handler;


            int requestCount = int.MaxValue;
            foreach (DbRequestHandler h in _handlers)
            {

                if (h.HandlingCount < DbConstants.MAX_ACCOUNTS_PER_HANDLER)
                    if (!h.IsBusy)
                    {
                        handler = h;
                        break;
                    }
                    else if (h.Requests.Count < requestCount)
                    {
                        handler = h;
                        requestCount = h.Requests.Count;
                    }
            }

            //no handlers were chosen, choose first handler
            //(this is unlikely to ever happen)
            if (handler == null)
                handler = _handlers.Where(h => h.HandlingCount < DbConstants.MAX_ACCOUNTS_PER_HANDLER).FirstOrDefault();

            if (handler == null)
                handler = RegisterHandler();

            if (handler != null)
            {
                //this shouldn't happen since base case did not return
                if (_accountHandlerMap.TryAdd(account, handler))
                    handler.HandlingCount++;
            }

            return handler;

        }

    }

}
