using Microsoft.Data.Sqlite;

namespace SCCPP1.Database
{
    public static class DbRequestManager
    {

        private static readonly object lockObject = new object();

        private static Queue<DbRequestHandler> Handlers { get; set; }
        private static Queue<Task> Tasks { get; set; }

        static DbRequestManager()
        {
            Handlers = new Queue<DbRequestHandler>();
            Tasks = new Queue<Task>();
        }


        public static SqliteConnection RequestConnection(DbRequestHandler handler)
        {
            lock (lockObject)
            {
                if (!HasConnection(handler))
                {
                    return null;
                }
                return null;
            }
        }

        private static bool HasConnection(DbRequestHandler handler)
        {
            lock (lockObject)
            {
                return Handlers.Contains(handler);
            }
        }

        public static async Task AddRequestAsync(DbRequest request)
        {
            lock (lockObject)
            {
                Tasks.Enqueue(HandleRequestAsync(request));
            }
            await Task.WhenAny(Tasks);
        }

        private static async Task HandleRequestAsync(DbRequest request)
        {
            var handler = await GetAvailableHandler();
            if (handler != null)
            {
                handler.HandleRequest(request);
            }
        }

        private static async Task<DbRequestHandler> GetAvailableHandler()
        {
            DbRequestHandler handler = null;
            while (handler == null)
            {
                lock (lockObject)
                {
                    if (Handlers.Count > 0)
                    {
                        handler = Handlers.Dequeue();
                    }
                }
                if (handler != null && !handler.IsBusy)
                {
                    return handler;
                }
                else if (handler != null)
                {
                    lock (lockObject)
                    {
                        Handlers.Enqueue(handler);
                    }
                }
                await Task.Delay(50);
            }
            return null;
        }
    }

}
