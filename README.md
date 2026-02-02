# PrecastTracker

A comprehensive quality control and production tracking system for the precast concrete manufacturing industry. PrecastTracker manages the complete workflow from job scheduling through concrete testing, providing traceability for all production activities and test results.

## Overview

PrecastTracker helps precast concrete manufacturers track:
- **Production Scheduling**: Job assignments to casting beds (Pours)
- **Concrete Batching**: Mix designs and batch tracking
- **Placements**: Concrete placement operations with full traceability
- **Quality Control**: Comprehensive test cylinder management with 1-day, 7-day, and 28-day testing
- **Compliance**: Complete audit trail for quality assurance and regulatory requirements

See [Specs/glossary.md](./Specs/glossary.md) for detailed domain terminology and data model documentation.

## Technology Stack

### Backend
- **.NET 9.0** - Web API and business logic
- **Entity Framework Core 9.0** - Data access with SQLite
- **ASP.NET Core** - REST API with OpenAPI/Swagger

### Frontend
- **SvelteKit** - Modern reactive UI framework
- **TypeScript** - Type-safe JavaScript
- **Vite** - Fast build tool and dev server
- **Tailwind CSS** - Utility-first styling
- **Playwright** - End-to-end testing

### Project Structure
```
PrecastTracker.sln                    # Main solution file
├── PrecastTracker.WebApi/            # REST API and web host
│   ├── Controllers/                  # API endpoints
│   ├── client-app/                   # SvelteKit frontend
│   └── Program.cs                    # Application entry point
├── PrecastTracker.Data/              # Entity Framework data layer
│   ├── Entities/                     # Domain entities
│   ├── Repositories/                 # Data access repositories
│   └── PrecastDbContext.cs          # EF Core context
├── PrecastTracker.Business/          # Business logic layer
├── PrecastTracker.Services/          # Application services
├── PrecastTracker.Contracts/         # DTOs and contracts
├── PrecastTracker.DataSeeder/        # Development data seeding tool
├── PrecastTracker.Tests/             # Unit and integration tests
└── Specs/                            # Domain documentation
    └── glossary.md                   # Domain model and terminology
```

## Prerequisites

