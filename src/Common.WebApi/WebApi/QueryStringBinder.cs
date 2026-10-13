using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Ploch.Common.WebApi;

/// <summary>
/// A static helper class for binding HTTP query string parameters to an instance of a specified type.
/// </summary>
public static class QueryStringBinder
{
    // Single source of truth for which property types can be bound and how each is read.
    // Query strings are a culture-neutral wire format, so values are read with the invariant
    // culture; bool has no culture-sensitive representation. A converter returns null to mean
    // "this value could not be parsed", which is unambiguous here because null values are
    // skipped before conversion and every supported type boxes to a non-null object.
    private static readonly Dictionary<Type, Func<string, object?>> Converters = new()
                                                                                 {
                                                                                     [typeof(string)] = value => value,
                                                                                     [typeof(int)] = value =>
                                                                                         int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null,
                                                                                     [typeof(bool)] = value => bool.TryParse(value, out var parsed) ? parsed : null,
                                                                                     [typeof(DateTime)] = value =>
                                                                                         DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : null,
                                                                                     [typeof(DateTimeOffset)] = value =>
                                                                                         DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : null,
                                                                                     [typeof(DateOnly)] = value =>
                                                                                         DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : null,
                                                                                     [typeof(TimeOnly)] = value =>
                                                                                         TimeOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : null
                                                                                 };

    /// <summary>
    /// Binds HTTP query string parameters from the given HttpContext to a new instance of the specified type.
    /// </summary>
    /// <typeparam name="TQuery">The type to which the query string parameters will be bound. Must have a parameterless constructor.</typeparam>
    /// <param name="httpContext">The HTTP context containing the query string parameters to bind.</param>
    /// <returns>An instance of type <typeparamref name="TQuery"/> populated with values from the query string.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when a property of type <typeparamref name="TQuery"/> has a type that is not supported for query string binding.
    /// </exception>
    /// <exception cref="FormatException">
    /// Thrown when a query string value cannot be converted to its target property type.
    /// </exception>
    public static TQuery Bind<TQuery>(HttpContext httpContext)
        where TQuery : new()
    {
        var query = httpContext.Request.Query;

        if (TryParse<TQuery>(query.ToDictionary(q => q.Key, q => q.Value), out var queryInstance))
        {
            return queryInstance;
        }

        throw new NotSupportedException($"The type {typeof(TQuery).Name} contains a property with an unsupported type for query string binding.");
    }

    /// <summary>
    /// Attempts to parse the specified query string parameters into an instance of the given type.
    /// </summary>
    /// <typeparam name="TQuery">The type into which the query string parameters will be parsed. Must have a parameterless constructor.</typeparam>
    /// <param name="query">A dictionary containing the query string parameters, using the property names as keys.</param>
    /// <param name="queryInstance">
    /// When this method returns, contains an instance of type <typeparamref name="TQuery"/>
    /// populated with the values from the query string if parsing was successful, or the default value if parsing failed.
    /// </param>
    /// <returns>
    /// True if the query string parameters were successfully parsed into an instance of <typeparamref name="TQuery"/>; otherwise, false.
    /// </returns>
    /// <exception cref="FormatException">
    /// Thrown when a query string value cannot be converted to its target property type. A <see langword="false"/>
    /// return value means the type itself is not bindable; a malformed <em>value</em> for an otherwise supported type
    /// is reported as an exception naming the property and the expected type.
    /// </exception>
    public static bool TryParse<TQuery>(IDictionary<string, StringValues> query, out TQuery queryInstance)
        where TQuery : new()
    {
        queryInstance = new TQuery();

        foreach (var property in typeof(TQuery).GetProperties())
        {
            var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            // Whether a type is bindable is a property of the type, not of the request, so it is
            // checked before the value is looked up. Deciding this only for properties that happen
            // to appear in the query string would let an unsupported type pass unnoticed whenever
            // the caller omitted it.
            if (!IsSupported(propertyType))
            {
                return false;
            }

            if (!query.TryGetValue(property.Name, out var queryValue) || queryValue.Count == 0)
            {
                continue;
            }

            var firstValue = queryValue[0];

            // An empty value ("?page=") is a supplied-but-blank parameter. It is a legitimate
            // empty string, but every other conversion would throw FormatException on it, so it
            // leaves non-string properties at their default instead.
            if (firstValue is null || (firstValue.Length == 0 && propertyType != typeof(string)))
            {
                continue;
            }

            property.SetValue(queryInstance, ConvertValue(firstValue, propertyType, property.Name));
        }

        return true;
    }

    private static bool IsSupported(Type propertyType) => propertyType.IsEnum || Converters.ContainsKey(propertyType);

    // Named ConvertValue rather than Convert so it does not shadow System.Convert.
    private static object ConvertValue(string value, Type propertyType, string propertyName)
    {
        object? converted;
        if (propertyType.IsEnum)
        {
            converted = Enum.TryParse(propertyType, value, out var parsedEnum) ? parsedEnum : null;
        }
        else
        {
            converted = Converters[propertyType](value);
        }

        // A malformed value still surfaces as FormatException, as it did when the conversions threw
        // directly, but now names the property and the expected type instead of only the bad input.
        return converted ?? throw new FormatException($"Query string value '{value}' for property '{propertyName}' is not a valid {propertyType.Name}.");
    }
}
