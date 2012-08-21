using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MvcApp.Demo.Models;

namespace MvcApp.Demo.Controllers
{
    public class ToDoController : ApiController
    {
        public void Post(ToDoItem item)
        {
            // add item
        }

        public HttpResponseMessage Delete(Guid id)
        {
            // delete item
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        public void Put(Guid id, ToDoItem item)
        {
            // update item
        }
    }
}
