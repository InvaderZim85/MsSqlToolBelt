namespace MsSqlToolBelt.Common;

/// <summary>
/// Provides the functions to map two objects
/// </summary>
internal static class Mapper
{
    /// <summary>
    /// Maps the values of the properties of <paramref name="source"/> object into the properties of the <paramref name="target"/> object
    /// </summary>
    /// <typeparam name="TSource">The type of the source object</typeparam>
    /// <typeparam name="TTarget">The type of the target object</typeparam>
    /// <param name="source">The source object that provides the values for the target object</param>
    /// <param name="target">The target object, which takes over the values from the source object</param>
    /// <exception cref="ArgumentNullException">Will be thrown when the target or the source is null</exception>
    public static void Map<TSource, TTarget>(TSource source, TTarget target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        var sourceProperties = source.GetType().GetProperties();
        var targetProperties = target.GetType().GetProperties();

        foreach (var targetProperty in targetProperties)
        {
            // Determine the source property
            var sourceProperty = sourceProperties.FirstOrDefault(f =>
                f.Name.Equals(targetProperty.Name, StringComparison.OrdinalIgnoreCase));

            // If the source property is null, skip the rest
            if (sourceProperty == null)
                continue;

            // Check if the types are matching
            if (targetProperty.PropertyType == sourceProperty.PropertyType)
                targetProperty.SetValue(target, sourceProperty.GetValue(source)); // Copy the value from the source to the target
            else
            {
                // The types are not matching, check if maybe a type is nullable
                var sourceType = Nullable.GetUnderlyingType(sourceProperty.PropertyType);
                var targetType = Nullable.GetUnderlyingType(targetProperty.PropertyType);
                var sourceValue = sourceProperty.GetValue(source);

                // The source type is nullable, but has the same underlying type as the target
                if (targetType == null && sourceType != null && targetProperty.PropertyType == sourceType)
                {
                    if (sourceValue == null)
                        continue; // The value is null, so skip the rest

                    targetProperty.SetValue(target, sourceValue);
                }
                // The target type is nullable, but has the same type as the source
                else if (targetType != null && sourceType == null && targetType == sourceProperty.PropertyType)
                    targetProperty.SetValue(target, sourceValue);
                // No else, because if both types are nullable, the first if were true
            }
        }
    }
}
