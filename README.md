# Backend - SeePaw - Configuration Guide

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)
- Visual Studio 2022 or VS Code (optional)

## Initial Setup

### 1. Clone the repository
```bash
git clone <repository-url>
cd backend
```

### 2. Configure environment variables

Create a `.env` file in the project root folder with the following content:
```bash
# .env
POSTGRES_DB=seepaw
POSTGRES_USER=seepaw
POSTGRES_PASSWORD=seepawpwd
```

> ⚠️ **Important:** This file is **not** committed to Git (it's listed in `.gitignore`).  
> Normally, these credentials should never be sent to the repository or shared with anyone,  
> but they are included here because the project is part of an academic context.

### 3. Configure User Secrets (for local development)
```bash
cd API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=seepaw;Username=seepaw;Password=seepawpwd"
cd ..
```

### 4. Start the database

(Note: Docker Desktop must be running)
```bash
docker-compose up -d database
```
> 💡 **What this does:**  
> Creates and starts a **Docker container** running PostgreSQL.  
> The container is an isolated environment that runs the database without requiring a direct PostgreSQL installation on your PC.  
> The `-d` (detached) flag runs it in the background, freeing the terminal.

Check that it's healthy:
```bash
docker ps
```

You should see:
```bash
CONTAINER ID   IMAGE             STATUS
xxxxxx         postgres:latest   Up X seconds (healthy)
```

### 5. Run the backend application
```bash
dotnet watch
```

Or in Visual Studio: **F5** (select the **API** profile, not Docker Compose)

### 6. Test

Open your browser at: **https://localhost:5001/swagger**

If you see the Swagger interface, everything is OK! ✅

## Development Workflow

### Working with Migrations

#### Create a new migration

When adding or modifying entities:
```bash
# 1. Add the entity in the Domain layer
# 2. Add it to the DbContext (Persistence/AppDbContext.cs)
# 3. Create the migration
dotnet ef migrations add MigrationName

# 4. Apply the migration (or start the app, which applies it automatically)
dotnet ef database update (or start the app: dotnet watch)
```

#### Update the database after a pull

If a teammate added migrations relevant to your work:
```bash
git pull (or merge from the respective branch)
cd API
dotnet watch
```

> 💡 The application automatically applies migrations when starting up!

## 🐳 Useful Docker Commands
```bash
# List running containers
docker ps

# View database logs
docker-compose logs database

# Stop the database
docker-compose stop database

# Stop all containers, keeping their data
docker-compose down

# Remove everything (⚠️ including data!)
docker-compose down -v

⚠️ WARNING: The `-v` flag also removes volumes (where database data is stored).  
All data inserted/created locally will be permanently deleted!  
It’s recommended to configure a seed and add the seed file/class to .gitignore if you plan to run tests.

# Start only the database
docker-compose up -d database
```

## Health Checks for Containers

### Database
```bash
# Check status
docker ps

# Test connection
docker exec -it database pg_isready -U seepaw -d seepaw
```

### API
```bash
# Should display the created tables
curl https://localhost:5001/swagger
```

## Project Structure
```bash
backend/
├── .env                    # Docker environment variables (ignored by Git)
├── docker-compose.yml      # Docker configuration
├── API/                    # Controllers, middleware, and HTTP setup
├── Application/            # Business logic
├── Domain/                 # Entities
└── Persistence/            # Data access
    └── Migrations/         # Database schema change history
```

## 📚 Resources

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [PostgreSQL](https://www.postgresql.org/docs/)
- [Docker Compose](https://docs.docker.com/compose/)

---

**Note:** Always keep Docker Desktop running while developing!
