using Microsoft.EntityFrameworkCore;
using SchoolApi.Data;
using SchoolApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration
    .GetConnectionString("SchoolDb")
    ?? throw new InvalidOperationException(
        "Missing ConnectionStrings:SchoolDb");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)));
builder.Services.AddScoped<IProgrammeService, ProgrammeService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IStudentService, StudentService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();