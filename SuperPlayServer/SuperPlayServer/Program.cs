using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("https://localhost:7182/");
var app = builder.Build();

app.MapGet("/", () => "Hello World!\n" + $"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}");

await app.RunAsync();