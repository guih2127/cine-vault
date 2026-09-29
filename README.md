# 🎬 CineVault

A full-stack movie-rating application built with **.NET 10** (Clean Architecture + TDD) and **Angular 20**.
Users register, log in, browse a movie catalog, and keep a personal list of rated movies with full CRUD.

## User story

> _As a movie enthusiast, I want to rate the movies I've watched and keep track of them in one place,
> so that I can remember my opinions and revisit or change my ratings later._
>
> I create an account and log in. From my **My Movies** page I add a movie from the catalog with a
> rating; later I can update that rating or remove the movie from my list.

## Tech stack

| Area | Choice |
|------|--------|
| Backend | .NET 10, ASP.NET Core Web API (controllers) |
| Architecture | Clean Architecture (Domain / Application / Infrastructure / API) |
| Persistence | Entity Framework Core + **SQLite** (file-based, zero setup) |
| Auth | JWT bearer, PBKDF2 password hashing |
| API docs | OpenAPI + **Scalar** UI |
| Testing | xUnit, FluentAssertions, Moq — **73 tests**, TDD throughout |
| Frontend | Angular 20 (standalone components + signals), Angular Material |

## Architecture

Dependencies point **inward** — the domain has no dependencies; outer layers depend on inner ones.

```
API  ──►  Application  ──►  Domain
              ▲
              │  implements the ports (interfaces)
       Infrastructure
```

- **Domain** — entities (`Movie`, `User`, `MovieReview`) and business invariants. No framework dependencies.
- **Application** — use cases + `Result<T>` + the **ports** (`IUserRepository`, `IPasswordHasher`, `ITokenService`, ...). Depends only on Domain.
- **Infrastructure** — EF Core repositories, JWT and password-hashing **adapters** that implement the ports.
- **API** — controllers, JWT auth, and the single place that maps business errors to HTTP status codes.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 20.19+ (or 22.12+ / 24.x) and npm

## Getting started

The app has two parts — run each in its own terminal.

### 1. Backend (API)

```bash
dotnet run --project src/Api
```

- Serves on **http://localhost:5055** (and https://localhost:7253).
- On startup it **applies EF migrations and seeds** the SQLite database automatically — no manual DB setup.
- Interactive API docs (Scalar) at **http://localhost:5055/scalar**.

### 2. Frontend (Angular)

```bash
cd frontend
npm install
npm start
```

- Serves on **http://localhost:4200** and talks to the API at `http://localhost:5055/api`.

Open **http://localhost:4200** and sign in.

## Demo credentials

The database is seeded with a demo user and a catalog of 5 movies:

- **Email:** `demo@cinevault.com`
- **Password:** `Demo123!`

You can also register a new account from the **Register** page.

## API reference

Base URL: `http://localhost:5055/api`. All endpoints require a `Bearer` token except register and login.

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/auth/register` | — | Create an account (`{ name, email, password }`) |
| POST | `/auth/login` | — | Log in, returns `{ token }` |
| GET | `/movies` | ✅ | List the movie catalog |
| GET | `/reviews` | ✅ | List the current user's reviews |
| POST | `/reviews` | ✅ | Rate a movie (`{ movieId, rating }`) |
| PUT | `/reviews/{id}` | ✅ | Update a review's rating (`{ rating }`) |
| DELETE | `/reviews/{id}` | ✅ | Remove a review |

The `userId` is always taken from the JWT, never from the request body.

## Running the tests

```bash
dotnet test
```

73 tests across four projects — Domain (23), Application (20), Infrastructure (20, integration
tests against in-memory SQLite) and API (10, endpoint tests via `WebApplicationFactory`).

## Project structure

```
src/
  Domain/           entities + business rules (no dependencies)
  Application/      use cases, Result<T>, ports (interfaces)
  Infrastructure/   EF Core, repositories, JWT + hashing adapters, seeder
  Api/              controllers, JWT auth, Program.cs (composition root)
tests/
  Domain.Tests/  Application.Tests/  Infrastructure.Tests/  Api.Tests/
frontend/           Angular 20 app (auth + My Movies)
```
