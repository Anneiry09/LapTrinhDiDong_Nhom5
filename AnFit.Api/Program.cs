using AnFit.Api.Data;
using AnFit.Api.Extensions;
using AnFit.Api.Integrations.Garmin;
using AnFit.Api.Middlewares;
using AnFit.Api.Repositories.Implementations;
using AnFit.Api.Repositories.Interfaces;

using AnFit.Api.Services.Implementations.Core;
using AnFit.Api.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.

builder.Services.AddControllers();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AnFitPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:5173" })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithAuth();
// Đăng ký Generic Repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Đăng ký Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<DataSeeder>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddScoped<IAuthService, AuthService>();

// ...
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<JwtTokenHelper>();
builder.Services.AddJwtAuthentication(builder.Configuration);
// ...

builder.Services.AddScoped<IMedicalConditionService, MedicalConditionService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserBodyLogService, UserBodyLogService>();
builder.Services.AddScoped<IDailyHealthLogService, DailyHealthLogService>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IFoodItemService, FoodItemService>();
builder.Services.AddScoped<IMealPlanService, MealPlanService>();
builder.Services.AddScoped<IWorkoutPlanService, WorkoutPlanService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Email & Cache cho OTP
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.Configure<GarminSettings>(builder.Configuration.GetSection("Garmin"));
// Dùng GarminBridgeClient (gọi Python microservice) thay vì GarminClient (gọi API chính thức)
// Khi có Garmin Developer API key, đổi GarminBridgeClient thành GarminClient là xong
builder.Services.AddHttpClient<IGarminClient, GarminBridgeClient>();
builder.Services.AddScoped<IGarminSyncService, GarminSyncService>();

// AI Microservice (Python FastAPI) - Gọi HTTP sang Python để lấy dự đoán AI
builder.Services.AddHttpClient<IAiService, AiService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AnFitPolicy");

// QUAN TRỌNG: 2 dòng này phải nằm TRƯỚC app.MapControllers();
app.UseAuthentication(); // Báo bảo vệ kiểm tra vé (Token)
app.UseAuthorization();  // Báo bảo vệ kiểm tra quyền (Role)

app.MapControllers();

// Gọi DataSeeder
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var seeder = services.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Đã xảy ra lỗi trong quá trình seed dữ liệu.");
    }
}

app.Run();

