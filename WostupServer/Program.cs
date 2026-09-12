using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Text.Json;
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

app.MapGet("/teinforeq", async (IWebHostEnvironment environment) => 
{
    try
    {
        string te = "TEconfig.json";
        string tePath = Path.Combine(environment.ContentRootPath, te);

        if (!File.Exists(tePath))
        {
            using var s = File.Create(tePath);
            using var writer = new StreamWriter(s);

            await writer.WriteAsync(TEConfig.GetDefaultConfig());
        }

        using var stream = File.OpenRead(tePath);
        var config = await JsonSerializer.DeserializeAsync<TEConfig>(stream);

        return Results.Ok(config);
    }
    catch 
    {
        return Results.InternalServerError();
    }
});

app.MapPost("/files", async ([FromForm] CreateFilesRequest request, 
    IWebHostEnvironment environment) => 
{
    if (request.Files.Count == 0) 
    {
        return Results.BadRequest();
    }

    var uploadsDirectory = Path.Combine(environment.ContentRootPath, "uploads");

    var userDirectory = 
        Path.Combine(uploadsDirectory, $"{request.UserName}--{request.UserNumber}");

    var destinationDirectory = Path.Combine(userDirectory, request.ServerDestinationPath);

    var dir = Directory.CreateDirectory(destinationDirectory);
    foreach (var file in dir.EnumerateFiles()) 
    {
        file.Delete();
    }

    foreach (var file in request.Files) 
    {
        var fileName = Path.GetRandomFileName();

        var filePath = Path.Combine
        (
            destinationDirectory,
            fileName
        );

        await using var stream = File.Create(filePath);
        await file.CopyToAsync(stream);
    }

    return Results.Created($"/files/{destinationDirectory}", new {});

}).DisableAntiforgery();

app.Run();