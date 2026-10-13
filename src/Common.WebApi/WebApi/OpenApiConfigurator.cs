using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Ploch.Common.ArgumentChecking;

namespace Ploch.Common.WebApi;

/// <summary>
///     Extension methods that register OpenAPI document generation for a Web API.
/// </summary>
public static class OpenApiConfigurator
{
    /// <summary>
    ///     Registers the built-in OpenAPI document provider together with Swagger generation for a single API version.
    /// </summary>
    /// <param name="services">The service collection to add the OpenAPI services to.</param>
    /// <param name="apiDescription">
    ///     The document metadata (title, version, description, contact, licence) published for the API.
    /// </param>
    /// <param name="apiVersionString">
    ///     The document name the generated Swagger document is registered under. Defaults to <c>v1</c>.
    /// </param>
    /// <returns>
    ///     The same <paramref name="services" /> instance, so further calls can be chained.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="services" />, <paramref name="apiDescription" /> or
    ///     <paramref name="apiVersionString" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="apiVersionString" /> is empty.
    /// </exception>
    /// <example>
    ///     <code>
    /// builder.Services.ConfigureOpenApiOptions(new OpenApiInfo
    ///                                          {
    ///                                              Title = "Orders API",
    ///                                              Version = "v1"
    ///                                          });
    ///     </code>
    /// </example>
    public static IServiceCollection ConfigureOpenApiOptions(this IServiceCollection services, OpenApiInfo apiDescription, string apiVersionString = "v1")
    {
        services.NotNull();
        apiDescription.NotNull();
        apiVersionString.NotNullOrEmpty();

        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
                               {
                                   options.CustomOperationIds(BuildOperationId);
                                   options.SwaggerDoc(apiVersionString, apiDescription);
                               });

        return services;
    }

    // ActionDescriptor.RouteValues has no "controller" entry for endpoints registered outside MVC —
    // Minimal API and FastEndpoints routes among them — and the dictionary indexer throws
    // KeyNotFoundException on a missing key, so Swagger generation would fail for exactly the
    // endpoint styles this library exists to support. Falls back to the route itself.
    internal static string BuildOperationId(ApiDescription apiDescription)
    {
        // ActionDescriptor is settable and not guaranteed populated for every ApiExplorer provider,
        // and this method exists precisely because the previous version assumed a shape it did not get.
        var routeValues = apiDescription.ActionDescriptor?.RouteValues;
        var relativePath = apiDescription.RelativePath;

        if (routeValues is not null && routeValues.TryGetValue("controller", out var controller) && !string.IsNullOrEmpty(controller))
        {
            // Controller plus verb is the ordinary shape of a REST controller, not a unique name:
            // "GET /orders", "GET /orders/{id}" and "GET /orders/{id}/items" all reduced to
            // "OrdersGET". OpenAPI requires operationId to be unique across the document, and
            // client generators either fail or silently drop the duplicates. The route-derived
            // suffix the fallback branch already uses separates the routes without having to detect
            // which of them collided, and the controller name stays in front for readability.
            // The action name is the last-resort discriminator, since two actions can share a name
            // (overloads) but not a route and verb.
            var discriminator = relativePath;
            if (string.IsNullOrEmpty(discriminator))
            {
                // A provider that leaves RelativePath unset can still carry the attribute-route
                // template, which separates routes exactly as the path would.
                discriminator = apiDescription.ActionDescriptor?.AttributeRouteInfo?.Template;
            }

            if (string.IsNullOrEmpty(discriminator) && routeValues.TryGetValue("action", out var action))
            {
                // Same-named actions on one controller still share an id here, but nothing
                // route-shaped is left to separate them: ActionDescriptor.Id is regenerated per
                // process, so using it would trade a rare collision for guaranteed non-determinism.
                discriminator = action;
            }

            if (string.IsNullOrEmpty(discriminator))
            {
                return $"{controller}{apiDescription.HttpMethod}";
            }

            return $"{controller}{apiDescription.HttpMethod}_{StableSuffix(discriminator)}";
        }

        if (string.IsNullOrEmpty(relativePath))
        {
            return apiDescription.HttpMethod ?? string.Empty;
        }

        // Normalisation is many-to-one and there is no reliable local test for when it collided:
        // "orders/{id}" vs "orders/id" (dropped braces), "orders-/id" vs "orders/id" (collapsed
        // separators) and "orders/items" vs "orders-items" (separators that normalise alike) all
        // reduce to the same name by different routes. Rather than enumerate the ways information
        // is lost — three attempts, three misses — every generated id carries the suffix, so
        // uniqueness follows from the route itself instead of from spotting every lossy case.
        return $"{apiDescription.HttpMethod}_{NormaliseRoute(relativePath)}_{StableSuffix(relativePath)}";
    }

    // Route templates carry characters an operationId should not: "orders/{id}" would become
    // "orders_{id}", and braces break many OpenAPI client generators. Separators collapse to single
    // underscores and anything else non-alphanumeric is dropped; the caller restores uniqueness.
    private static string NormaliseRoute(string relativePath)
    {
        var builder = new StringBuilder(relativePath.Length);

        foreach (var character in relativePath)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
            }
            else if (character is '/' or '-' or '.' && builder.Length > 0 && builder[^1] != '_')
            {
                builder.Append('_');
            }
        }

        return builder.ToString().Trim('_');
    }

    // Deterministic across processes and runs. string.GetHashCode is randomised per process, so a
    // generated document would differ between runs and break diffing of committed OpenAPI specs.
    // Eight bytes, not four: a 32-bit digest is small enough for two ordinary routes to share one
    // ("orders/collision-32524" and "orders/collision-68690" both truncate to 61565973), which would
    // reintroduce the duplicate operationId this suffix exists to prevent. 64 bits keeps the id
    // readable while making a collision within one document vanishingly unlikely.
    private static string StableSuffix(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));

        return Convert.ToHexString(hash, 0, 8).ToLowerInvariant();
    }
}
