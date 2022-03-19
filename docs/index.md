---
layout: default
title: API Generation 🤖
---

Generate dotnet APIs, mocks and clients from openapi specifications.

View blog [here](https://www.maisiesadler.co.uk/api-generation/blog).

This ensures that the application is always returning models that match the defined API specification.

The easiest way to describe what the project does is through an example of what is generated:
- [Controller](example/generated/ApiTodoController.cs) created for [path](./definition.json#L8)
- [Model](example/generated/models/ToDoItem.cs) created for [schema](./definition.json#L56)
- [Interface](example/generated/interactors/IGetApiTodoInteractor.cs) describing each [route](./definition.json#L8)

The application can then implement the interactor with any custom logic.

When the tool runs new files will be generated in the application.

## To Run

- Generate example project using `dotnet run --project src/OpenApiSpecGeneration.ApiGeneration` or using the script `./run.sh`
- Run example project using `dotnet run --project example/`

## How it works

This project uses [roslyn](https://github.com/dotnet/roslyn) to create an API from open api specification.

## Use it




