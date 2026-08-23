using InteractiveGameApi.InteractiveGame.API.Hubs;
using InteractiveGameApi.InteractiveGame.API.Services;
using InteractiveGameApi.InteractiveGame.BLL;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// 1) Controllers + Swagger/OpenAPI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "InteractiveGameApi", Version = "v1" });
});

// 2) In-memory PotentiometerService singleton
builder.Services.AddSingleton<PotentiometerService>();

// 3) SignalR (for real-time pushes)
builder.Services.AddSignalR();

// 4) Arduino service (real or fake) as singleton
builder.Services.AddSingleton<InteractiveGameService>(sp =>
{
    Console.WriteLine("🔧 Program.cs: Creating InteractiveGameService…");
    var potService = sp.GetRequiredService<PotentiometerService>();
    var hubContext = sp.GetRequiredService<IHubContext<GameHub>>();
    return new InteractiveGameService(potService, hubContext);
});

// 5) (Optional) CORS for a separate web UI
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("WebAppPolicy", policy =>
    {
        policy
          .WithOrigins("http://localhost:5173")  // your front-end origin
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials();
    });
});

var app = builder.Build();

// 6) Enable Swagger UI at /swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "InteractiveGameApi v1");
    // c.RoutePrefix = ""; // Uncomment to serve Swagger at the app root
});

// 7) Static files, CORS, routing, HTTPS, auth
app.UseStaticFiles();
app.UseCors("WebAppPolicy");
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();

// 8) Map controllers and SignalR hub
app.MapControllers();
app.MapHub<GameHub>("/gameHub");

// 9) Force the service constructor to run now (opens COM / starts fake loop)
app.Services.GetRequiredService<InteractiveGameService>();

app.Run();
