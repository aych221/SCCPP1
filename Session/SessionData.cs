using SCCPP1.User;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;

namespace SCCPP1.Session
{
    public class SessionData : IDictionary<string, object>
    {


        //This is the session ID, not necessarily the Colleages ID
        public int ID { get; set; }


        private string _username;


        ///<summary>
        /// This property is used in <see cref="Account.Equals(object)"/>.
        /// It is important that it always returns a unique value in the scope of runtime.
        /// If the value entered is null, the property will assign the value returned from
        /// <see cref="Utilities.ToSHA256Hash(string)"/> where the string is set to $"SessionDataHashCode={<see cref="object.GetHashCode()"/>}".
        ///</summary>
        public string Username
        { 
            get { return _username; }
            set
            {
                if (value == null)
                    _username = Utilities.ToSHA256Hash($"AccountHashCode={GetHashCode()}");
                else
                    _username = value;
            }
        }

        [Obsolete("Use IsAuthenticated instead.")]
        public bool SignedOn { get; set; }

        public Account Owner  { get; set; }

        //user that's logged in
        public ClaimsPrincipal User { get; set; }


        private SessionData()
        {
            _dict = new Dictionary<string, object>();
            Console.WriteLine("[SessionData] constructor called");
        }

        public SessionData(string username) : this()
        { 
            this.SignedOn = true;
            this.Username = username;
        }

        public SessionData(ClaimsPrincipal sessionUser) : this()
        {
            this.User = sessionUser;


            this.SignedOn = IsAuthenticated();
            this.Username = GetUsersNameIdentifier();
        }


        ~SessionData()
        {
            Console.WriteLine("[SessionData] destructor called");
        }

        public bool IsAuthenticated()
        {
            if (User == null || User.Identity == null)
                return false;

            return User.Identity.IsAuthenticated;

        }



        /// <summary>
        /// Gets the authentication user's email, based on what is used as their Microsoft account.
        /// </summary>
        /// <returns>User's email</returns>
        public string GetUsersEmail()
        {
            if (User == null)
                return Owner.EmailAddress;
            //return User.FindFirstValue(ClaimTypes.Email);
            return User.FindFirstValue("preferred_username");
        }

        /// <summary>
        /// Gets the authentication user's full name, based on what is displayed on their Microsoft account.
        /// </summary>
        /// <returns>User's full name</returns>
        public string GetUsersName()
        {
            if (User == null)
                return Owner.Name;
            //return User.FindFirstValue(ClaimTypes.Name);
            return User.FindFirstValue("name");
        }

        /// <summary>
        /// Gets the authentication user's name identifier for their Microsoft account.
        /// </summary>
        /// <returns>User's name identifier</returns>
        public string GetUsersNameIdentifier()
        {

            if (User == null)
                return Owner.GetUsername();
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }



        #region dictionary implementation
        private readonly Dictionary<string, object> _dict;

        public object this[string key]
        {
            get
            {
                if (!_dict.ContainsKey(key))
                    Add(key, null);

                return _dict[key];
            }
            set
            {
                _dict[key] = value;
            }
        }

        public ICollection<string> Keys => _dict.Keys;

        public ICollection<object> Values => _dict.Values;

        public int Count => _dict.Count;

        public bool IsReadOnly => false;

        public void Add(string key, object value)
        {
            _dict.Add(key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            ((IDictionary<string, object>)_dict).Add(item);
        }

        public void Clear()
        {
            _dict.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return ((IDictionary<string, object>)_dict).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((IDictionary<string, object>)_dict).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return _dict.Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return ((IDictionary<string, object>)_dict).Remove(item);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _dict.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dict.GetEnumerator();
        }
    }
    #endregion

}
