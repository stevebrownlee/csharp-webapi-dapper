1. `dotnet new webapi`
1. `code .`
1. Answer yes to popup
1. `dotnet add package Microsoft.Data.Sqlite`
1. `dotnet add package Dapper`
1. `dotnet restore`
1. Create Models directory
1. Define Model for each Table in your ERD
1. Add connection string to `appsettings.json`
    ```json
    "ConnectionStrings": {
        "DefaultConnection": "Data Source=nss.db"
    },
    ```
1. Inject configuration into controller.
    ```cs
    private readonly IConfiguration _config;

    public ValuesController(IConfiguration config)
    {
        _config = config;
    }
    ```
1. Write method to generate a connection
    ```cs
    public IDbConnection Connection
    {
        get
        {
            return new SqliteConnection(_config.GetConnectionString("DefaultConnection"));
        }
    }
    ```
1. Use the lightbulb to include dependencies, or manually include them
    ```cs
    using System.Data;
    using Microsoft.Data.Sqlite;
    using Microsoft.Extensions.Configuration;
    using Dapper;
    ```