- **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Node.js 18+** - [Download](https://nodejs.org/)
- **npm** or **pnpm** - Package manager

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/5ha/precast.git
cd precast
```

### 2. Set Up the Database

The application uses SQLite and will create the database automatically on first run. The database file is stored at `PrecastTracker.WebApi/App_Data/PrecastTracker.db`.

To apply migrations manually:

```bash
# From the solution root
dotnet ef database update --project PrecastTracker.Data --startup-project PrecastTracker.WebApi
```

### 3. Seed Development Data (Optional)

For development and testing, use the DataSeeder tool to populate the database with realistic test data:

```bash
# See available scenarios
dotnet run --project PrecastTracker.DataSeeder

# Run a specific scenario (e.g., typical production day)
dotnet run --project PrecastTracker.DataSeeder TypicalProductionDay
```

See [DataSeeder.md](./DataSeeder.md) for complete documentation on available scenarios.

### 4. Run the Backend API

```bash
# From the solution root
dotnet run --project PrecastTracker.WebApi

# Or from the WebApi project directory
cd PrecastTracker.WebApi
dotnet run
```

The API will be available at:
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001
- **Swagger UI**: http://localhost:5000/swagger

### 5. Run the Frontend

In a separate terminal:

```bash
# Navigate to the frontend directory
cd PrecastTracker.WebApi/client-app

# Install dependencies (first time only)
npm install

# Start the development server
npm run dev
```

The frontend will be available at: http://localhost:5173

## Development Workflow

### Running Both Backend and Frontend

For active development, run both the API and frontend in separate terminals:

```bash
# Terminal 1: Backend API
dotnet run --project PrecastTracker.WebApi

# Terminal 2: Frontend
cd PrecastTracker.WebApi/client-app
npm run dev
```

### Building for Production

#### Backend
```bash
dotnet build --configuration Release
dotnet publish --configuration Release
```

#### Frontend
```bash
cd PrecastTracker.WebApi/client-app
npm run build
```

The frontend build outputs to `PrecastTracker.WebApi/client-app/build`.

## Testing

### Backend Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true
```

### Frontend Tests

```bash
cd PrecastTracker.WebApi/client-app

# Run E2E tests
npm run test:e2e

# Run type checking
npm run check
```

## Database Migrations

### Create a New Migration

```bash
dotnet ef migrations add <MigrationName> --project PrecastTracker.Data --startup-project PrecastTracker.WebApi
```

### Apply Migrations

```bash
dotnet ef database update --project PrecastTracker.Data --startup-project PrecastTracker.WebApi
```

### Reset Database

```bash
dotnet ef database drop --project PrecastTracker.Data --startup-project PrecastTracker.WebApi
dotnet ef database update --project PrecastTracker.Data --startup-project PrecastTracker.WebApi
```

Or use the DataSeeder tool which automatically resets the database:

```bash
dotnet run --project PrecastTracker.DataSeeder TypicalProductionDay
```

## Architecture

PrecastTracker follows a clean architecture pattern with clear separation of concerns:

- **WebApi**: HTTP endpoints, request/response handling, API contracts
- **Services**: Application orchestration, workflow coordination
- **Business**: Core business logic and domain rules
- **Data**: Entity Framework, repositories, database access
- **Contracts**: DTOs, request/response models, shared interfaces

### Key Principles

- **Minimal Includes**: Repositories use minimal includes; prefer projections for read operations
- **Service Ownership**: Services own their data access patterns
- **Explicit Testing**: Repository tests use in-memory database; service/business tests use mocking
- **Domain-Driven**: Entity model closely matches precast concrete industry workflows

See [CLAUDE.md](./CLAUDE.md) for development guidelines and patterns.

## API Documentation

When the API is running, you can access:

- **Swagger UI**: http://localhost:5000/swagger - Interactive API documentation
- **OpenAPI Spec**: http://localhost:5000/swagger/v1/swagger.json

## Contributing

### Development Guidelines

1. Consult [Specs/glossary.md](./Specs/glossary.md) for domain terminology
2. Follow existing patterns in [CLAUDE.md](./CLAUDE.md)
3. Write tests with clear comments explaining what is being verified
4. Use in-memory database for repository tests; use mocking for other layers
5. Keep entity includes minimal; prefer projections for read operations

### Code Style

- Use meaningful variable and method names
- Follow C# and TypeScript naming conventions
- Add comments only when necessary to explain complex logic
- Keep methods focused and single-purpose

## Common Tasks

### Resetting Development Environment

```bash
# Reset database and seed with typical data
dotnet run --project PrecastTracker.DataSeeder TypicalProductionDay

# Restart backend and frontend
dotnet run --project PrecastTracker.WebApi
cd PrecastTracker.WebApi/client-app && npm run dev
```

### Viewing Logs

The application logs to the console. For more detailed logging, adjust the log level in `appsettings.Development.json`.

### Troubleshooting

**Database Locked Errors**:
```bash
# Stop all running instances
# Delete the database file
rm PrecastTracker.WebApi/App_Data/PrecastTracker.db
# Recreate with seeder
dotnet run --project PrecastTracker.DataSeeder TypicalProductionDay
```

**Port Already in Use**:
```bash
# Backend: Update ports in PrecastTracker.WebApi/Properties/launchSettings.json
# Frontend: Use --port flag
npm run dev -- --port 5174
```

## License

This project is proprietary software. All rights reserved.

## Support

For questions or issues, please consult:
- [Domain Glossary](./Specs/glossary.md) - Understanding the precast concrete industry and data model
- [DataSeeder Documentation](./DataSeeder.md) - Development data scenarios
- [Development Guidelines](./CLAUDE.md) - Coding patterns and best practices
