using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace TheDistributorsWebService.Controllers
{
    public class TestController : ApiController
    {
       
        [HttpGet]
        [Route("api/test/getservertime")]
        public IHttpActionResult Get()
        {
            return Ok("Server time is  : " + DateTime.Now.ToString());
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("api/test/poststring")]
        public IHttpActionResult Post([FromBody] string title)
        {
            return Content(HttpStatusCode.OK, title,null, "application/json");
        }
    }
}
