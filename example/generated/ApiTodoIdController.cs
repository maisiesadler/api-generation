using Microsoft.AspNetCore.Mvc;
using Example.Models;

namespace Example
{
    [ApiController]
    [Route("/api/Todo/{id}")]
    public class ApiTodoId
    {
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