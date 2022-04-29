using System;

namespace MsSqlToolBelt.DataObjects
{
    /// <summary>
    /// Represents a description attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class DescriptionValueAttribute : Attribute
    {
        /// <summary>
        /// Gets the description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the name of the value
        /// </summary>
        public string ValueText { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="DescriptionValueAttribute"/>
        /// </summary>
        /// <param name="description">The description</param>
        /// <param name="valueText">The value text</param>
        public DescriptionValueAttribute(string description, string valueText)
        {
            Description = description;
            ValueText = valueText;
        }
    }
}
