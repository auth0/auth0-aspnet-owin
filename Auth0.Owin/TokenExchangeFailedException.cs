using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth0.Owin
{
    public class TokenExchangeFailedException : Exception
    {
        public string ErrorDescription
        {
            get;
            private set;
        }

        public TokenExchangeFailedException(string errorDescription, Exception innerException)
            : base(errorDescription ?? "Token Exchange Failed", innerException)
        {

        }
    }
}
