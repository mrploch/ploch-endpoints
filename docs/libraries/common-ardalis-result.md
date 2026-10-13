# Ploch.Common.Ardalis.Result

> Extension methods that bridge `Ardalis.Result` result status values to HTTP status codes.

## Overview

`Ploch.Common.Ardalis.Result` is a thin adapter library that adds HTTP concern awareness to the `Ardalis.Result` library's `ResultStatus` enumeration. It targets `netstandard2.0` and has a single dependency: the `Ardalis.Result` NuGet package.

The library is intended for use in Web API projects where result objects returned from use-case or service layers need to be translated into HTTP responses. Rather than writing the `ResultStatus → HTTP status code` switch statement in each endpoint, this library centralises the mapping as a single extension method on `ResultStatus`.

## Installation

```bash
dotnet add package Ploch.Common.Ardalis.Result
```

## Key Types

| Type | Description |
|------|-------------|
| `ResultStatusExtensions` | Static class providing the `ToHttpStatusCode(this ResultStatus)` extension method. |

## Usage Examples

### Mapping result status in a minimal API endpoint

```csharp
using Ardalis.Result;
using Ploch.Common.Ardalis.Result;

app.MapGet("/profiles/{id}", async (int id, IGetProfileUseCase useCase) =>
{
    var result = await useCase.ExecuteAsync(id);

    if (!result.IsSuccess)
    {
        return Results.StatusCode(result.Status.ToHttpStatusCode());
    }

    return Results.Ok(result.Value);
});
```

### Mapping result status in a controller

```csharp
using Ardalis.Result;
using Ploch.Common.Ardalis.Result;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProfilesController : ControllerBase
{
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, IDeleteProfileUseCase useCase)
    {
        var result = await useCase.ExecuteAsync(id);

        return result.IsSuccess
            ? NoContent()
            : StatusCode(result.Status.ToHttpStatusCode());
    }
}
```

### Using the extension directly

```csharp
using Ardalis.Result;
using Ploch.Common.Ardalis.Result;

int code = ResultStatus.NotFound.ToHttpStatusCode();   // 404
int ok   = ResultStatus.Ok.ToHttpStatusCode();         // 200
int err  = ResultStatus.Error.ToHttpStatusCode();      // 500
```

## Status Code Mapping

| `ResultStatus` | HTTP Status Code |
|---------------|-----------------|
| `Ok` | 200 |
| `Created` | 201 |
| `NoContent` | 204 |
| `Invalid` | 400 |
| `Unauthorized` | 401 |
| `Forbidden` | 403 |
| `NotFound` | 404 |
| `Conflict` | 409 |
| `Unavailable` | 503 |
| `Error` | 500 |
| Any other value | 500 |

## Related Libraries

- [Ploch.Common](common.md) — Core library. Does not directly depend on `Ploch.Common.Ardalis.Result`, but projects using both libraries benefit from the `Guard` argument checking and collection utilities when implementing use cases that return `Ardalis.Result` results.
