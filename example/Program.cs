using Microsoft.OpenApi.Models;
using Example.Interactors;
using Example.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDeleteApiTodoIdInteractor, DeleteApiTodoIdInteractor>();
builder.Services.AddScoped<IGetApiTodoIdInteractor, GetApiTodoIdInteractor>();
builder.Services.AddScoped<IGetApiTodoInteractor, GetApiTodoInteractor>();
builder.Services.AddScoped<IPostApiTodoInteractor, PostApiTodoInteractor>();
builder.Services.AddScoped<IPutApiTodoIdInteractor, PutApiTodoIdInteractor>();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "example", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "example v1"));
}

app.UseAuthorization();

app.MapControllers();

app.Run();
