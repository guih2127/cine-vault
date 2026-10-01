# Generative AI — Generating a Task Management API

> **Scope note.** This is the *standalone GenAI exercise* from the brief, not the CineVault app.
> The brief asks me to imagine generating a RESTful **task management** API (CRUD of tasks with
> `title`, `description`, `status`, `due_date`, each task associated to a `User`) and to show:
> (1) the prompt I'd use, (2) a representative sample of the output, and (3) how I validated,
> corrected, and handled edge cases / auth / validations.
>
> The tool I used is **Claude Code** (same workflow I use in Cursor / Copilot). The code in
> section 2 is the **literal, unedited output** of running the prompt in section 1 — defects and all.
> That's deliberate: section 4 critiques the *real* problems the generation introduced, which is the
> point the brief is grading ("critical thinking when evaluating AI-generated code"). The yardstick
> I measure the output against is the set of patterns I actually shipped in CineVault's backend.

---

## 1. The prompt

I don't ask for "generate a tasks API" in one line. A one-liner produces a `TasksController` with a
`DbContext` injected straight in, business rules in the controller, and zero tests — exactly what the
evaluation penalizes. I treat the prompt as a **spec**: domain, enforced architecture, conventions,
output contract, and a working order (TDD, inside-out).

```text
# Context
I need a REST API in .NET 10 / C# for a task management system.
This is evaluated on Clean Architecture, test coverage, and code quality.

# Domain
- User (Id: Guid, Name, Email [unique], PasswordHash) — authentication.
- TaskItem (Id: Guid, Title, Description?, Status, DueDate?, OwnerId -> User).
  - Status is an enum: Todo, InProgress, Done.
  - Title is required; DueDate, when provided, cannot be in the past (at creation).
  - A task belongs to a user; only the owner may read/update/delete it.

# Architecture (MANDATORY — 4 projects, dependencies pointing inward)
- Domain:         entities with a private constructor + static factory Create();
                  private setters; invariants validated in the domain, throwing DomainException.
                  No EF / ASP.NET dependency.
- Application:    use cases (class XUseCase with an Execute method, NO MediatR).
                  Ports (interfaces) in Application/Abstractions. Expected business flow returns
                  Result<T> + Error(ErrorType, Code, Message). ErrorType = Validation, NotFound,
                  Conflict, Unauthorized, Forbidden. No EF here.
- Infrastructure: EF Core (SQLite), repositories implementing the ports, PasswordHasher (PBKDF2),
                  JWT token service, EF config via IEntityTypeConfiguration (domain has no EF attributes).
- Api:            Controllers (MVC). Map Result/ErrorType -> HTTP in a ToActionResult() extension.
                  JWT bearer. Fallback policy: everything protected except [AllowAnonymous].
                  OwnerId ALWAYS comes from the token (sub claim), NEVER from the request body.

# Conventions
- Descriptive names carrying the role suffix (TokenService, not tokens; TaskRepository, not repo).
- No code comments (no // and no XML docs) — the name reveals intent.
- Request DTOs separate from entities.

# How I want you to work (TDD)
Go layer by layer, inside-out. For each piece: write the tests first (xUnit), show them, then the
implementation that makes them pass.
```

Deliberate prompt-engineering choices here:

- **Architecture is enforced, not suggested.** Without this the model gravitates to the "everything in
  the controller" default.
- **Explicit error contract** (`Result<T>` + `ErrorType`). Stops the model from using exceptions as
  control flow or returning `BadRequest("loose string")`.
- **A security rule in the prompt** ("OwnerId always from the token"). This is the most common
  authorization flaw AI introduces — cheaper to forbid up front than to catch later.
- **TDD, inside-out.** Forces tests to exist and come first, and keeps each batch small enough to
  actually review instead of rubber-stamp.

Even with all of that in the prompt, the output still shipped several defects (section 4). That's the
lesson: a strong prompt **raises the floor**, it does not remove the review.

