using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Auth0.Owin.Tests
{
    public class HttpUtilitiesTests
    {
        [Fact]
        public void HandlesEmptyString()
        {
            string queryString = "";

            var result = HttpUtilities.ParseQueryString(queryString);

            Assert.Empty(result);
        }

        [Fact]
        public void HandlesSingleParameter()
        {
            string queryString = "ru=one";

            var result = HttpUtilities.ParseQueryString(queryString);

            Assert.Equal(1, result.Count);
            Assert.Equal("one", result["ru"]);
        }

        [Fact]
        public void HandlesMultipleParameters()
        {
            string queryString = "xsrf=blah&ru=one";

            var result = HttpUtilities.ParseQueryString(queryString);

            Assert.Equal(2, result.Count);
            Assert.Equal("blah", result["xsrf"]);
            Assert.Equal("one", result["ru"]);
        }

        [Fact]
        public void IgnoresQuestionmark()
        {
            string queryString = "?ru=one";

            var result = HttpUtilities.ParseQueryString(queryString);

            Assert.Equal(1, result.Count);
            Assert.Equal("one", result["ru"]);
        }

        [Fact]
        public void HandlesEncodedString()
        {
            string queryString = "ru=" + WebUtility.UrlEncode("http://www.test.com?query=one");

            var result = HttpUtilities.ParseQueryString(queryString);

            Assert.Equal(1, result.Count);
            Assert.Equal("http://www.test.com?query=one", result["ru"]);
        }


    }
}
