# Product API

ASP.NET Core Web API (.NET 8) for managing products, backed by SQL Server via Entity Framework Core.

## AI Tools Used

- **GitHub Copilot** — inline code completion for controllers, model classes, and EF Core boilerplate.
- **ChatGPT (GPT-4)** — architecture decisions, debugging EF Core migration errors.
- **Claude** — code review, refactoring suggestions, and generating this README.

## Example Prompts

A few prompts that helped move the project forward:

- "Generate an ASP.NET Core controller for CRUD operations on a Product entity with EntityFrameworkCore."
- "Why is my EF Core migration failing with 'Unable to create an object of type AppDbContext'?"
- "Write a Product model with Id, Name, Price, and Stock fields, following EF Core conventions."
- "Review this controller for REST best practices and suggest improvements."

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server or SQL Server LocalDB (installed with Visual Studio, or standalone)

## Running Locally

1. Restore dependencies:
   ```
   dotnet restore
   ```

2. Configure the connection string in `ProductApi/appsettings.json` (or `appsettings.Development.json`) if your SQL Server setup differs from the default LocalDB instance:
   ```
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ProductApiDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```

3. Database setup — migrations run automatically on startup (`db.Database.Migrate()` in `Program.cs`), so no manual step is required. To apply migrations manually instead:
   ```
   dotnet ef database update --project ProductApi
   ```

4. Run the project:
   ```
   dotnet run --project ProductApi
   ```

5. Open the Swagger UI to explore the API (URL printed in the console, typically):
   ```
   https://localhost:{port}/swagger
   ```

## Project Structure

- `Controllers/` — API endpoints
- `Models/` — entity classes
- `Data/` — `AppDbContext` and EF Core configuration
- `Migrations/` — EF Core migrations