---

## 2. Output (literal, unedited)

**Domain — the enum and the entity.** Note two things the model chose on its own: the enum is named
`TaskStatus` (collides with `System.Threading.Tasks.TaskStatus`, forcing a `using` alias in every
file), and the past-date invariant reads the system clock directly with `DateTime.UtcNow`.

```csharp
// src/Domain/Enums/TaskStatus.cs
namespace Domain.Enums;

public enum TaskStatus
{
    Todo,
    InProgress,
    Done
}
```

```csharp
// src/Domain/Entities/TaskItem.cs
using Domain.Exceptions;
using TaskStatus = Domain.Enums.TaskStatus;

namespace Domain.Entities;

public sealed class TaskItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public TaskStatus Status { get; private set; }
    public DateTime? DueDate { get; private set; }
    public Guid OwnerId { get; private set; }

    private TaskItem() { Title = string.Empty; }

    public static TaskItem Create(string title, string? description, DateTime? dueDate, Guid ownerId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Task title is required.");

        if (ownerId == Guid.Empty)
            throw new DomainException("A task must belong to a user.");

        if (dueDate.HasValue && dueDate.Value < DateTime.UtcNow)
            throw new DomainException("Due date cannot be in the past.");

        return new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = description,
            DueDate = dueDate,
            OwnerId = ownerId,
            Status = TaskStatus.Todo
        };
    }

    public void UpdateDetails(string title, string? description, DateTime? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Task title is required.");

        if (dueDate.HasValue && dueDate.Value < DateTime.UtcNow)
            throw new DomainException("Due date cannot be in the past.");

        Title = title.Trim();
        Description = description;
        DueDate = dueDate;
    }

    public void ChangeStatus(TaskStatus status) => Status = status;
}
```

**Application — the Result/Error plumbing** (this part came out well, including an ergonomic implicit
conversion from `Error` to `Result<T>`):

```csharp
// src/Application/Common/Result.cs
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }

    private Result(bool isSuccess, T? value, Error? error) { IsSuccess = isSuccess; Value = value; Error = error; }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(Error error) => new(false, default, error);

    public static implicit operator Result<T>(Error error) => Failure(error);
}

// src/Application/Common/Error.cs
public sealed record Error(ErrorType Type, string Code, string Message)
{
    public static Error NotFound(string code, string message) => new(ErrorType.NotFound, code, message);
    public static Error Forbidden(string code, string message) => new(ErrorType.Forbidden, code, message);
    public static Error Validation(string code, string message) => new(ErrorType.Validation, code, message);
    // Conflict, Unauthorized ...
}
```

**Application — the update use case.** Ownership check is here (good), but notice it `catch`es
`DomainException` and re-wraps it as a `Validation` error — mixing two error channels:

```csharp
// src/Application/UseCases/Tasks/UpdateTaskUseCase.cs
public async Task<Result<TaskItem>> Execute(UpdateTaskCommand command, CancellationToken cancellationToken = default)
{
    var task = await _taskRepository.GetByIdAsync(command.TaskId, cancellationToken);

    if (task is null)
        return Error.NotFound("task.not_found", "Task not found.");

    if (task.OwnerId != command.OwnerId)
        return Error.Forbidden("task.forbidden", "You are not allowed to modify this task.");

    try
    {
        task.UpdateDetails(command.Title, command.Description, command.DueDate);
        task.ChangeStatus(command.Status);
        await _taskRepository.UpdateAsync(task, cancellationToken);
        return Result<TaskItem>.Success(task);
    }
    catch (DomainException ex)
    {
        return Error.Validation("task.invalid", ex.Message);
    }
}
```

**Api — the controller.** Here the generation drifted: `Update`/`Create` go through use cases, but
`GetById` and `Delete` fetch from the repository and run the ownership check **inline in the
controller**, return the domain entity directly (`Ok(task)`), and `GetOwnerId()` parses the claim with
no guard:

