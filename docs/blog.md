# Api Definition Generation

After watching [this](https://www.youtube.com/watch?v=j6ow-UemzBc) video about microservices done right and hearing about how they use custom config files to generate APIs, clients, mocks and tests. I started to think about how powerful that could be, and how we could make it useful at work.

At TrueLayer we maintain open api specifications for all of our services and then the application is designed against the open api spec. It would be great if we had some assertion that the code we create is exactly as in the spec.

Could we generate a c# API from an openapi specification?

## The Idea

So I wanted to take an open api specification and generate models and controllers that match that spec.

Ideally the generated files could be deleted and regenerated so they would need to compile without extra code.
The generated code shouldn't depend on the tool, in case it isn't maintained or the API simply wants to diverge from it.

The idea I had was that each route could have an interface that matched it, the controller would then resolve the interface and return the model.

Controller method:

```csharp
[HttpGet]
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

This interface could then be implemented in the service.

The other requirement was that the tool needed to be easier to use than writing the API yourself so that people actually use it.

### Benefits

- 🤩 Running API code is consistent with spec
- 🤩 Consistent code across all APIs generated with this tool
- 🤩 Apply best practices in one place

## The implementation

### Generating Code

I knew that [source generators](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) generate code and are relatively new, I was keen to give them a go with the hopes that they would be what I was looking for.

Source Generators are a way to add generated code at compile time, I felt that the code would need to be visible for the user to actually implement the interface and use it in the API.
I then went down a bit of a rabbit hole of "well, I could read the file and compile an assembly with the generated code, then reference that compiled assembly elsewhere" before realising that I was making it too complicated. All I needed to do was generate some code from code and what I wanted was the more general .NET Compiler Platform [Roslyn](https://github.com/dotnet/roslyn).

I found [this](https://www.stevejgordon.co.uk/getting-started-with-the-roslyn-apis-writing-code-with-code) article (written by the author of [this](https://www.stevejgordon.co.uk/introduction-to-httpclientfactory-aspnetcore) wonderful series of posts on HttpClientFactory - well worth a read!) that had some reference code that I could get started with.

The code looks weird when you first start working with it, but the API is pretty consistent so you start to get a feel for what the functions are expecting.

Once I had some basic code working - generate a class, save it as a file - I started to look at how I could write tests against it. I'm a big advocate for TDD and the confidence it gives you while you're developing.
The code to generate the file looks like this:

```csharp
var node = GenerateModel(); // node contains usings, namespace and class to create one file
await using var streamWriter = new StreamWriter(fileLocation);
node.NormalizeWhitespace().WriteTo(streamWriter);
```

I'd like my tests to test as much as possible and be easy to work with, I didn't want to read files or mess around with TextWriter and so decided that my test suite would work with the public interface of `GenerateModel` and assert against the roslyn types.

### Generating useful code

Now I can generate some code from code I wanted to be able to generate the controllers, interactors and models to create an API.

#### First the models

The TDD approach is to write a test to describe the thing you're implementing and then only add the code required for that feature. In this case I wasn't sure what the roslyn types should look like so there was a bit of back and forth between trying stuff/debugging the types and being able to write the asserts.

The first bit of behaviour I wanted to create was a type with the right name, the test looked something like this:

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

I didn't know how to get the name yet so a quick debug,

![The model](./images/single-record.png)

Then filling out the asserts...

```csharp
// Assert
var model = Assert.Single(models);
Assert.Equal("ToDoItem", model.Identifier.Value);
var classModifier = Assert.Single(recordDeclarationSyntax.Modifiers);
Assert.Equal("public", classModifier.Value);
```

I worked iteratively like this while implementing different bits of the model, adding attributes for `JsonPropertyName`, adding the namespace, etc until I got a feel for what the roslyn models look like. They're a bit strange at first but they do start to make sense, honest.

I also set up an example project so I would know when the types I was creating actually generated something that compiled and could be worked with. This was created using `dotnet new webapi` and removing the default controller. The generated code is all under `./generated` and is deleted and recreated each time the tool runs. View the example project [here](https://github.com/maisiesadler/api-generation/tree/main/example).

### Generic or Specific

One of the aims of this would be that it keeps code consistent across projects and so that means the generated code is opionated and I was making a lot of decisions about what the code looks like along the way.

An example of this would be choosing between a `class` and a `record` for the models.
In my opnion best practice for models would be records with init only setters.
Records are available from C# 9, which is only supported by .NET 5 and higher.
If this is to be used at work then it isn't a problem, but if this will be an open source library used by the masses then I should probably opt for class or make it configurable.

![The General Problem](./images/the_general_problem.png)

I put the idea of my API generation tool becoming the next Newtonsoft.Json and this being built into dotnet to one side and decide that it can always be added in as an option later.

In the balance between configurable and opinionated this definitely leans towards opionated.

## The Result

The idea works! I can generate a usable API from an openapi spec - this is really cool!

I tried it against another openapi spec and found that the openapi definition types I have created are probably not very well defined against the spec.
I think if this would work then enforcing some code styles around definitions that could/should be used would be required.

Is the solution [Jimmy-Proof](https://blog.codinghorror.com/new-programming-jargon/#10)?
It's definitely more complicated than writing the API manually and so we need to be cautious the benefits of using it outweigh the extra complexity.
The generated API can be seperated without the tool and I would always argue for automation over manual steps.

One of the cool things about this solution are that, since it only creates a part of the project, the project is still free to implement other functionality as they see fit.

Need to decide if it's worth investing more time into this project or a similar new project based on this idea.

How can we make this actually useful, and easier to use than making the change yourself?
Where would it run?

