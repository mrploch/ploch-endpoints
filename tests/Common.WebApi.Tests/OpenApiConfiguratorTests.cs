using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace Ploch.Common.WebApi.Tests;

public class OpenApiConfiguratorTests
{
    private static OpenApiInfo ApiInfo => new() { Title = "Orders API", Version = "v1" };

    [Fact]
    public void ConfigureOpenApiOptions_should_return_the_same_service_collection_to_allow_chaining()
    {
        var services = new ServiceCollection();

        services.ConfigureOpenApiOptions(ApiInfo).Should().BeSameAs(services);
    }

    [Fact]
    public void ConfigureOpenApiOptions_should_register_the_swagger_generator()
    {
        var services = new ServiceCollection();

        services.ConfigureOpenApiOptions(ApiInfo);

        services.Should().Contain(descriptor => descriptor.ServiceType.FullName!.Contains("ISwaggerProvider", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildOperationId_should_combine_the_controller_and_http_method_for_an_MVC_endpoint()
    {
        var apiDescription = new ApiDescription { HttpMethod = "GET", ActionDescriptor = new ActionDescriptor() };
        apiDescription.ActionDescriptor.RouteValues["controller"] = "Orders";

        OpenApiConfigurator.BuildOperationId(apiDescription).Should().Be("OrdersGET");
    }

    // The controller name leads the id so a generated client stays readable; only the
    // disambiguating suffix is appended.
    [Fact]
    public void BuildOperationId_should_prefix_a_controller_endpoint_with_the_controller_and_http_method()
    {
        ControllerOperationIdFor("GET", "Orders", "orders/{id}").Should().StartWith("OrdersGET_").And.MatchRegex("^[A-Za-z0-9_]+$");
    }

    // Regression: "OrdersGET" was returned for every GET action on OrdersController, so the ordinary
    // shape of a REST controller produced duplicate operationIds. OpenAPI requires them to be unique
    // across the document; duplicates make client generators fail or silently drop endpoints.
    [Theory]
    [InlineData("orders", "orders/{id}")]
    [InlineData("orders/{id}", "orders/{id}/items")]
    [InlineData("orders/{id}", "orders/id")]
    public void BuildOperationId_should_not_collide_when_one_controller_has_two_routes_for_the_same_http_method(string firstRoute, string secondRoute)
    {
        ControllerOperationIdFor("GET", "Orders", firstRoute).Should().NotBe(ControllerOperationIdFor("GET", "Orders", secondRoute));
    }

    // The suffix must not depend on per-process hash randomisation, or a generated document would
    // differ between runs and make committed OpenAPI specs undiffable. The expected id is the
    // literal SHA-256 value: comparing two calls in one process would also pass for
    // string.GetHashCode, which is stable within a process but randomised across processes.
    [Fact]
    public void BuildOperationId_should_be_deterministic_for_the_same_controller_route()
    {
        // First eight bytes of SHA-256("orders/{id}").
        ControllerOperationIdFor("GET", "Orders", "orders/{id}").Should().Be("OrdersGET_ef7dbd91889b7768");
    }

    // Regression: a four-byte suffix left ordinary routes sharing one value — these two truncate to
    // 61565973 alike — which reintroduced the duplicate id the suffix exists to prevent.
    [Fact]
    public void BuildOperationId_should_not_collide_for_routes_that_share_the_first_four_digest_bytes()
    {
        ControllerOperationIdFor("GET", "Orders", "orders/collision-32524")
            .Should()
            .NotBe(ControllerOperationIdFor("GET", "Orders", "orders/collision-68690"));
    }

    [Fact]
    public void BuildOperationId_should_still_distinguish_controller_endpoints_that_differ_only_by_http_method()
    {
        ControllerOperationIdFor("GET", "Orders", "orders/{id}").Should().NotBe(ControllerOperationIdFor("DELETE", "Orders", "orders/{id}"));
    }

    // An ApiExplorer provider is not obliged to populate RelativePath, but the attribute-route
    // template separates the routes just as well, so it is preferred over the action name.
    [Fact]
    public void BuildOperationId_should_fall_back_to_the_attribute_route_template_when_a_controller_endpoint_has_no_route()
    {
        var listAll = ControllerOperationIdFor("GET", "Orders", relativePath: null, actionName: "Get", routeTemplate: "orders");
        var getById = ControllerOperationIdFor("GET", "Orders", relativePath: null, actionName: "Get", routeTemplate: "orders/{id}");

        listAll.Should().StartWith("OrdersGET_");
        listAll.Should().NotBe(getById);
    }

    // With neither a route nor a template, the action name is the only route-distinguishing value
    // left, and it is better than nothing.
    [Fact]
    public void BuildOperationId_should_fall_back_to_the_action_name_when_a_controller_endpoint_has_no_route()
    {
        var listAll = ControllerOperationIdFor("GET", "Orders", relativePath: null, actionName: "ListAll");
        var getById = ControllerOperationIdFor("GET", "Orders", relativePath: null, actionName: "GetById");

        listAll.Should().StartWith("OrdersGET_");
        listAll.Should().NotBe(getById);
    }

    private static string ControllerOperationIdFor(string httpMethod, string controllerName, string? relativePath, string? actionName = null, string? routeTemplate = null)
    {
        var apiDescription = new ApiDescription { HttpMethod = httpMethod, RelativePath = relativePath, ActionDescriptor = new ActionDescriptor() };
        apiDescription.ActionDescriptor.RouteValues["controller"] = controllerName;
        if (actionName is not null)
        {
            apiDescription.ActionDescriptor.RouteValues["action"] = actionName;
        }

        if (routeTemplate is not null)
        {
            apiDescription.ActionDescriptor.AttributeRouteInfo = new AttributeRouteInfo { Template = routeTemplate };
        }

        return OpenApiConfigurator.BuildOperationId(apiDescription);
    }

    // Regression: this used the dictionary indexer, which throws KeyNotFoundException. Endpoints
    // registered outside MVC — Minimal API and FastEndpoints routes, the very styles this library
    // exists to support — carry no "controller" route value, so Swagger generation crashed.
    [Fact]
    public void BuildOperationId_should_fall_back_to_the_route_when_there_is_no_controller_route_value()
    {
        var apiDescription = new ApiDescription { HttpMethod = "POST", RelativePath = "orders/items", ActionDescriptor = new ActionDescriptor() };

        var act = () => OpenApiConfigurator.BuildOperationId(apiDescription);

        act.Should().NotThrow<KeyNotFoundException>();
        OpenApiConfigurator.BuildOperationId(apiDescription).Should().StartWith("POST_orders_items_");
    }

    // Braces from a route template would otherwise land in the operationId, which breaks many
    // OpenAPI client generators.
    [Fact]
    public void BuildOperationId_should_strip_route_parameter_braces_from_the_fallback()
    {
        OperationIdFor("GET", "orders/{id}/items").Should().StartWith("GET_orders_id_items").And.MatchRegex("^[A-Za-z0-9_]+$");
    }

    // Regression: normalisation is not injective — "orders/{id}" and "orders/id" both reduce to
    // "orders_id". OpenAPI requires operationIds to be unique or client generation breaks.
    [Fact]
    public void BuildOperationId_should_not_collide_when_two_routes_normalise_to_the_same_name()
    {
        OperationIdFor("GET", "orders/{id}").Should().NotBe(OperationIdFor("GET", "orders/id"));
    }

    // Regression: collapsing consecutive separators loses information too. Without flagging it,
    // "orders-/id" and "orders/id" both reduced to "GET_orders_id" with no disambiguator.
    [Fact]
    public void BuildOperationId_should_not_collide_when_two_routes_differ_only_by_a_collapsed_separator()
    {
        OperationIdFor("GET", "orders-/id").Should().NotBe(OperationIdFor("GET", "orders/id"));
    }

    // Separators that normalise alike ("/" and "-") were the third collision case found; every
    // generated id now carries the route-derived suffix rather than relying on detecting loss.
    [Fact]
    public void BuildOperationId_should_not_collide_when_two_routes_differ_only_by_separator_character()
    {
        OperationIdFor("GET", "orders/items").Should().NotBe(OperationIdFor("GET", "orders-items"));
    }

    [Fact]
    public void BuildOperationId_should_prefix_the_generated_id_with_the_method_and_normalised_route()
    {
        OperationIdFor("GET", "orders/id").Should().StartWith("GET_orders_id_").And.MatchRegex("^[A-Za-z0-9_]+$");
    }

    // The suffix must not depend on per-process hash randomisation, or a generated document would
    // differ between runs and make committed OpenAPI specs undiffable. Pinning the literal id, not
    // comparing two calls, is what makes this a guard: string.GetHashCode is stable within a single
    // process, so a self-comparison would pass even after a regression to it.
    [Fact]
    public void BuildOperationId_should_be_deterministic_for_the_same_route()
    {
        // First eight bytes of SHA-256("orders/{id}").
        OperationIdFor("GET", "orders/{id}").Should().Be("GET_orders_id_ef7dbd91889b7768");
    }

    [Fact]
    public void BuildOperationId_should_fall_back_when_the_action_descriptor_has_no_route_values()
    {
        var apiDescription = new ApiDescription { HttpMethod = "GET", RelativePath = "health", ActionDescriptor = new ActionDescriptor() };

        OpenApiConfigurator.BuildOperationId(apiDescription).Should().StartWith("GET_health_");
    }

    private static string OperationIdFor(string httpMethod, string relativePath) =>
        OpenApiConfigurator.BuildOperationId(new ApiDescription { HttpMethod = httpMethod, RelativePath = relativePath, ActionDescriptor = new ActionDescriptor() });

    [Fact]
    public void BuildOperationId_should_fall_back_to_the_http_method_when_there_is_no_route_either()
    {
        var apiDescription = new ApiDescription { HttpMethod = "DELETE", ActionDescriptor = new ActionDescriptor() };

        OpenApiConfigurator.BuildOperationId(apiDescription).Should().Be("DELETE");
    }

    [Fact]
    public void ConfigureOpenApiOptions_should_throw_when_the_service_collection_is_null()
    {
        var act = () => ((IServiceCollection)null!).ConfigureOpenApiOptions(ApiInfo);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ConfigureOpenApiOptions_should_throw_when_the_api_description_is_null()
    {
        var act = () => new ServiceCollection().ConfigureOpenApiOptions(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // NotNullOrEmpty delegates to ArgumentException.ThrowIfNullOrEmpty, which distinguishes the two
    // cases. Asserting only ArgumentException would pass either way, since ArgumentNullException
    // derives from it, so each case is pinned separately to match the documented contract.
    [Fact]
    public void ConfigureOpenApiOptions_should_throw_ArgumentNullException_when_the_api_version_string_is_null()
    {
        var act = () => new ServiceCollection().ConfigureOpenApiOptions(ApiInfo, null!);

        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void ConfigureOpenApiOptions_should_throw_ArgumentException_when_the_api_version_string_is_empty()
    {
        var act = () => new ServiceCollection().ConfigureOpenApiOptions(ApiInfo, string.Empty);

        // ThrowExactly, not Throw: ArgumentNullException derives from ArgumentException, so the
        // looser assertion would pass even if the empty case threw the null exception.
        act.Should().ThrowExactly<ArgumentException>();
    }
}