```csharp
// src/Api/Controllers/TasksController.cs  (excerpt)
[HttpGet("{id:guid}")]
public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
{
    var ownerId = GetOwnerId();
    var task = await _taskRepository.GetByIdAsync(id, cancellationToken);

    if (task is null) return NotFound();
    if (task.OwnerId != ownerId) return Forbid();

    return Ok(task);
}

[HttpDelete("{id:guid}")]
public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
{
    var ownerId = GetOwnerId();
    var task = await _taskRepository.GetByIdAsync(id, cancellationToken);

    if (task is null) return NotFound();
    if (task.OwnerId != ownerId) return Forbid();

    await _taskRepository.DeleteAsync(task, cancellationToken);
    return NoContent();
}

private Guid GetOwnerId()
{
    var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
    return Guid.Parse(sub!);
}
```

The EF config (string-converted enum, FK + index on `OwnerId`) and the domain tests came out clean and
idiomatic — I'm omitting them here for length.

---

## 3. How I validated the AI's suggestions

I don't trust the output because "it compiles." My validation order:

1. **Read the tests first.** Because the prompt is TDD, the model hands over the tests before the
   implementation — that's where intent is visible. I check *which edge cases have a test*; the ones
   that don't, don't exist. Here the domain tests covered missing title, past due date, and empty
   owner — but every time-based test was written as `DateTime.UtcNow.AddDays(±n)`, which is a smell
   (see §4, the clock issue).
2. **Build + test as a gate.** `dotnet test` green and `dotnet build -warnaserror` clean is the floor,
   not the ceiling. (In CineVault this discipline landed at 73 tests, zero warnings.)
3. **Read the layer boundaries.** The classic AI slip is a leaked dependency or leaked logic. I grep
   the Application project for `Microsoft.EntityFrameworkCore`, and I read the controller to see whether
   business rules crept in. That's exactly how I caught the `GetById`/`Delete` ownership logic sitting
   in the controller.
4. **Exercise the error paths, not the happy path.** Via Scalar/curl I hit 401/403/404/409 — not 200.
   The model nails the happy path; the failure paths are where it's wrong (e.g. the unguarded
   `Guid.Parse` returning 500 instead of 401 on a bad token).

---

## 4. What I corrected / improved

These are the **actual** defects in the output above, not hypotheticals:

- **`DateTime.UtcNow` inside the domain** (`TaskItem.Create` / `UpdateDetails`). Couples the entity to
  the system clock and makes the "past due date" invariant non-deterministic to test — which is why the
  tests leaned on `UtcNow.AddDays(...)`. Fix: pass `today`/`now` in as a parameter (or inject an
  `IClock` port). The test then feeds a fixed date and the boundary becomes exactly testable.
- **Enum named `TaskStatus`** collides with `System.Threading.Tasks.TaskStatus`, which is why every file
  needs `using TaskStatus = Domain.Enums.TaskStatus;`. Fix: rename to `TaskItemStatus`. Small thing, but
  it's the kind of ambient friction that spreads across the whole codebase.
- **Enum serialized as an integer over the wire.** EF persists `Status` as a string
  (`HasConversion<string>()`), but the API has no `JsonStringEnumConverter`, so JSON emits/accepts
  `2` instead of `"Done"` — storage and transport disagree. Fix: register
  `JsonStringEnumConverter` so the API speaks `"Done"` too.
- **Ownership logic leaked into the controller.** `Update`/`Create` go through use cases, but `GetById`
  and `Delete` do the fetch + owner check inline and return the domain entity via `Ok(task)`. That's
  three problems: the authorization rule now lives in two places (and the controller copy has no
  Application-layer test), the domain entity leaks out as the API contract, and it undoes the Clean
  Architecture the prompt asked for. Fix: `GetTaskByIdUseCase` / `DeleteTaskUseCase` behind
  `ToActionResult()`, returning a response DTO — the same shape `Update` already uses.
