using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>();

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

public static class ProductRepository
{
  public static List<Product> Products { get; set; } = new List<Product>();

  public static void Init(IConfiguration configuration)
  {
    var products = configuration.GetSection("Products").Get<List<Product>>();
    Products = products;
  }
  public static void Add(Product product)
  {
    Products.Add(product);
  }

  public static Product GetBy(string code) => Products.FirstOrDefault(p => p.Code == code);

  public static void Remove(Product product)
  {
    Products.Remove(product);
  }
}

public class Product
{
  public string Code { get; set; }
  public string Name { get; set; }
}

public class ApplicationDbContext : DbContext
{
  public DbSet<Product> Products { get; set; }

  protected override void OnConfiguring(DbContextOptionsBuilder options)
    => options.UseSqlServer(@"Server=localhost\SQLEXPRESS;Database=Products;Trusted_Connection=True;");
}