using Microsoft.AspNetCore.Mvc;
using Example.Models;

namespace Example
{
    [ApiController]
    [Route("/api/Todo")]
    public class ApiTodo
    {
        private readonly IGetApiTodoInteractor _getApiTodoInteractor;
        private readonly IPostApiTodoInteractor _postApiTodoInteractor;
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