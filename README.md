# dotnet core ef test

Testing EF with dotnet core.

- Scaffolding from existing DB
- Separate project for model classes
- Data access layer for encapsulating ORM details
- Migrations
- API project on top of the DAL

## Running

Copy the `.env.example` file as `.env` in Api project directory, replace connection string with connection string to your db (`.env` file is excluded from version control). Do the same for Data project (this is for ef tools, not runtime).

Execute `dotnet ef database update`. This creates the database artifacts by running all migrations against the DB.

Execute `dotnet run` in the Api project dir.

## Model Updates

Copy the `.env.example` file as `.env` in Data project directory, replace connection string with connection string to the db to be updated (`.env` file is excluded from version control). This env file is used for tooling only, not when running apps based on the Data project.

Update the Model and the DbContext, then run the following commands in the Data project directory:

`dotnet ef migrations add <Name for changeset>` creates the migration  
`dotnet ef database update` updates the target database

### Running migrations in a CD pipeline

https://clearmeasure.com/run-ef-core-migrations-in-azure-devops/

Apparently the only feasible way is to export migration scripts in the build pipeline and run them in the release pipeline.

Run `dotnet ef migrations script --idempotent --output \path\to\staging\migrations.sql` in the build pipeline to create the script file.

Connect to the correct database and run the migrations script in the release pipeline.

NOTE downgrading is not supported this way if need to go back to previous version.

The migrations file looks like this:

```
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20200315221601_InitialCreate')
BEGIN
    CREATE TABLE [Table] (
        [id] int NOT NULL IDENTITY,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_Table] PRIMARY KEY ([id])
    );
END;

GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20200315221601_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20200315221601_InitialCreate', N'3.1.2');
END;

GO
```

### Re-scaffolding

`dotnet ef dbcontext scaffold "Data Source=localhost;Initial Catalog=ef-api;User=ef-test;Password=xxx" Microsoft.EntityFrameworkCore.SqlServer -o ../Model --context-dir . --force`

Note that with the `--force` flag the existing db context will be overwritten, inlcuding the changes in the `OnConfiguring` method (see notes on migrations below). Keep a copy of the original context class and copy the contents of the `OnConfiguring` method over after scaffolding.

TODO How does this affect migrations, is it possible to just re-scaffold and then create a new migration on top?

## Notes

EF support is installed into the project where DbContext lives, in this case the Data project.

`dotnet add package Microsoft.EntityFrameworkCore.SqlServer`

EF tooling is installed in the project where EF commands are run, in this case also the Data project.
Here the tooling is installed locally instead of global installation.

`dotnet add package Microsoft.EntityFrameworkCore.Design`  
`dotnet new tool-manifest` --> creates `./config/dotnet-tools.json`  
`dotnet tool install dotnet-ef`  
`dotnet restore`

In addition, because the Data project is a netstandard project, there must be a "startup project" with .NETCore build target, otherwise there will be an error message:

> Startup project 'data.csproj' targets framework '.NETStandard'. There is no runtime associated with this framework, and projects targeting it cannot be executed directly. To use the Entity Framework Core .NET Command-line Tools with this project, add an executable project targeting .NET Core or .NET Framework that references this project, and set it as the startup project using --startup-project; or, update this project to cross-target .NET Core or .NET Framework. For more information on using the EF Core Tools with .NET Standard projects, see https://go.microsoft.com/fwlink/?linkid=2034781

In this case the workaround is to cross target netcore in addition to netstandard: add netcoreapp into TargetFramework with semicolon, change element to TargetFrameworks. Note that netcoreapp must be the first target in the list.

`<TargetFrameworks>netcoreapp3.1;netstandard2.1</TargetFrameworks>`

The other option is to include `--startup-project <path-to-core-app>` in each `dotnet ef` command.

https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet  
https://github.com/dotnet/sdk/issues/9910

### Scaffolding

To have the model classes generated in a separate project, create the project first (here Model) and refer to it with the `-o` argument. Note that now the `--context-dir` argument must be given also, otherwise the dbcontext is written into the Model project along with the entities.

`dotnet ef dbcontext scaffold "Data Source=localhost;Initial Catalog=ef-api;User=ef-test;Password=xxx" Microsoft.EntityFrameworkCore.SqlServer -o ../Model --context-dir .`

https://docs.microsoft.com/en-us/ef/core/managing-schemas/scaffolding

### Migrations

https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli  
https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet#dotnet-ef-migrations-script

In order to run ef cli tools against the database, the DB context must also have the DB provider configured with valid connection strings and all.
To achieve this, the connection string is read from the environment variables in the `OnConfiguring` method on the context class.
Note that this is a manual change to generated source code which will be overwritten when re-scaffolding!

```
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
	if (!optionsBuilder.IsConfigured)
	{
		DotEnv.Config();
		optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("EF_TEST_CONNSTR"));
	}
}
```

`dotnet ef migrations add InitialCreate` --> creates initial migration and adds table `__EFMigrationsHistory`

However, the `__EFMigrationsHistory` is not populated with the details of this migration, which causes a problem when running the update script `dotnet ef database update`. The command fails because the db objects already exists and there is no migration history. To fix this, run `dotnet ef migrations script 0 InitialCreate` and look for output that looks like:

```
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20200315221601_InitialCreate', N'3.1.2');
```

and run it against the database. After this, `dotnet ef database update` does not fail any more.

TODO Using a Separate Migrations Project: https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/projects?tabs=dotnet-core-cli

TODO automatic migration https://blog.rsuter.com/automatically-migrate-your-entity-framework-core-managed-database-on-asp-net-core-application-start/

