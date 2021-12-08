cd yamltojson
npm run start ../dist.yaml ../dist.json
cd ../

dotnet run --project src/OpenApiSpecGeneration.ApiGeneration.Redoc.Console
