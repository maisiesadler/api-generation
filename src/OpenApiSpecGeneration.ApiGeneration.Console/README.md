# ApiGeneration.Console

This is a console application to invoke the API Generation.

It takes in a JSON file, if the spec is a yaml file then run the file through the [yamltojson](../yamltojson) tool first.

## Usage

```sh
dotnet run --project src/OpenApiSpecGeneration.ApiGeneration.Console \
    generate -i <input-json-file> -o <output-directory> -n <namespace>
```

### Example

```sh
dotnet run --project src/OpenApiSpecGeneration.ApiGeneration.Console \
    generate -i definition.json -o example/generated -n Example
```
