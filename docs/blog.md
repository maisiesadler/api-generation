# Api Definition Generation

I watched [this]() video around microservices done right, one of the things that is discussed is that they generate a lot of their code from config files. The config files are linted to give consistent APIs and then they generate APIs, clients, mocks and tests.

At TrueLayer we maintain open api specifications for all of our services and then the application is designed against the open api spec. It would be great if we had some assertion that the API we create is exactly as in the spec.

Could we generate a c# API from an openapi specification?

## The dream

I wanted to take an open api specification and generate models and controllers that match that spec.
I wanted it so you didn't have to edit the generated files, so they can be deleted and regenerated, and that the generated files compile without extra code.
The generated code shouldn't depend on the tool, in case it isn't maintained or the API simply wants to diverge from it.

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

- easier to use than writing the API yourself so that people actually use it

### Benefits

- 🤩 Running API code is consistent with spec
- 🤩 Apply best practices

## The implementation

### Generating Code

I knew that [source generators](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) generate code and are relatively new, I was keen to give them a go with the hopes that they would be what I was looking for.

Source Generators are a way to add generated code at compile time, I felt that the code would need to be visible for the user to actually implement the interface and use it in the API.
I then went down a bit of a rabbit hole of "well, I could read the file and compile an assembly with the generated code, then reference that compiled assembly elsewhere" before realising that I was making it too complicated. All I needed to do was generate some code from code and what I wanted was the more general .NET Compiler Platform [Roslyn](https://github.com/dotnet/roslyn).

I found [this](https://www.stevejgordon.co.uk/getting-started-with-the-roslyn-apis-writing-code-with-code) article (written by the author of [this](https://www.stevejgordon.co.uk/introduction-to-httpclientfactory-aspnetcore) wonderful series of posts on HttpClientFactory - well worth a read!) that had some reference code that I could get started with.

The code looks weird when you first start working with it, but the API is pretty consistent so you start to get a feel for what the functions are expecting.

Once I had some basic code working - generate a class, save it as a file I started to look at how I could write tests against it. I'm a big advocate for TDD and the confidence it gives you while you're developing.
The code to generate the file looks like this:

```csharp
var node = GenerateModel(); // node contains usings, namespace and class to create one file
await using var streamWriter = new StreamWriter(fileLocation);
node.NormalizeWhitespace().WriteTo(streamWriter);
```

I'd like my tests to test as much as possible and be easy to work with, I didn't want to read files or mess around with TextWriter and so decided that my test suite would work with the public interface of `GenerateModel` and assert against the roslyn types.

### Generating useful code

Now I can generate some code from code I wanted to be able to generate the controllers, interactors and models to create an API.

- hard to work out what to test for if i don't know what i'm working with
- first generate something, work out what is useful and create tests

- work like this for a bit start to understand what needs to be created

#### First the models

TDD approach is to write a test to describe the thing you're implementing and then only add the code required for that feature. In this case I wasn't sure what the roslyn types should look like so there was a bit of back and forth between trying stuff/debugging the types and being able to write the asserts.

First I needed to create a type with the right name, the test looked something like this:

```csharp
// Arrange
var toDoItemProperties = new Dictionary<string, OpenApiComponentProperty>
{
    { "id", new OpenApiComponentProperty("integer", default, default) },
};
var componentSchemas = new Dictionary<string, OpenApiComponentSchema>
{
    { "ToDoItem", new OpenApiComponentSchema("object", toDoItemProperties) }
};
var components = new OpenApiComponent(componentSchemas);
var spec = new OpenApiSpec(new Dictionary<string, OpenApiPath>(), components);

// Act
var models = ApiGenerator.GenerateModels(spec);

// Assert
var model = Assert.Single(models);
```

Once I could generate _something_ I started to dig into what actually needed to be created and padding out the asserts.

I worked iteratively like this while implementing different bits of the model, adding attributes for `JsonPropertyName`, adding the namespace, etc. Even though the roslyn models look strange at first they are pretty consistent and you do get a feel for what they might look like and where to look for the right values.

I also set up an example project so I would know when the types I was creating actually generated something that compiled and could be worked with. This was created using `dotnet new webapi` and removing the default WeatherController. The generated code is all under `./generated` and is deleted and recreated each time the tool runs.
