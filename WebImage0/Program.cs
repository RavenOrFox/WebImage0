using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection.PortableExecutable;
using Azure;
using System.Text.Json;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using System;
using Ollama.Core.Models;
using Ollama.Core;

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

app.MapPost("/api/ai", async (HttpContext context) =>
{
    OllamaClient client = new("http://localhost:11434");
    ollamaResponse response = new ollamaResponse();
    ollamaJsonOptions ollama = new ollamaJsonOptions { };
    ollama.model = "deepseek-r1:14b";
    ollama.system = "you have to come up with one joke in the format of top and bottom text in the style of 2010s Russian memes based on the text description of the image. mark the top text as \"**top:**\" and the bottom as \"**bottom:**\". Use one language in your answer.";
    //ollama.system = "ты должен придумать одну шутку в формате верхнего и нижнего текста в стиле российских мемов 2010-х годов на основе текстового описания изображения. помечай верхний текст как \"**top:**\" и нижний как \"**bottom:**\". в своем ответе используй один язык.";
    var prompt = await context.Request.ReadFromJsonAsync<aiPrompt>();
    Console.WriteLine("1 " + prompt.prompt);
    ollama.prompt = prompt.prompt;

    const string url = "http://localhost:11434/api/generate";


    ollama.system = "you have to come up with one joke in the format of top and bottom text in the style of 2010s Russian memes based on the text description of the image. mark the top text as \"**top:**\" and the bottom as \"**bottom:**\". Use one language in your answer. The length of one inscription should not exceed"+prompt.length+"characters.";

    GenerateCompletionResponse response2 = await client.GenerateCompletionAsync(new GenerateCompletionOptions
    {
        Model = ollama.model,
        Prompt = ollama.prompt,
        System = ollama.system

    });

    ollamaResponse resp = new ollamaResponse();
  
    resp.top = response2.Response;
    Console.WriteLine(resp.top);
    var repl = resp.top.IndexOf("</think>") +10;
    Console.WriteLine(repl);
    resp.top = resp.top.Substring(repl);
    Console.WriteLine(resp.top);

    /*
     завтра добавить разделение нижнего и верхнего текста и добавть генерацию описания картинки
     */
    
    await context.Response.WriteAsJsonAsync(resp);
});


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

public class aiPrompt
{
    public string prompt { get; set; }
    public int length { get; set; }
}




public class ollamaJsonOptions
{
    public string model { get; set; }
   // public string stream { get; set; }
    public string prompt { get; set; }
   // public string format { get; set; }

    public string system { get; set; }

}

public class ollamaResponse
{
    public string? top { get; set; }
    public string? bottom { get; set; }
}
