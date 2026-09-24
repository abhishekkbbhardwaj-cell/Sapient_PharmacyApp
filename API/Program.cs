using PharmacyApp.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(); 
builder.Services.AddEndpointsApiExplorer();

//register services
builder.Services.AddScoped<IMedicineRepository, MedicineRepository>();

var app = builder.Build();
app.UseCors("AllowAngularApp");

app.UseExceptionHandler("/error");
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
