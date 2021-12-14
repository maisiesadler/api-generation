# Yaml To Json

YAML is not easily read in dotnet, I created this node application to parse the yaml file and generate a json equivalent.

## Usage

```sh
npm run start <input-file> <output-file>
```

### Example
```sh
npm run start ../dist.yaml ../dist.json
```

## What was the problem?

The parser complains if it finds properties in the YAML that are not on the models.
This would be an issue because you can define a lot of extra metadata in the openapi specs that we are not interested in for the API.
