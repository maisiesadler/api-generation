cd yamltojson
npm run start ../identity.yaml ../identity.json
cd ../

dotnet run --project src/OpenApiSpecGeneration.ApiGeneration.Console \
    generate-mock -i identity.json -o example-2/generated -n Example

