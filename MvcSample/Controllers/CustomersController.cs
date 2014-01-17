using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MvcSample.Controllers
{
    public class CustomersController : ApiController
    {
        // GET api/<controller>
        [Authorize]
        public IEnumerable<string> Get()
        {
            // you could access here the user_id by doing 
            // ClaimsPrincipal.Current.Claims.SingleOrDefault(c => c.Type == "sub").Value

            return new string[] { "John Doe", "Nancy Davolo" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}