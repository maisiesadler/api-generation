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
            return await _getApiTodoIdInteractor.Execute();
        }

        [HttpPut]
        public async Task<IActionResult> Put()
        {
            return await _putApiTodoIdInteractor.Execute();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            return await _deleteApiTodoIdInteractor.Execute();
        }
    }
}