---
layout: default
title: API Generation 🤖
---

Generate dotnet APIs, mocks and clients from openapi specifications.

View blog [here](https://www.maisiesadler.co.uk/api-generation/blog).

This ensures that the application is always returning models that match the defined API specification.

The easiest way to describe what the project does is through an example of what is generated:

For the following yaml

```yaml
openapi: 3.0.1
paths:
  /api/Todo:
    get:
      tags:
        - Todo
      operationId: ApiTodoGet
      responses:
        '200':
          description: Success
          content:
            application/json:
              schema:
                type: array
                items:
                  $ref: '#/components/schemas/ToDoItem'
components:
  schemas:
    ToDoItem:
      type: object
      properties:
        id:
          type: integer
          format: int32
        name:
          type: string
          nullable: true
        isCompleted:
          type: boolean
```

A controller, model and interface are generated.

Controller:

```csharp
[ApiController]
[Route("/api/Todo")]
public class ApiTodo : ControllerBase
{
    private readonly IGetApiTodoInteractor _getApiTodoInteractor;
    public ApiTodo(IGetApiTodoInteractor getApiTodoInteractor)
    {
        _getApiTodoInteractor = getApiTodoInteractor;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _getApiTodoInteractor.Execute();
        return Ok(result);
    }
}
```

Model:

```csharp
public record ToDoItem
{
    [JsonPropertyName("id")]
    public int? Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("isCompleted")]
    public bool? IsCompleted { get; init; }
}
```

Interface:

```csharp
public interface IGetApiTodoInteractor
{
    Task<ToDoItem[]> Execute();
}
```

This can either be implemented or use the AutoFixture implementation, to create a generated response:

```csharp
public class GetApiTodoInteractor : IGetApiTodoInteractor
{
    private readonly Fixture _fixture = new Fixture();
    public async Task<ToDoItem[]> Execute()
    {
        return _fixture.Create<ToDoItem[]>();
    }
}
```

---

Generated files for [definition.yaml](./definition.yaml) can be found [here](./example/generated/README.md)

The application can then implement the interactor with any custom logic.

When the tool runs new files will be generated in the application.

## To Run

- Generate example project using `dotnet run --project src/OpenApiSpecGeneration.ApiGeneration` or using the script `./run.sh`
- Run example project using `dotnet run --project example/`

## How it works

This project uses [roslyn](https://github.com/dotnet/roslyn) to create an API from open api specification.

## Use it




