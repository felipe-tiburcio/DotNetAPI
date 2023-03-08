using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSqlServer<ApplicationDbContext>(builder.Configuration["Database:SqlServer"]);

var app = builder.Build();
var configuration = app.Configuration;
ProductRepository.Init(configuration);

app.MapPost("/product", (Product product) =>
{
  ProductRepository.Add(product);
  return Results.Created($"/products/{product.Code}", product.Code);
});

app.MapGet("/product/{code}", ([FromRoute] string code) =>
{
  var product = ProductRepository.GetBy(code);

  return product != null ? Results.Ok(product) : Results.NotFound();
});

app.MapPut("/product", (Product product) =>
{
  var selection = ProductRepository.GetBy(product.Code);
  selection.Name = product.Name;
  return Results.Ok();
});

app.MapDelete("/product/{code}", ([FromRoute] string code) =>
{
  var selection = ProductRepository.GetBy(code);

  ProductRepository.Remove(selection);
  return Results.Ok();
});

app.MapGet("/configuration/database", (IConfiguration configuration) =>
{
  return Results.Ok(configuration["Database:Connection"]);
});

app.Run();
