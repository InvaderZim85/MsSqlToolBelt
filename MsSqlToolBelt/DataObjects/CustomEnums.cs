using System;
using System.Collections.Generic;
using ZimLabs.CoreLib.Extensions;

namespace MsSqlToolBelt.DataObjects
{
    /// <summary>
    /// Provides several enums
    /// </summary>
    internal static class CustomEnums
    {
        /// <summary>
        /// Provides the different filter types
        /// </summary>
        public enum FilterType
        {

            /// <summary>
            /// The text contains the desired value
            /// </summary>
            [DescriptionValue("The text contains the desired value", "Contains")]
            Contains,

            /// <summary>
            /// The text starts with the desired value
            /// </summary>
            [DescriptionValue("The text starts with the desired value", "Starts with")]
            StartsWith,

            /// <summary>
            /// The text ends with the desired value
            /// </summary>
            [DescriptionValue("The texts ends with the desired value", "Ends with")]
            EndsWith,

            /// <summary>
            /// The text and the desired value are equals
            /// </summary>
            [DescriptionValue("The text and the desired value are equal", "Equals")]
            Equals
        }

        /// <summary>
        /// Gets the list with the enums as <see cref="TextValueItem"/>
        /// </summary>
        /// <returns>The list with the types</returns>
        public static List<TextValueItem> GetFilterTypeList()
        {
            var result = new List<TextValueItem>();

            foreach (FilterType value in Enum.GetValues(typeof(FilterType)))
            {
                var attribute = value.GetAttribute<DescriptionValueAttribute>();

                if (attribute == null)
                    result.Add(new TextValueItem((int)value, value.ToString()));
                else
                    result.Add(new TextValueItem((int)value, attribute.ValueText)
                    {
                        AdditionalText = attribute.Description
                    });
            }

            return result;
        }
    }
}
