cd yamltojson
npm run start ../dist.yaml ../dist.json
cd ../

dotnet run --project src/OpenApiSpecGeneration.ApiGeneration.Console \
    generate-mock -i dist.json -o example-2/generated -n Example

