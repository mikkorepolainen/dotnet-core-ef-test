# dotnet core ef test

Testing EF with dotnet core.

- Scaffolding from existing DB
- Separate project for model classes
- Data access layer for encapsulating ORM details
- Migrations
- API project on top of the DAL

## Development

Create an mssql database with table:

```
CREATE TABLE [dbo].[Table](
	[id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Value] [nvarchar](max) NULL
)
```

Copy the `.env.example` file as `.env` in Api project directory, replace connection string with connection string to your db (`.env` file is excluded from version control)

`dotnet run` in the Api project dir

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

TODO

Using a Separate Migrations Project: https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/projects?tabs=dotnet-core-cli

