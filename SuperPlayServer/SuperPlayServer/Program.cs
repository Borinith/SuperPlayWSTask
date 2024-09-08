using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SuperPlayServer.ConnectionManager;
using SuperPlayServer.Data;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("https://localhost:7182/");

builder.Services.AddDbContext<SuperplayContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers().AddControllersAsServices();
builder.Services.AddScoped<IConnection, Connection>();

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

app.UseWebSockets();
app.UseRouting();
app.UseEndpoints(endpoints => { _ = endpoints.MapControllers(); });

await app.RunAsync();