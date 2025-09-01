
using Microsoft.EntityFrameworkCore;
using StudentApi.Data;
using StudentApi.Models;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);


builder.Configuration.AddEnvironmentVariables();


var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
              ?? builder.Configuration["ConnectionStrings:DefaultConnection"]
              ?? "Server=localhost,1433;Database=StudentDb;User Id=sa;Password=P@ssw0rd12345;TrustServerCertificate=True";

builder.Services.AddDbContext<StudentDbContext>(options =>
    options.UseSqlServer(connStr));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StudentDbContext>();


    var maxRetries = 20;
    var delay = TimeSpan.FromSeconds(3);
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            Console.WriteLine($"Attempting database migrate (try {attempt}/{maxRetries})...");
            db.Database.Migrate();
            Console.WriteLine("Database migrate applied.");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Migration attempt {attempt} failed: {ex.Message}");
            if (attempt == maxRetries) throw;
            await Task.Delay(delay);
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "StudentApi up");


app.MapGet("/students", async (StudentDbContext db) =>
    Results.Ok(await db.Students.AsNoTracking().ToListAsync()));

app.MapGet("/students/{rn:int}", async (int rn, StudentDbContext db) =>
{
    var s = await db.Students.FindAsync(rn);
    return s is null ? Results.NotFound() : Results.Ok(s);
});

app.MapPost("/students", async (Student s, StudentDbContext db) =>
{
    var exists = await db.Students.AnyAsync(x => x.Rn == s.Rn);
    if (exists) return Results.Conflict($"Student with Rn {s.Rn} exists.");
    db.Students.Add(s);
    await db.SaveChangesAsync();
    return Results.Created($"/students/{s.Rn}", s);
});

app.MapPut("/students/{rn:int}", async (int rn, Student s, StudentDbContext db) =>
{
    if (rn != s.Rn) return Results.BadRequest("Rn in route and body must match.");
    var exists = await db.Students.AnyAsync(x => x.Rn == rn);
    if (!exists) return Results.NotFound();
    db.Students.Update(s);
    await db.SaveChangesAsync();
    return Results.Ok(s);
});

app.MapDelete("/students/{rn:int}", async (int rn, StudentDbContext db) =>
{
    var s = await db.Students.FindAsync(rn);
    if (s is null) return Results.NotFound();
    db.Remove(s);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
