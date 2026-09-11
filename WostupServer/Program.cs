using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using WostupServer;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/users/{id}", async (int id) =>
{
    await using var connection = new NpgsqlConnection(
        builder.Configuration.GetConnectionString("Database")
    );

    var user = await connection.QuerySingleOrDefaultAsync<User>(
        "SELECT id, name, number FROM users WHERE id = @Id",
        new { Id = id }
    );

    return user is null
        ? Results.NotFound()
        : Results.Ok(user);
});

app.MapPost("/users", async (CreateUserRequest request) =>
{
    await using var connection = new NpgsqlConnection(
        builder.Configuration.GetConnectionString("Database")
    );

    const string query = "INSERT INTO users (name, number)" +
        " VALUES (@Name, @Number) RETURNING id, name, number";

    var newUser = await connection.QuerySingleAsync<User>(
        query,
        new { Name = request.Name, Number = request.Number}
    );

    return Results.Created($"/users/{newUser.Id}", newUser);
});

app.MapDelete("/users/{id}", async (int id) => 
{
    await using var connection = new NpgsqlConnection(
        builder.Configuration.GetConnectionString("Database")
    );

    int affectedRows = await connection.ExecuteAsync(
        "DELETE FROM users WHERE id == @Id;",
        new { Id = id }
    );

    return affectedRows == 0 ? Results.NotFound() : Results.NoContent();
});

app.MapPost("/messages", async (CreateMessageRequest request) =>
{
    await using var connection = new NpgsqlConnection(
        builder.Configuration.GetConnectionString("Database")
    );

    int affectedRows = await connection.ExecuteAsync(
        "INSERT INTO messages (recipient_number, sender_number, content, sent_date_time)" +
        "VALUES(@RecipientNumber, @SenderNumber, @Content, @SentDateTime)",
        new 
        {
            RecipientNumber = request.RecipientNumber,
            SenderNumber = request.SenderNumber,
            Content = request.Content,
            SentDateTime = request.SentDateTime
        }
    );

    return affectedRows == 0 ? Results.NotFound() : Results.Created();
});

app.MapGet("/messages/{number}", async (string number) => 
{
    await using var connection = new NpgsqlConnection(
        builder.Configuration.GetConnectionString("Database")
    );

    const string query =
        "DELETE FROM messages WHERE recipient_number = @Number " +
        "RETURNING " +
        "id, " +
        "recipient_number AS RecipientNumber, " +
        "sender_number AS SenderNumber, " +
        "content, " +
        "sent_date_time AS SentDateTime";

    var messages = await connection.QueryAsync<Message>(
        query,
        new { Number = number }
    );

    return !messages.Any() ? Results.NotFound() : Results.Ok(messages);
});

app.MapPost("/files", async ([FromForm] CreateFileRequest request, 
    IWebHostEnvironment environment) => 
{
    if (request.File.Length == 0) 
    {
        return Results.BadRequest();
    }

    var uploadsDirectory = Path.Combine(environment.ContentRootPath, "uploads");

    Directory.CreateDirectory(uploadsDirectory);

    var fileName = Path.GetRandomFileName();

    var filePath = Path.Combine
    (
        uploadsDirectory,
        $"{request.UserName}--{request.UserNumber}",
        fileName
    );

    await using var stream = File.Create(filePath);
    await request.File.CopyToAsync(stream);

    return Results.Created($"/files/{fileName}", new
    {
        fileName,
        originalFileName = request.File.FileName,
        size = request.File.Length,
        contentType = request.File.ContentType
    });
}).DisableAntiforgery();

app.Run();