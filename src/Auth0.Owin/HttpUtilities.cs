using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Auth0.Owin
{
    internal static class HttpUtilities
    {
        internal static NameValueCollection ParseQueryString(string query)
        {
            NameValueCollection queryParameters = new NameValueCollection();
            string[] querySegments = query.Split('&');
            foreach (string segment in querySegments)
            {
                string[] parts = segment.Split('=');
                if (parts.Length > 1)
                {
                    string key = parts[0].Trim('?', ' ');
                    string val = parts[1].Trim();

                    queryParameters.Add(key, WebUtility.UrlDecode(val));
                }
            }

            return queryParameters;
        }
    }
}
