cd yamltojson
npm run start ../definition.yaml ../definition.json
cd ../

dotnet run --project src/OpenApiSpecGeneration.ApiGeneration.Console \
    generate-mock -i definition.json -o example/generated -n Example

