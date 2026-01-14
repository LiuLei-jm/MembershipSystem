# Production Environment Variables Setup

For the production deployment of the Membership System API, you need to set the following environment variables to configure the application properly.

## Required Environment Variables

### Database Configuration
- **`Database__ConnectionString`** or **`ConnectionStrings__SqliteConnection`**
  - Example: `Data Source=MembershipSystem.db`
  - This sets the connection string for the SQLite database in production
  - Note: Use double underscores (`__`) to map to nested JSON configuration properties

### JWT Configuration
- **`Jwt__SecretKey`**
  - Example: `A-very-long-and-secure-JWT-secret-key-for-production`
  - This should be a strong, unique secret key for JWT token signing in production

## Setting Environment Variables

### On Windows (Command Prompt):
```
set Database__ConnectionString=Data Source=MembershipSystem.db
set ConnectionStrings__SqliteConnection=Data Source=MembershipSystem.db
set Jwt__SecretKey=A-very-long-and-secure-JWT-secret-key-for-production
```

### On Windows (PowerShell):
```
$env:Database__ConnectionString="Data Source=MembershipSystem.db"
$env:ConnectionStrings__SqliteConnection="Data Source=MembershipSystem.db"
$env:Jwt__SecretKey="A-very-long-and-secure-JWT-secret-key-for-production"
```

### On Linux/macOS:
```
export Database__ConnectionString="Data Source=MembershipSystem.db"
export ConnectionStrings__SqliteConnection="Data Source=MembershipSystem.db"
export Jwt__SecretKey="A-very-long-and-secure-JWT-secret-key-for-production"
```

## Docker Environment Variables
If deploying with Docker, you can set these in your docker run command:
```
docker run -e Database__ConnectionString="Data Source=MembershipSystem.db" \
           -e ConnectionStrings__SqliteConnection="Data Source=MembershipSystem.db" \
           -e Jwt__SecretKey="A-very-long-and-secure-JWT-secret-key-for-production" \
           your-membership-api-image
```

Or in a docker-compose.yml file:
```yaml
version: '3.8'
services:
  membership-api:
    image: your-membership-api-image
    environment:
      - Database__ConnectionString=Data Source=MembershipSystem.db
      - ConnectionStrings__SqliteConnection=Data Source=MembershipSystem.db
      - Jwt__SecretKey=A-very-long-and-secure-JWT-secret-key-for-production
```

## Notes
- The .NET configuration system automatically reads environment variables and maps them to configuration settings
- The double underscore (`__`) in environment variable names maps to nested JSON properties
  - `Database__ConnectionString` maps to `Database:ConnectionString` in JSON
  - `ConnectionStrings__SqliteConnection` maps to `ConnectionStrings:SqliteConnection` in JSON
  - `Jwt__SecretKey` maps to `Jwt:SecretKey` in JSON
- Environment variables take precedence over values in configuration files
- For security reasons, never commit actual secret values to version control