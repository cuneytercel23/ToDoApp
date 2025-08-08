using Dapper;
using Npgsql;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger document
builder.Services.AddOpenApi();

// Connection string'i config'den çek
var cs = builder.Configuration.GetConnectionString("Default")!;

var app = builder.Build();

app.MapOpenApi();

app.MapScalarApiReference(options =>
{
    options
        .WithTitle("Benim API'm")
        .WithTheme(ScalarTheme.Mars);
});

// Basit health check
app.MapGet("/", () => Results.Ok(new { status = "ok" }));

// LISTE
app.MapGet("/todos", async (ILogger<Program> logger) =>
{
    logger.LogInformation("GET /todos isteği geldi");

    await using var conn = new NpgsqlConnection(cs);
    var items = await conn.QueryAsync<Todo>(
        "SELECT id, title, is_done AS IsDone FROM todos ORDER BY id");

    return Results.Ok(items);
});

// TEK KAYIT
app.MapGet("/todos/{id:int}", async (int id) =>
{
    await using var conn = new NpgsqlConnection(cs);
    var item = await conn.QuerySingleOrDefaultAsync<Todo>(
        "SELECT id, title, is_done AS IsDone FROM todos WHERE id = @id", new { id });
    return item is null ? Results.NotFound() : Results.Ok(item);
});

// EKLE
app.MapPost("/todos", async (TodoCreate input) =>
{
    if (string.IsNullOrWhiteSpace(input.Title))
        return Results.BadRequest(new { message = "Title zorunlu." });

    await using var conn = new NpgsqlConnection(cs);
    var id = await conn.ExecuteScalarAsync<int>(
        "INSERT INTO todos (title, is_done) VALUES (@Title, @IsDone) RETURNING id", input);

    var created = new Todo(id, input.Title, input.IsDone);
    return Results.Created($"/todos/{id}", created);
});

// GÜNCELLE (tam nesne)
app.MapPut("/todos/{id:int}", async (int id, TodoUpdate input) =>
{
    await using var conn = new NpgsqlConnection(cs);
    var rows = await conn.ExecuteAsync(
        "UPDATE todos SET title = @Title, is_done = @IsDone WHERE id = @id",
        new { id, input.Title, input.IsDone });

    return rows == 0 ? Results.NotFound() : Results.NoContent();
});

// SİL
app.MapDelete("/todos/{id:int}", async (int id) =>
{
    await using var conn = new NpgsqlConnection(cs);
    var rows = await conn.ExecuteAsync("DELETE FROM todos WHERE id = @id", new { id });
    return rows == 0 ? Results.NotFound() : Results.NoContent();
});

app.Run();

// Model tipleri (record)
public record Todo(int Id, string Title, bool IsDone);
public record TodoCreate(string Title, bool IsDone = false);
public record TodoUpdate(string Title, bool IsDone);