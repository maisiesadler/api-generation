using Microsoft.AspNetCore.Mvc;
using Example.Interactors;
using Example.Models;

namespace Example
{
    [ApiController]
    [Route("/api/Todo/{id}")]
    public class ApiTodoId
    {
        private readonly IGetApiTodoIdInteractor _getApiTodoIdInteractor;
        private readonly IPutApiTodoIdInteractor _putApiTodoIdInteractor;
        private readonly IDeleteApiTodoIdInteractor _deleteApiTodoIdInteractor;
        public ApiTodoId(IGetApiTodoIdInteractor getApiTodoIdInteractor, IPutApiTodoIdInteractor putApiTodoIdInteractor, IDeleteApiTodoIdInteractor deleteApiTodoIdInteractor)
        {
            _getApiTodoIdInteractor = getApiTodoIdInteractor;
            _putApiTodoIdInteractor = putApiTodoIdInteractor;
            _deleteApiTodoIdInteractor = deleteApiTodoIdInteractor;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _getApiTodoIdInteractor.Execute();
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Put()
        {
            var result = await _putApiTodoIdInteractor.Execute();
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            var result = await _deleteApiTodoIdInteractor.Execute();
            return Ok(result);
        }
    }
}