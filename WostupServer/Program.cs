using Dapper;
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

    var affectedRows = await connection.ExecuteAsync(
        "DELETE FROM users WHERE id == @Id;",
        new { Id = id }
    );

    return affectedRows == 0 ? Results.NotFound() : Results.NoContent();
});

app.MapPost("/messages", ())

app.Run();