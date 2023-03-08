using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSqlServer<ApplicationDbContext>(builder.Configuration["Database:SqlServer"]);

var app = builder.Build();
var configuration = app.Configuration;
ProductRepository.Init(configuration);

app.MapPost("/product", (ProductRequest productRequest, ApplicationDbContext context) =>
{
  var category = context.Category.Where(c => c.Id == productRequest.CategoryId).First();
  var product = new Product
  {
    Code = productRequest.Code,
    Name = productRequest.Name,
    Description = productRequest.Description,
    Category = category
  };

  context.Products.Add(product);
  context.SaveChanges();

  return Results.Created($"/products/{product.Id}", product.Id);
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
