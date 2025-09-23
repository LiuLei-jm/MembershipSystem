# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET 8.0 membership system with a REST API backend, WPF desktop client, and web client. The main components are:

1. **MembershipSystemAPI** - ASP.NET Core Web API using FastEndpoints framework
2. **MembershipSystemWPF** - WPF desktop application
3. **MembershipSystemWeb** - Web frontend application

## Key Technologies

- .NET 8.0
- FastEndpoints for API development
- Entity Framework Core with SQLite
- SignalR for real-time communication
- BCrypt for password hashing
- JWT for authentication

## Project Structure

```
MembershipSystem/
├── MembershipSystemAPI/     # Main API backend
│   ├── Data/               # Database context and migrations
│   ├── Endpoints/          # API endpoints organized by feature
│   ├── Models/             # Entity models
│   ├── Services/           # Business logic services
│   ├── Hubs/               # SignalR hubs
│   └── Program.cs          # Application entry point
├── MembershipSystemWPF/    # WPF desktop client
└── MembershipSystemWeb/    # Web frontend
```

## Core Functionality

The system provides membership card management with these key features:
- User registration and authentication
- API key-based authentication for clients
- Membership card creation with CDK generation
- Real-time file operations via SignalR
- Admin user management
- Automatic expiration processing for memberships

## Data Models

1. **User** - Core user entity with username, password hash, role, and status
2. **ApiKey** - API keys for client authentication
3. **MembershipCard** - Membership cards with CDK, duration, and amount

## API Structure

Endpoints are organized by feature in the Endpoints/ directory:
- Users/ - Registration, login, password management
- Admins/ - User management, password changes, user status toggling
- ApiKeys/ - API key generation and retrieval
- Memberships/ - Membership card creation and management
- FilePushes/ - File operations via SignalR

## Development Commands

### Building the Project
```bash
# Build the entire solution
dotnet build

# Build just the API project
dotnet build MembershipSystemAPI
```

### Running the Application
```bash
# Run the API
dotnet run --project MembershipSystemAPI
```

### Database Migrations
```bash
# Add a new migration
dotnet ef migrations add MigrationName --project MembershipSystemAPI

# Update the database
dotnet ef database update --project MembershipSystemAPI
```

### Running Tests
Tests are not currently configured in this project.

## Architecture Notes

1. **Authentication**: Uses JWT tokens for API authentication and API keys for SignalR
2. **Authorization**: Role-based access control (Admin/User)
3. **Rate Limiting**: Configured for login and registration endpoints
4. **Real-time Communication**: SignalR hub for file operations
5. **Background Services**: Hosted service for processing expired memberships
6. **CDK Generation**: Service for generating unique membership codes

## Key Configuration

- SQLite database at `MembershipSystem.db`
- JWT signing key in appsettings.json
- CORS enabled for all origins (development setting)