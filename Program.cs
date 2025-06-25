using FirebaseAdmin;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using FoodSeekerAPI.Data;
using FoodSeekerAPI.Services;
using Google.Apis.Auth.OAuth2;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
// Turn off Entity Framework Core logging to reduce noise in the logs
// Only log warnings and errors
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
        options.TokenValidationParameters = TokenService.GetDefaultTokenValidationParameters(builder.Configuration)
    );

FirebaseApp.Create(new AppOptions()
{
    // Initialize Firebase Admin SDK with the service account key file
    Credential = GoogleCredential.FromFile("firebase-service-account.json"),
});

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddScoped<PickupIntentService>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<PasswordService>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<NotificationService>();

// Register AppDbContext
builder.Services.AddDbContext<FoodSeekerContext>(options =>
    options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]));

var app = builder.Build();

app.UseAuthentication(); // 🔐 This validates the JWT token from requests
app.UseAuthorization(); // 👮 This enforces [Authorize] attributes

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = ""; // serve swagger UI at root
});

// app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();
app.Run();