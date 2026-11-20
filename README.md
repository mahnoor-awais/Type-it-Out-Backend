# Type-It-Out Backend

A RESTful Web API for **Type-It-Out**, a social article-writing platform where users can publish articles, interact with other writers, and build a community.

## Tech Stack

- **Backend:** C# / ASP.NET Core Web API (.NET 10)
- **Database:** Microsoft SQL Server
- **ORM:** Entity Framework Core
- **Authentication:** JWT (JSON Web Tokens)
- **Password Security:** ASP.NET Core PasswordHasher
- **API Documentation & Testing:** Swagger / OpenAPI
- **Image Storage:** Cloudinary (planned integration)

## Features

- User registration and login
- JWT-based authentication
- Create, read, update, and delete articles
- Like and unlike articles
- Comment on articles
- Follow and unfollow users
- View user profiles
- Update user profiles
- View user articles, followers, and following lists

## API Endpoints

All endpoints use the following base URL:

```text
https://localhost:<port>/api
```

The actual port is determined by your local ASP.NET Core configuration.

### 1. Authentication — `/Auth`

| Method | Endpoint | Description | Authentication |
|---|---|---|---|
| POST | `/Auth/register` | Register a new user | Not required |
| POST | `/Auth/login` | Log in and receive a JWT token | Not required |

### 2. Articles — `/Articles`

| Method | Endpoint | Description | Authentication |
|---|---|---|---|
| GET | `/Articles/articles` | Retrieve all articles | Not required |
| POST | `/Articles/articles` | Create a new article | Required |
| GET | `/Articles/articleDetail` | Retrieve article details | Not required |
| GET | `/Articles/{id}/likes` | Get an article's likes | Not required |
| POST | `/Articles/{id}/likes` | Like an article | Required |
| DELETE | `/Articles/{id}/likes` | Unlike an article | Required |
| POST | `/Articles/{id}/comments` | Add a comment | Required |
| GET | `/Articles/{id}/comments` | Retrieve article comments | Not required |
| DELETE | `/Articles/{articleId}/comments/{commentId}` | Delete a comment | Required |
| PUT | `/Articles/{id}` | Update an article | Required |
| DELETE | `/Articles/{id}` | Delete an article | Required |

### 3. Follows — `/Follows`

| Method | Endpoint | Description | Authentication |
|---|---|---|---|
| POST | `/Follows/{followingId}` | Follow a user | Required |
| DELETE | `/Follows/{followingId}` | Unfollow a user | Required |

### 4. Users — `/User`

| Method | Endpoint | Description | Authentication |
|---|---|---|---|
| GET | `/User/{id}` | Retrieve a user's profile | Not required |
| PUT | `/User/profile` | Update the authenticated user's profile | Required |
| GET | `/User/{id}/articles` | Retrieve a user's articles | Not required |
| GET | `/User/{id}/followers` | Retrieve a user's followers | Not required |
| GET | `/User/{id}/following` | Retrieve users they follow | Not required |

**Note:** Authentication requirements are based on the current endpoint implementation. Refer to Swagger for exact request parameters, DTOs, and response formats.

## Authentication

The API uses JWT authentication.

1. Register an account using `/api/Auth/register`.
2. Log in using `/api/Auth/login`.
3. Receive a JWT access token.
4. Include the token in the Authorization header when accessing protected endpoints.

Header format:

```http
Authorization: Bearer YOUR_JWT_TOKEN
```

Protected operations use the authenticated user's identity to enforce ownership and permissions.

## Database

The project uses Microsoft SQL Server with Entity Framework Core.

Database name:

```text
TypeItOutDB
```

Main tables:

- `Users` — User accounts and profile information.
- `Articles` — Published articles.
- `Comments` — Comments associated with articles and users.
- `Likes` — Article likes and user relationships.
- `Follows` — Follower/following relationships.

## Local Setup

### Prerequisites

- .NET 10 SDK
- Microsoft SQL Server
- SQL Server Management Studio (SSMS)
- Visual Studio Code (or Visual Studio)

### 1. Clone the repository

```bash
git clone <repository-url>
cd Type-It-Out-Backend
```

### 2. Configure the database connection

Add your SQL Server connection string under `ConnectionStrings:DefaultConnection` in your local configuration.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TypeItOutDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 3. Configure JWT settings

Add your JWT configuration locally:

```json
{
  "Jwt": {
    "Issuer": "TypeItOut",
    "Audience": "TypeItOutUsers",
    "Key": "YOUR_SECRET_SIGNING_KEY"
  }
}
```

Use a strong secret key and keep credentials out of source control.

### 4. Restore dependencies

```bash
dotnet restore
```

### 5. Run the application

```bash
dotnet run
```

## API Documentation

Once the application is running in the Development environment, open the Swagger UI to explore, execute, and test the API endpoints.

```text
/swagger/index.html
```

Swagger provides interactive documentation for the available endpoints.

## Project Status

**Backend API:** Core functionality implemented and tested.

**Frontend:** Angular application planned.

**Image Uploads:** Cloudinary integration planned.

---

Built as part of the Type-It-Out project.
