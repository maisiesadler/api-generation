using Microsoft.AspNetCore.Mvc;
using Example.Interactors;
using Example.Models;

namespace Example
{
    [ApiController]
    [Route("/api/Todo/{id}")]
    public class ApiTodoId : ControllerBase
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
        public async Task<IActionResult> Get([FromHeader] string xRequestId, [FromRoute] int id, [FromQuery] string type)
        {
            var result = await _getApiTodoIdInteractor.Execute(xRequestId, id, type);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Put()
        {
            await _putApiTodoIdInteractor.Execute();
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            await _deleteApiTodoIdInteractor.Execute();
            return Ok();
        }
    }
}