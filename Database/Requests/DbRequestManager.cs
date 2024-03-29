﻿using Azure.Core;
using Microsoft.Data.Sqlite;
using SCCPP1.Database.Requests.Operations.Certifications;
using SCCPP1.Database.Requests.Operations.Colleague;
using SCCPP1.Database.Requests.Operations.Education;
using SCCPP1.Database.Requests.Operations.Profiles;
using SCCPP1.Database.Requests.Operations.Skills;
using SCCPP1.Database.Requests.Operations.Work;
using SCCPP1.Session;
using SCCPP1.User;
using SCCPP1.User.Data;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SCCPP1.Database.Requests
{
    /// <summary>
    /// The <see cref="DbRequestManager"/> class manages the database requests made by the application.
    /// It provides methods for saving and loading different types of data objects from the database,
    /// as well as searching for profiles using a keyphrase. It also manages a pool of database
    /// <see cref="DbRequestHandler"/>'s to handle incoming requests.
    /// </summary>
    public static class DbRequestManager
    {
        private static string _connectionString = DbConstants.ConnectionString;

        private static SemaphoreSlim _handlerSemaphore;

        private static readonly object lockObject;



        private static ConcurrentBag<DbRequestHandler> _handlers;
        private static ConcurrentDictionary<SessionData, DbRequestHandler> _sessionHandlerMap;
        private static ConcurrentQueue<DbRequest> _requests;



        /// <summary>
        /// Initializes the database and starts the request handler cleanup and request processing tasks.
        /// </summary>
        static DbRequestManager()
        {
            
            _handlerSemaphore = new SemaphoreSlim(DbConstants.CONNECTION_POOL_SIZE);
            lockObject = new object();
            _handlers = new ConcurrentBag<DbRequestHandler>();

            //for (int i = 0; i < CONNECTION_POOL_SIZE; i++)
            //    RegisterHandler(new DbRequestHandler(new SqliteConnection(_connectionString)));


            _requests = new ConcurrentQueue<DbRequest>();
            _sessionHandlerMap = new ConcurrentDictionary<SessionData, DbRequestHandler>();

            //start the database
            InitializeDatabase();

            Task.Run(HandlerCleanupAsync);
            Task.Run(ProcessRequestsAsync);
        }


        /// <summary>
        /// Performs handler cleanup operations on a regular interval to remove idle and unused handlers.
        /// </summary>
        /// <returns>An asynchronous operation.</returns>
        private static async Task HandlerCleanupAsync()
        {
            while (true)
            {
                await Task.Delay(DbConstants.HANDLER_CLEANUP_INTERVAL);

                try
                {
                    await _handlerSemaphore.WaitAsync();
#if DEBUG_HANDLER
                    Console.WriteLine($"Handler clean up started...");
                    int handlerCount = _handlers.Count;
#endif

                    //do some handler clean up.
                    //(may want to add stopwatch to check how long it has been since it last did requests)
                    foreach (DbRequestHandler handler in _handlers)
                        if (!handler.IsBusy && handler.HandlingCount == 0)
                            UnregisterHandler(handler);

#if DEBUG_HANDLER
                    Console.WriteLine($"Removed {handlerCount - _handlers.Count} handlers");
#endif

                }
                finally
                {
                    _handlerSemaphore.Release();
                }
            }
        }


        /// <summary>
        /// Registers a new <see cref="DbRequestHandler"/> instance.
        /// </summary>
        /// <returns>An asynchronous operation that returns a new <see cref="DbRequestHandler"/> instance.</returns>
        private static async Task<DbRequestHandler> RegisterHandler()
        {
            await _handlerSemaphore.WaitAsync();
#if DEBUG_HANDLER
            Console.WriteLine($"Attempting to register handler #{_handlers.Count + 1} out of {DbConstants.CONNECTION_POOL_SIZE}");
#endif
            if (_handlers.Count >= DbConstants.CONNECTION_POOL_SIZE)
            {
#if DEBUG_HANDLER
                Console.WriteLine($"Max handlers reached {_handlers.Count}/{DbConstants.CONNECTION_POOL_SIZE}");
#endif
                return null;
            }

            DbRequestHandler handler;

            //should we create the connection int the handler to ensure no multithreading issues?
            _handlers.Add(handler = new DbRequestHandler(new SqliteConnection(_connectionString)));
#if DEBUG_HANDLER
            Console.WriteLine($"Added handler #{_handlers.Count} out of {DbConstants.CONNECTION_POOL_SIZE}");
#endif
            Task.Run(handler.ProcessRequestsAsync);
#if DEBUG_HANDLER
            Console.WriteLine($"Handler #{_handlers.Count} set for processing.");
#endif
            return handler;
        }


        /// <summary>
        /// Unregisters the specified <see cref="DbRequestHandler"/> instance.
        /// </summary>
        /// <param name="handler">The handler to unregister.</param>
        /// <returns>A boolean value indicating whether the handler was successfully unregistered or not.</returns>
        private static bool UnregisterHandler(DbRequestHandler handler)
        {
            return _handlers.TryTake(out handler);
        }


        /// <summary>
        /// This method processes database requests by dequeuing them from the request queue and assigning them to a suitable
        /// <see cref="DbRequestHandler"/>. It also periodically cleans up inactive request handlers and handles session cleanup. 
        /// </summary>
        /// <returns>A task that completes when the method finishes processing requests.</returns>
        private static async Task ProcessRequestsAsync()
        {
            Stopwatch sw = Stopwatch.StartNew();


            while (true)
            {

                //requests found, attempt dequeue
                if (_requests.TryDequeue(out DbRequest request))
                {

#if DEBUG_HANDLER
                    Console.WriteLine($"[DbRequestManager] dequeueing request (Total: {_requests.Count})");
#endif
                    await _handlerSemaphore.WaitAsync();
                    bool success = await HandleRequestAsync(request);
                    _handlerSemaphore.Release();

                    //if the request failed, add it back to the queue to be processed again
                    if (!success && request.Status != DbRequest.RequestStatus.COMPLETED)
                    {
                        _requests.Enqueue(request);
                    }
                }
                else
                {
                    sw.Restart();
                    //do some account cleanup
                    foreach (KeyValuePair<SessionData, DbRequestHandler> pair in _sessionHandlerMap)
                    {
                        if (pair.Key.IsAuthenticated())
                            continue;

                        if (_sessionHandlerMap.TryRemove(pair.Key, out DbRequestHandler handler))
                            handler.HandlingCount--;

                    }
                    sw.Stop();

                    //no requests found, wait for rest of SLEEP_TIME
                    await Task.Delay(DbConstants.SLEEP_TIME - (int)sw.ElapsedMilliseconds);
                    continue;
                }

            }
        }


        /// <summary>
        /// Handles a request by attempting to get the associated <see cref="DbRequestHandler"/> for the session,
        /// updating the request status, and enqueuing the request for processing.
        /// </summary>
        /// <param name="request">The request to be handled.</param>
        /// <returns>Returns true if the request was successfully handled, false otherwise.</returns>
        private static async Task<bool> HandleRequestAsync(DbRequest request)
        {

#if DEBUG_HANDLER
            Console.WriteLine($"[DbRequestManager] handling request: {request.GetType().Name}");
#endif
            //user logged out, cancel request
            if (!request.GetSessionData().IsAuthenticated())
            {
                //might not want, since user can log out
                //but data still needs to be manipulated
                request.Status = DbRequest.RequestStatus.COMPLETED;
                return false;
            }

            DbRequestHandler? handler = await GetHandlerForSession(request.GetSessionData());

            if (handler == null)
                return false;

            //update request status
            request.Status = DbRequest.RequestStatus.PROCESSING;
#if DEBUG_HANDLER
            Console.WriteLine($"[DbRequestManager] set status to processing for request: {request.GetType().Name}");
#endif

            //enqueue request then return immeadiately
            return await handler.HandleRequestAsync(request);

        }




        /// <summary>
        /// Attempts to get a handler for a session by looking through the session-handler map for an existing handler,
        /// or registering a new handler if a suitable one is not found.
        /// </summary>
        /// <param name="sessionData">The session for which a handler is being requested.</param>
        /// <returns>Returns a DbRequestHandler if one is available, null otherwise.</returns>
        private static async Task<DbRequestHandler?> GetHandlerForSession(SessionData sessionData)
        {

#if DEBUG_HANDLER
            Console.WriteLine($"[DbRequestManager] Grabbing handler for session: {sessionData.GetType().Name}");
#endif
            if (_sessionHandlerMap.TryGetValue(sessionData, out DbRequestHandler? handler))
            {
#if DEBUG_HANDLER
                Console.WriteLine($"[DbRequestManager] found existing handler for session: {sessionData.GetType().Name}");
#endif
                return handler;
            }

#if DEBUG_HANDLER
            Console.WriteLine($"[DbRequestManager] Searching for suitable handler (Total handlers: {_handlers.Count})");
#endif

            int requestCount = int.MaxValue;
            foreach (DbRequestHandler h in _handlers)
            {

                if (h.HandlingCount < DbConstants.MAX_ACCOUNTS_PER_HANDLER)
                    if (!h.IsBusy)
                    {
                        handler = h;
#if DEBUG_HANDLER
                        Console.WriteLine($"[DbRequestManager] Found suitable handler with request count: {h.Requests.Count}");
#endif

                        break;
                    }
                    else if (h.Requests.Count < requestCount)
                    {

                        handler = h;
#if DEBUG_HANDLER
                        Console.WriteLine($"[DbRequestManager] Found suitable handler with request count: {h.Requests.Count}");
#endif
                        requestCount = h.Requests.Count;
                    }
            }

            //no handlers were chosen, choose first handler
            //(this is unlikely to ever happen)
            if (handler == null)
                handler = _handlers.Where(h => h.HandlingCount < DbConstants.MAX_ACCOUNTS_PER_HANDLER).FirstOrDefault();

            if (handler == null)
                handler = await RegisterHandler();

            if (handler != null)
            {
#if DEBUG_HANDLER
                Console.WriteLine($"[DbRequestManager] Registering a new handler");
#endif
                //this shouldn't happen since base case did not return
                if (_sessionHandlerMap.TryAdd(sessionData, handler))
                    handler.HandlingCount++;
            }

#if DEBUG_HANDLER
            Console.WriteLine($"[DbRequestManager] found new handler for session: {sessionData.GetType().Name}");
#endif
            return handler;

        }


        /// <summary>
        /// Enqueues a database request to the request queue for processing.
        /// </summary>
        /// <param name="request">The request to be enqueued.</param>
        /// <returns>Returns true if the request was successfully enqueued, false otherwise.</returns>
        private static bool EnqueueRequest(DbRequest request)
        {

#if DEBUG_HANDLER
//            Console.WriteLine($"[DbRequestManager] enqueueing request: {request.GetType().Name}");
#endif
            if (request == null)
                return false;

            _requests.Enqueue(request);

#if DEBUG_HANDLER
//            Console.WriteLine($"[DbRequestManager] request enqueued: {request.GetType().Name}");
#endif
            return true;
        }


        #region Request Methods

        /// <summary>
        /// Saves the data within the <see cref="RecordData"/> object to the database.
        /// </summary>
        /// <param name="data">The data object to saved from.</param>
        /// <returns>Returns true if the request was successfully enqueued, false otherwise.</returns>
        public static bool Save(RecordData data)
        {
            //probably should've used interfaces for this part...
#if DEBUG_HANDLER
//            Console.WriteLine($"[DbRequestManager] new save request for object: {data.GetType().Name}; Total requests: {_requests.Count}");
#endif
            DbRequest request;
            switch (data)
            {
                case Account account:
                    request = new PersistColleagueDataRequest(account);
                    break;
                case ProfileData profile:
                    request = new PersistProfileDataRequest(profile);
                    break;
                case SkillData skill:
                    request = new PersistSkillDataRequest(skill);
                    break;
                case EducationData education:
                    request = new PersistEducationDataRequest(education);
                    break;
                case CertificationData certification:
                    request = new PersistCertificationDataRequest(certification);
                    break;
                case WorkData work:
                    request = new PersistWorkDataRequest(work);
                    break;
                default:
                    request = null;
                    break;
            }

            return EnqueueRequest(request);
        }


        /// <summary>
        /// Loads the account data from the database based on the given session data. (This is used for initial login)
        /// </summary>
        /// <param name="sessionData">The session data of the account to be loaded.</param>
        /// <returns>Returns the DbRequest object representing the load request.</returns>
        public static DbRequest LoadAccount(SessionData sessionData) => LoadRequest(new LoadColleagueDataRequest(sessionData));


        /// <summary>
        /// Loads the account data from the database based on the given account object. (This is used for refreshing data)
        /// </summary>
        /// <param name="account">The Account object for which the data is to be loaded.</param>
        /// <returns>Returns the DbRequest object representing the load request.</returns>
        public static DbRequest LoadAccount(Account account) => LoadAccount(account.Data);


        /// <summary>
        /// Loads the profiles of a colleague into the Account class. This method populates the <see cref="Account.SavedProfiles"/> dictionary.
        /// </summary>
        /// <param name="account">The Account object for which the profiles will be loaded.</param>
        /// <returns>A database request object representing the request to load the colleague's profiles.</returns>
        public static DbRequest LoadColleagueProfiles(Account account) => LoadRequest(new LoadProfileDataRequest(account));


        /// <summary>
        /// Loads the skills of a colleague into the Account class. This method populates the <see cref="Account.SavedSkills"/> dictionary.
        /// </summary>
        /// <param name="account">The Account object for which the skills will be loaded.</param>
        /// <returns>A database request object representing the request to load the colleague's skills.</returns>
        public static DbRequest LoadColleagueSkills(Account account) => LoadRequest(new LoadSkillDataRequest(account));


        /// <summary>
        /// Loads the education history of a colleague into the Account class. This method populates the <see cref="Account.SavedEducationHistory"/> dictionary.
        /// </summary>
        /// <param name="account">The Account object for which the education history will be loaded.</param>
        /// <returns>A database request object representing the request to load the colleague's education history.</returns>
        public static DbRequest LoadColleagueEducationHistory(Account account) => LoadRequest(new LoadEducationDataRequest(account));


        /// <summary>
        /// Loads the certifications of a colleague into the Account class. This method populates the <see cref="Account.SavedCertifications"/> dictionary.
        /// </summary>
        /// <param name="account">The Account object for which the certifications will be loaded.</param>
        /// <returns>A database request object representing the request to load the colleague's certifications.</returns>
        public static DbRequest LoadColleagueCertifications(Account account) => LoadRequest(new LoadCertificationDataRequest(account));


        /// <summary>
        /// Loads the work history of a colleague into the Account class. This method populates the <see cref="Account.SavedWorkHistory"/> dictionary.
        /// </summary>
        /// <param name="account">The Account object for which the work history will be loaded.</param>
        /// <returns>A database request object representing the request to load the colleague's work history.</returns>
        public static DbRequest LoadColleagueWorkHistory(Account account) => LoadRequest(new LoadWorkDataRequest(account));


        /// <summary>
        /// Helper method to enqueue a database request and return it.
        /// </summary>
        /// <param name="request">The database request object to be enqueued.</param>
        /// <returns>The database request object passed as a parameter.</returns>
        private static DbRequest LoadRequest(DbRequest request)
        {
            EnqueueRequest(request);
            return request;
        }


        /// <summary>
        /// Searches through all profiles containing the specified keyphrase.
        /// </summary>
        /// <param name="account">The account requesting the search.</param>
        /// <param name="keyphrase">The keyphrase to search for.</param>
        /// <returns>A database request object representing the search request.</returns>
        public static DbRequest SearchProfilesForKeyphrase(Account account, string keyphrase)
        {
            DbRequest request;
            EnqueueRequest(request = new SearchProfileKeyphraseRequest(account, keyphrase));
            return request;
        }
        #endregion


        #region Startup code
        /// <summary>
        /// Initializes the database by executing the startup SQL script.
        /// </summary>
        private static void InitializeDatabase()
        {
            //we won't use a handler for this since this is a one time operation
            using (SqliteConnection conn = new SqliteConnection(_connectionString))
            {
                conn.Open();
                Console.WriteLine("Server Version: " + conn.ServerVersion);
                using (SqliteCommand cmd = new SqliteCommand(DbConstants.StartupSql(), conn))
                {
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqliteException e)
                    {

                    }
                    catch (Exception e)
                    {

                    }

                }
            }
        }
        #endregion

    }

}
