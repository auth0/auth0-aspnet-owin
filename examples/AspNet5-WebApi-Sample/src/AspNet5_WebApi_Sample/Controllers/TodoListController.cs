using AspNet5_WebApi_Sample.Models;
using Microsoft.AspNet.Mvc;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace AspNet5_WebApi_Sample.Controllers
{
	[Authorize]
    [Route("api/[controller]")]
    public class TodoListController : Controller
    {
		static ConcurrentBag<TodoItem> todoStore = new ConcurrentBag<TodoItem>();

		// GET: api/todolist
		[HttpGet]
		public IEnumerable<TodoItem> Get()
		{
			string owner = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
			return todoStore.Where(t => t.Owner == owner).ToList();
		}

		// POST api/todolist
		[HttpPost]
		public void Post([FromBody]TodoItem item)
		{
			item.Owner = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
			todoStore.Add(item);
		}
	}
}
