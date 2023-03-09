using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

  if (productRequest.Tags != null)
  {
    product.Tags = new List<Tag>();

    foreach (var item in productRequest.Tags)
    {
      product.Tags.Add(new Tag { Name = item });
    }
  }

  context.Products.Add(product);
  context.SaveChanges();

  return Results.Created($"/products/{product.Id}", product.Id);
});

app.MapGet("/product/{id}", ([FromRoute] int id, ApplicationDbContext context) =>
{
  var product = context.Products
    .Include(p => p.Category)
    .Include(p => p.Tags)
    .Where(p => p.Id == id).First();

  return product != null ? Results.Ok(product) : Results.NotFound();
});

app.MapPut("/product/{id}", ([FromRoute] int id, ProductRequest productRequest, ApplicationDbContext context) =>
{
  var selection = context.Products
    .Include(p => p.Tags)
    .Where(p => p.Id == id).First();

  selection.Code = productRequest.Code;
  selection.Name = productRequest.Name;
  selection.Description = productRequest.Description;

  var category = context.Category.Where(c => c.Id == productRequest.CategoryId).First();
  selection.Category = category;
  selection.Tags = new List<Tag>();

  if (productRequest.Tags != null)
  {
    selection.Tags = new List<Tag>();

    foreach (var item in productRequest.Tags)
      selection.Tags.Add(new Tag { Name = item });
  }

  context.SaveChanges();
  return Results.Ok();
});

app.MapDelete("/product/{id}", ([FromRoute] int id, ApplicationDbContext context) =>
{
  var product = context.Products
    .Where(p => p.Id == id).First();

  context.Products.Remove(product);

  context.SaveChanges();
  return Results.Ok();
});

app.MapGet("/configuration/database", (IConfiguration configuration) =>
{
  return Results.Ok(configuration["Database:Connection"]);
});

app.Run();
