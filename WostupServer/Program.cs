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
            await using var s = File.Create(tePath);
            await using var writer = new StreamWriter(s);

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
        return Results.BadRequest("No files submitted.");
    }

    if (request.Files.Count != request.ServerDestinationPaths.Count) 
    {
        return Results.BadRequest("The number of files and server dest paths must be the same" +
            $", in this case they were not: files count={request.Files.Count} server dest " +
            $"paths count={request.ServerDestinationPaths.Count}.");
    }

    try
    {
        var uploadsDirectory = Path.Combine(environment.ContentRootPath, "uploads");

        var userDirectory =
            Path.Combine(uploadsDirectory, $"{request.UserName}--{request.UserNumber}");

        var alreadyTouchedDirs = new HashSet<string>();

        for (int i = 0; i < request.Files.Count; i++)
        {
            var destinationDirectory = Path.Combine
            (
                userDirectory,
                request.ServerDestinationPaths[i]
            );

            var dir = Directory.CreateDirectory(destinationDirectory);

            if (!alreadyTouchedDirs.Contains(dir.FullName)) 
            {
                foreach (var file in dir.EnumerateFiles())
                {
                    file.Delete();
                }
            }

            alreadyTouchedDirs.Add(dir.FullName);

            var fileName = Path.GetRandomFileName();

            var filePath = Path.Combine
            (
                destinationDirectory,
                fileName
            );

            await using var stream = File.Create(filePath);
            await request.Files[i].CopyToAsync(stream);
        }

        string lastUpdateFilePath = Path.Combine(userDirectory, "lastUpdate.log");

        await using var luStream = File.Create(lastUpdateFilePath);
        await using var luWriter = new StreamWriter(luStream);

        await luWriter.WriteAsync($"The latest update received from this client was" +
            $" on the (dd/MM/yyyy hh:mm:ss)(UTC+2):" +
            $" {DateTime.UtcNow.AddHours(2).ToString("dd/MM/yyyy hh:mm:ss")}");

        return Results.Created($"/files/{userDirectory}", new { });
    }
    catch 
    {
        return Results.InternalServerError("Uknown server error.");
    }

}).DisableAntiforgery();

app.Run();