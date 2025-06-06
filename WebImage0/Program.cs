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
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5005, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});
builder.WebHost.UseKestrel();

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

app.MapRazorPages();

app.MapGet("/api/memes", async (ApplicationContext db) => await db.Memes.ToListAsync()); //отправка всех мемов на главную страницу

app.MapGet("/api/meme/{id:int}", async(int id, ApplicationContext db, HttpContext context) => // отправка 1 мема по запросу
{
    Meme? meme = await db.Memes.FirstOrDefaultAsync(u => u.Id == id);

    if (meme == null) return Results.NotFound(new { message = "мем не найден" });

    return Results.Json(meme);

});

app.MapPost("/api/ai", async (HttpContext context) =>   // обработка запроса на генерацию мема неросетью
{
    OllamaClient client = new("http://localhost:11434"); // настройка запроса gemma и deepseek
    ollamaJsonOptions ollama = new ollamaJsonOptions { };
    GemmaJsonOptions gemma = new GemmaJsonOptions { };
    ollama.model = "deepseek-r1:14b";
    gemma.model = "gemma3:12b-it-qat";
    ollama.system = "you must compose a joke based on the image description in the style of \"top and bottom text\". respond as json with the parameters top for the upper text and bottom for the lower. jokes can be absurd, ironic and offensive. the respond must be translated into Russian";
    gemma.system = "ты должна описать изображение. обязательно выпиши все надписи с картинки.";
    var prompt = await context.Request.ReadFromJsonAsync<aiPrompt>();
    if (prompt.prompt == "")
    {
        prompt.prompt = "просто опиши картинку с использованием системного промпта";
    }
    else 
    {
        prompt.prompt = "контекст к изображению: " + prompt.prompt;
    }
    gemma.prompt = prompt.prompt;

    var repl = prompt.imgBase64.IndexOf("base64") +7; // обработка изображения в формате base64 (удаление заголовка)
    prompt.imgBase64 = prompt.imgBase64.Substring(repl);

    gemma.Image[0] = prompt.imgBase64;
    Console.WriteLine(prompt.imgBase64.Length);


    GenerateCompletionResponse response = await client.GenerateCompletionAsync(new GenerateCompletionOptions // генерация описания изображения
    {
        Model = gemma.model,
        System = gemma.system,
        Images = gemma.Image,
        Prompt = gemma.prompt
        
    });

    Console.WriteLine(response.Response);   // настройка промпта deepseek
    ollama.prompt = response.Response;
    ollama.format = "json";
    Console.WriteLine(ollama.format);
    GenerateCompletionResponse response2 = await client.GenerateCompletionAsync(new GenerateCompletionOptions
    {
        Model = ollama.model,
        Prompt = ollama.prompt,
        System = ollama.system,
        Format = ollama.format

    });

    ollamaResponse resp = new ollamaResponse(); // обработка ответа deepseek
    var text = response2.Response;

    return response2.Response;
});


app.Run();

public class ApplicationContext : DbContext     // описание класса базы данных
{
    public DbSet<Meme> Memes { get; set; } = null!; // класс данных
    public ApplicationContext(DbContextOptions<ApplicationContext> options) // настройка интерфейса
        : base(options){}

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) //  настройка подключения
    {
        optionsBuilder.UseSqlServer(@"Server=localhost;Database=WebImage0;Trusted_Connection=True;encrypt=false");
    }
}

public class Meme // описание класса мемов
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public string ImgUrl { get; set; } = "";

    public string tag1 { get; set; } = "";

    public string tag2 { get; set; } = "";

}

public class aiPrompt // описание класса получаемых данных от клиента для нейросети
{
    public string prompt { get; set; }
    public int length { get; set; }

    public string imgBase64 { get; set; }
}



public class ollamaJsonOptions // описание класса параметров deepseek
{
    public string model { get; set; }
    public string prompt { get; set; }

    public string system { get; set; }

    public string format { get; set; }

}

public class GemmaJsonOptions // описание класса параметров gemma
{
    public string model { get; set; }
    public string[] Image { get; set; } = {"image"};

    public string system { get; set; }

    public string prompt { get; set; }

}

public class ollamaResponse // описание класса ответа сервера
{
    public string? top { get; set; }
    public string? bottom { get; set; }
}

