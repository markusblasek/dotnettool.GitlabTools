using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using CommandLine;

namespace GitLabTools;

public static class ExpressionUtils
{
    public static string GetCommandlineArgumentLongName<TValue>(Expression<Func<TValue>> exp)
    {
        return GetOptionAttribute(exp).LongName;
    }

    public static OptionAttribute GetOptionAttribute<TValue>(Expression<Func<TValue>> exp)
    {
        return GetAttribute<OptionAttribute, TValue>(exp);
    }

    public static string GetJsonPropertyName<TValue>(Expression<Func<TValue>> exp)
    {
        return GetJsonPropertyNameAttribute(exp).Name;
    }

    public static JsonPropertyNameAttribute GetJsonPropertyNameAttribute<TValue>(Expression<Func<TValue>> exp)
    {
        return GetAttribute<JsonPropertyNameAttribute, TValue>(exp);
    }

    public static T GetAttribute<T, TValue>(Expression<Func<TValue>> exp) where T : Attribute
    {
        if (exp.Body is not MemberExpression body)
        {
            throw new ArgumentException($"Attribute {nameof(exp.Body)} is null or is not of type '{typeof(MemberExpression)}'");
        }

        return body.Member.GetCustomAttribute<T>()
               ?? throw new ArgumentException($"Submitted property has not attribute of type {typeof(T)}");
    }
}
