using Microsoft.AspNetCore.Mvc;
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
        }

        [HttpGet]
        public void Get()
        {
        }

        [HttpPut]
        public void Put()
        {
        }

        [HttpDelete]
        public void Delete()
        {
        }
    }
}