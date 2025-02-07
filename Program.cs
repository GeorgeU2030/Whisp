using Whisp.Hub;
using dotenv.net;

var builder = WebApplication.CreateBuilder(args);

DotEnv.Load();

var url = Environment.GetEnvironmentVariable("FRONTEND_URL");
var frontendUrl = url ?? "http://localhost:4200";

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

builder.Services.AddSingleton<IDictionary<string, UserGroupConnection>>(opt => new Dictionary<string, UserGroupConnection>());

builder.Services.AddCors(options =>
{
    // Configure CORS policies
    options.AddDefaultPolicy(builder =>
    {
        // Set the allowed origins, headers, methods, and credentials
        builder.WithOrigins(frontendUrl)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors();
// Message for the root path
app.MapGet("/", () => "Whisp backend running...!");

app.MapHub<ChatHub>("/chat");

app.Run();


