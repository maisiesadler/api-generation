using Microsoft.AspNetCore.Mvc;
using Example.Interactors;
using Example.Models;

namespace Example
{
    [ApiController]
    [Route("/api/Todo")]
    public class ApiTodo : ControllerBase
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
            var result = await _getApiTodoInteractor.Execute();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var result = await _postApiTodoInteractor.Execute();
            return Ok(result);
        }
    }
}