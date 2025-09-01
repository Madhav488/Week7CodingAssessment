using System.Collections.Concurrent;
using StudentWebApi.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Thread-safe in-memory store
var store = new ConcurrentDictionary<int, Student>();

app.MapGet("/", () => "StudentWebApi (in-memory) running");

// Read all students
app.MapGet("/students", () =>
    Results.Ok(store.Values));

// Read one student by Rn
app.MapGet("/students/{rn:int}", (int rn) =>
    store.TryGetValue(rn, out var s) ? Results.Ok(s) : Results.NotFound());

// Create a new student
app.MapPost("/students", (Student s) =>
{
    if (!store.TryAdd(s.Rn, s))
        return Results.Conflict($"Student with Rn {s.Rn} already exists.");
    return Results.Created($"/students/{s.Rn}", s);
});

// Update an existing student
app.MapPut("/students/{rn:int}", (int rn, Student s) =>
{
    if (rn != s.Rn) return Results.BadRequest("Route Rn and body Rn must match.");
    if (!store.ContainsKey(rn)) return Results.NotFound();
    store[rn] = s;
    return Results.Ok(s);
});

// Delete a student
app.MapDelete("/students/{rn:int}", (int rn) =>
    store.TryRemove(rn, out _) ? Results.NoContent() : Results.NotFound());

app.Run();
