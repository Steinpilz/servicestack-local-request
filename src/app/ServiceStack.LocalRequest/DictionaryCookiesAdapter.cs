using System;
using System.Collections.Generic;
using System.Net;
using ServiceStack.ServiceHost;

namespace ServiceStack.LocalRequest
{
    class DictionaryCookiesAdapter : ICookies
    {
        private readonly Dictionary<string, Cookie> _dict;

        public DictionaryCookiesAdapter(Dictionary<string, Cookie> dict)
        {
            if (dict == null) throw new ArgumentNullException(nameof(dict));
            _dict = dict;
        }

        public void AddCookie(Cookie cookie)
        {
            _dict.Add(cookie.Name, cookie);
        }

        public void AddPermanentCookie(string cookieName, string cookieValue, bool? secureOnly = default(bool?))
        {
            AddCookie(new Cookie
            {
                Name = cookieName,
                Value = cookieValue,
                Secure = secureOnly ?? false
            });
        }

        public void AddSessionCookie(string cookieName, string cookieValue, bool? secureOnly = default(bool?))
        {
            AddCookie(new Cookie
            {
                Name = cookieName,
                Value = cookieValue,
                Secure = secureOnly ?? false
            });
        }

        public void DeleteCookie(string cookieName)
        {
            _dict.Remove(cookieName);
        }
    }
}