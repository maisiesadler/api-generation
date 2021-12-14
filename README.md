# API Generation 🤖

This project uses [roslyn](https://github.com/dotnet/roslyn) to create an API from open api specification.

This ensures that the application is always returning models that match the defined API specification.

Creates
- Controller
- Models
- Interface for an interactor

The application can then implement the interactor with any custom logic.

When the tool runs new files will be generated in the application.

View post on the idea behind this project [here](https://www.maisiesadler.co.uk/api-generation/blog).

## To Run

- Generate example project using `dotnet run --project src/OpenApiSpecGeneration.ApiGeneration` or using the script `./run.sh`
- Run example project using `dotnet run --project example/`

## Contributing

## What's supported

### Reading open api specs

The [console app](src/OpenApiSpecGeneration.ApiGeneration.Console) takes an input json openapi spec and outputs C# files to the given output directory.

Our API specs are defined in YAML, since this is not easily parsed in dotnet I have created a tool [yamltojson](./yamltojson) that takes a YAML file and outputs a JSON file which can then be used by the console app.

### Creating APIs from one open api spec
