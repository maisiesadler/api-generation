# API Generation 🤖

This project uses [roslyn](https://github.com/dotnet/roslyn) to create an API from open api specification.

This ensures that the application is always returning models that match the defined API specification.

Creates
- Controller
- Models
- Interface for an interactor

The application can then implement the interactor with any custom logic.

When the tool runs new files will be generated in the application.

## To Run

- Generate example project using `dotnet run --project src/OpenApiSpecGeneration.ApiGeneration` or using the script `./run.sh`
- Run example project using `dotnet run --project example/`
