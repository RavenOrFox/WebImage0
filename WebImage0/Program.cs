using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System.Windows;


var builder = WebApplication.CreateBuilder(args);

string connection = "Server=localhost;Database=WebImage0;Trusted_Connection=True;encrypt=false";
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();


//app.MapDefaultControllerRoute();


app.MapRazorPages();



app.MapGet("/api/memes", async (ApplicationContext db) => await db.Memes.ToListAsync());

app.MapGet("/api/meme/{id:int}", async(int id, ApplicationContext db, HttpContext context) =>
{
    Meme? meme = await db.Memes.FirstOrDefaultAsync(u => u.Id == id);

    if (meme == null) return Results.NotFound(new { message = "Пользователь не найден" });

    return Results.Json(meme);
});

/*app.MapPost("/api/create/", async (HttpContext context, ApplicationContext db) =>
{
    var form =  context.Request.Form;

    if (form["id"] != 0)
    {
        //сохранить картинку в дб
    }

    var file = form.Files[0];

});*/

app.Run();

public class ApplicationContext : DbContext
{
    public DbSet<Meme> Memes { get; set; } = null!;
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options){}

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=localhost;Database=WebImage0;Trusted_Connection=True;encrypt=false");
    }
}

public class Meme
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public string ImgUrl { get; set; } = "";

    public string tag1 { get; set; } = "";

    public string tag2 { get; set; } = "";

}

