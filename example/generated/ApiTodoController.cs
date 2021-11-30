using Microsoft.AspNetCore.Mvc;
using Example.Models;

namespace Example
{
    [ApiController]
    [Route("/api/Todo")]
    public class ApiTodo
    {
        public ApiTodo(IGetApiTodoInteractor getApiTodoInteractor, IPostApiTodoInteractor postApiTodoInteractor)
        {
        }

        [HttpGet]
        public void Get()
        {
        }

        [HttpPost]
        public void Post()
        {
        }
    }
}