using Microsoft.AspNetCore.Mvc;
using Example.Interactors;
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
            _getApiTodoInteractor = getApiTodoInteractor;
            _postApiTodoInteractor = postApiTodoInteractor;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return await _getApiTodoInteractor.Execute();
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            return await _postApiTodoInteractor.Execute();
        }
    }
}