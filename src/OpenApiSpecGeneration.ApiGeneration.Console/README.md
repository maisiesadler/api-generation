# ApiGeneration.Console

This is a console application to invoke the API Generation.

## Usage

```sh
dotnet run --project src/OpenApiSpecGeneration.ApiGeneration.Console \
    generate -i <input-file> -o <output-directory> -n <namespace>
```

### Example

```sh
dotnet run --project src/OpenApiSpecGeneration.ApiGeneration.Console \
    generate -i definition.yaml -o example/generated -n Example
```
