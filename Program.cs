using FirebaseAdmin;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using FoodSeekerAPI.Data;
using FoodSeekerAPI.Services;
using Google.Apis.Auth.OAuth2;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Configure logging
// Reduce noise by showing only warnings and errors from Entity Framework Core
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

// 🔐 Configure JWT authentication using a helper method for token validation parameters
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
        options.TokenValidationParameters = TokenService.GetDefaultTokenValidationParameters(builder.Configuration)
    );

// 🔥 Initialize Firebase Admin SDK using service account credentials
FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.FromFile("firebase-service-account.json"),
});

// 📦 Register services with the dependency injection container
builder.Services.AddControllers();                         // For API controllers
builder.Services.AddEndpointsApiExplorer();                // For Swagger endpoint discovery
builder.Services.AddSwaggerGen();                          // For Swagger UI
builder.Services.AddAuthorization();                       // For role/policy-based authorization

// 🔄 Application services
builder.Services.AddScoped<PickupIntentService>();         // Scoped: new instance per request
builder.Services.AddSingleton<TokenService>();             // Singleton: one instance for the app lifetime
builder.Services.AddSingleton<PasswordService>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<NotificationService>();

// 🗄️ Configure EF Core with SQL Server using the connection string from configuration
builder.Services.AddDbContext<FoodSeekerContext>(options =>
    options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]));

// 🚀 Build the app
var app = builder.Build();

// 🔐 Add authentication and authorization middleware
app.UseAuthentication(); // Validates JWTs on incoming requests
app.UseAuthorization();  // Enforces authorization attributes on endpoints

// 🛠️ Enable Swagger only in development for API testing and docs
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔀 Map controller routes to endpoints
app.MapControllers();

// 🏁 Run the application
app.Run();
