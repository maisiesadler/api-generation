# Api Definition Generation

I watched [this]() video around microservices done right, one of the things that is discussed is that the APIs, mocks and tests are generated from their open api specifications.

Could we generate a c# API from an openapi spec?

At TrueLayer we maintain open api specifications for all of our services and then the application is designed against the open api spec. It would be great if we had some assertion that the API we create is exactly as in the spec.

## The dream

I wanted to take an open api specification and generate models and controllers that match that spec.
I wanted it so you didn't have to edit the generated files, so they can be deleted and regenerated, and that the generated files compile without extra code.

The idea I had was that each route could have an interface that matched it, the controller would then resolve the interface and return the model.

Controller method:

```csharp
public async Task<ThingModel> Get()
{
    return await _getThingInteractor.Execute();
}
```

Interactor interface:

```csharp
public interface IGetThingInteractor
{
    Task<ThingModel> Execute(); 
}
```

### Benefits

- 🤩 Running API code is consistent with spec
- 🤩 Apply best practices

## The implementation

I knew that [source generators](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) generate code and are relatively new, I was keen to give them a go with the hopes that they would be what I was looking for.

Source Generators are a way to add generated code at compile time, I quickly realised that the code would need to be visible for the user to actually implement the interface and use it in the API.
I then went down a bit of a rabbit hole of "well, I could read the file and compile an assembly with the generated code, then reference that compiled assembly elsewhere" before realising that I was making it too complicated. All I needed to do was generate some code from code and what I wanted was the more general .NET Compiler Platform [Roslyn](https://github.com/dotnet/roslyn).

I found [this](https://www.stevejgordon.co.uk/getting-started-with-the-roslyn-apis-writing-code-with-code) article (written by the author of [this](https://www.stevejgordon.co.uk/introduction-to-httpclientfactory-aspnetcore) wonderful series of posts on HttpClientFactory - well worth a read!) that had some reference code that I could get started with.

The code looks weird when you first start working with it, but the API is pretty consistent so you start to get a feel for what the functions are expecting.

Once I had some basic code working - generate a class, save it as a file I started to look at how I could write tests against it. I'm a big advocate for TDD and the confidence it gives you while you're developing.
I considered having my own `GeneratableClassThing` type and decided I didn't want to reinvent the wheel and so the tests are against the roslyn models before they are written to file.