- **Unguarded `Guid.Parse(sub!)` in `GetOwnerId()`.** A missing or malformed `sub` throws, surfacing as
  an unhandled 500 instead of a clean 401. Fix: `TryParse` and short-circuit to `Unauthorized` — and
  disable inbound claim remapping (`MapInboundClaims = false`) so `sub` is read verbatim rather than
  silently remapped to `NameIdentifier`.
- **`UpdateDetails` re-validates the past-due-date rule.** The spec said "at creation" only. As written,
  editing a task (e.g. just moving it to `Done`) whose due date is already in the past would throw —
  a real bug for the most common update flow. Fix: drop the re-check on update, or make it explicit and
  tested as a deliberate rule.
- **`try/catch DomainException` in the use cases.** Defensible, but it mixes two error channels. In
  CineVault I let `DomainException` bubble to a single `ExceptionHandlingMiddleware` that maps it to 400,
  keeping use cases free of try/catch. Either is fine — the point is to pick one on purpose, not have
  both by accident.

---

## 5. How I handled edge cases, authentication, and validations

**Edge cases** (each becomes a test):
- Empty / whitespace title → `DomainException` → 400.
- Empty `OwnerId` → `DomainException` (defense in depth; the real owner always comes from the token).
- `DueDate` in the past at creation → blocked in the domain. On **update** it is *not* re-validated, so
  an overdue task can still be moved to `Done` (this is the fix to the bug in §4).
- Update/Delete/Get of a non-existent task → `NotFound` → 404.
- Update/Delete/Get of **another** user's task → `Forbidden` → 403 (checked *after* existence, so a
  missing task is 404, not 403).
- Missing/malformed JWT `sub` → `Unauthorized` → 401 (the guarded parse from §4).

**Authentication / authorization:**
- JWT bearer with a **fallback `RequireAuthenticatedUser` policy**: every endpoint is protected by
  default; only `register`/`login` carry `[AllowAnonymous]`. Secure-by-default — forgetting an
  `[Authorize]` can't open a hole. (The raw output relied on this policy but never wired it — a thing
  to verify, not assume.)
- **`OwnerId` always from the token** (`sub` claim), never from the body — a single
  `GetUserId()`/`ClaimsPrincipal` helper centralizes it. Stops a user acting on another's tasks by
  forging the payload.
- User-enumeration defense on login: same `Unauthorized` for unknown email and wrong password.
- Passwords hashed with PBKDF2 (`PasswordHasher<T>`) behind an `IPasswordHasher` port — never stored or
  logged in plain text.

**Validations** — two layers, on purpose:
- **Domain invariants** (title required, valid owner, creation due-date) live in `TaskItem` and throw
  `DomainException`. These can never be violated regardless of caller.
- **Expected application flow** (not found, conflict, forbidden) returns `Result<T>` + `Error`, mapped
  to the right HTTP status in a single `ToActionResult()` extension. The controller has no error
  `if/else`.

---

## 6. Critical read — where AI helps and where it's confidently wrong

- **Big accelerator:** repetitive, symmetric boilerplate — the `Result`/`Error` plumbing (the implicit
  operator was a nice touch), EF configs, DTOs, the use-case skeleton, the arrange block of tests.
  High-volume mechanical work it does well.
- **Confidently wrong, in this exact run:** it coupled the domain to the clock, picked a colliding type
  name, let storage and transport disagree on the enum, left an unguarded claim parse, and silently
  *drifted* — applying Clean Architecture to two endpoints and skipping it on the other two. Nothing
  here fails to compile; it all fails on review. These are precisely the things the rubric grades.
- **My rule:** the AI writes the code; **I own the design and the tests.** The prompt carries the
  architecture and raises the floor, and the tests are the contract that keeps the generation honest —
  but the drift above is why a strong prompt reduces review, it doesn't replace it. Small batches + TDD
  turn the AI from "a code generator I hope is right" into "a pair I review at every step."
