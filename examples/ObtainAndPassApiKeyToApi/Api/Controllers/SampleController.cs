using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace Api.Controllers
{
    [Authorize]
    [RoutePrefix("api/sample")]
    public class SampleController : ApiController
    {
        [Route("")]
        public IEnumerable<object> Get()
        {
            return (User.Identity as ClaimsIdentity).Claims.Select(c => new { c.Type, c.Value });
        }
    }
}