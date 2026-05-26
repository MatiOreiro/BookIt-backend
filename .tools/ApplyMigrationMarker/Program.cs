using Npgsql;

var connString = Environment.GetEnvironmentVariable("MIGRATION_CONN");
if (string.IsNullOrWhiteSpace(connString))
{
    Console.Error.WriteLine("MIGRATION_CONN environment variable is required.");
    Environment.Exit(1);
}

var migrations = new[]
{
    ("20260521063557_InitialCreate", "8.0.0")
};

var createTableSql = "CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\" (\"MigrationId\" varchar(150) NOT NULL PRIMARY KEY, \"ProductVersion\" varchar(32) NOT NULL);";

try
{
    var action = Environment.GetEnvironmentVariable("MIGRATION_ACTION") ?? string.Empty;
    if (action.Equals("remove-initial", StringComparison.OrdinalIgnoreCase))
    {
        await using var connRem = new NpgsqlConnection(connString);
        await connRem.OpenAsync();
        await using var delCmd = connRem.CreateCommand();
        delCmd.CommandText = "DELETE FROM \"__EFMigrationsHistory\" WHERE \"MigrationId\" = '20260521063557_InitialCreate';";
        var rows = await delCmd.ExecuteNonQueryAsync();
        Console.WriteLine($"Removed {rows} rows for InitialCreate from __EFMigrationsHistory.");
        Environment.Exit(0);
    }
    if (action.Equals("list-tables", StringComparison.OrdinalIgnoreCase))
    {
        await using var connList = new NpgsqlConnection(connString);
        await connList.OpenAsync();
        await using var cmdList = connList.CreateCommand();
        cmdList.CommandText = "SELECT tablename FROM pg_catalog.pg_tables WHERE schemaname='public' ORDER BY tablename;";
        await using var reader = await cmdList.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            Console.WriteLine(reader.GetString(0));
        }
        Environment.Exit(0);
    }
    var builder = new NpgsqlConnectionStringBuilder(connString);
    var targetDatabase = builder.Database;

    try
    {
        await using var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = createTableSql;
            await cmd.ExecuteNonQueryAsync();
        }

        foreach (var tuple in migrations)
        {
            var id = tuple.Item1;
            var version = tuple.Item2;
            await using var ins = conn.CreateCommand();
            ins.CommandText = $"INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('{id}', '{version}') ON CONFLICT (\"MigrationId\") DO NOTHING;";
            await ins.ExecuteNonQueryAsync();
            Console.WriteLine($"Ensured migration '{id}' is recorded.");
        }

        Console.WriteLine("Done.");
        Environment.Exit(0);
    }
    catch (PostgresException pex) when (pex.SqlState == "3D000")
    {
        // Database does not exist - create it by connecting to the default 'postgres' database
        Console.WriteLine($"Database '{targetDatabase}' does not exist, attempting to create it...");
        var adminBuilder = new NpgsqlConnectionStringBuilder(connString) { Database = "postgres" };
        await using var adminConn = new NpgsqlConnection(adminBuilder.ConnectionString);
        await adminConn.OpenAsync();
        await using var createCmd = adminConn.CreateCommand();
        createCmd.CommandText = $"SELECT 1 FROM pg_database WHERE datname = '{targetDatabase}';";
        var exists = (await createCmd.ExecuteScalarAsync()) is not null;
        if (!exists)
        {
            createCmd.CommandText = $"CREATE DATABASE \"{targetDatabase}\";";
            await createCmd.ExecuteNonQueryAsync();
            Console.WriteLine($"Database '{targetDatabase}' created.");
        }
        else
        {
            Console.WriteLine($"Database '{targetDatabase}' already exists (race). Proceeding.");
        }

        // Re-run the tool against the newly created database
        await using var conn2 = new NpgsqlConnection(connString);
        await conn2.OpenAsync();
        await using (var cmd2 = conn2.CreateCommand())
        {
            cmd2.CommandText = createTableSql;
            await cmd2.ExecuteNonQueryAsync();
        }

        foreach (var tuple in migrations)
        {
            var id = tuple.Item1;
            var version = tuple.Item2;
            await using var ins2 = conn2.CreateCommand();
            ins2.CommandText = $"INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('{id}', '{version}') ON CONFLICT (\"MigrationId\") DO NOTHING;";
            await ins2.ExecuteNonQueryAsync();
            Console.WriteLine($"Ensured migration '{id}' is recorded.");
        }

        Console.WriteLine("Done.");
        Environment.Exit(0);
    }

    
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.ToString());
    Environment.Exit(2);
}