using System;
using System.Collections.Generic;

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
        /// Provides the different save / copy types
        /// </summary>
        public enum SaveType
        {
            /// <summary>
            /// Saves / copies the data as markdown
            /// </summary>
            Markdown,

            /// <summary>
            /// As a comma separated file 
            /// </summary>
            Csv,

            /// <summary>
            /// As a ASCII art table
            /// </summary>
            Default
        }

        /// <summary>
        /// Gets the list with the <see cref="FilterType"/> enums as <see cref="TextValueItem"/>
        /// </summary>
        /// <returns>The list with the types</returns>
        public static List<TextValueItem> GetFilterTypeList()
        {
            return GetValueList<FilterType>();
        }

        /// <summary>
        /// Gets the list with the <see cref="SaveType"/> enums as <see cref="TextValueItem"/>
        /// </summary>
        /// <param name="textPrefix">The prefix of the text</param>
        /// <returns>The list with the types</returns>
        public static List<TextValueItem> GetSaveTypeList(string textPrefix)
        {
            return GetValueList<SaveType>(textPrefix);
        }

        /// <summary>
        /// Gets the desired enum as a list
        /// </summary>
        /// <typeparam name="T">The type of the enum</typeparam>
        /// <param name="textPrefix">The prefix of the text (optional)</param>
        /// <returns>The list with the values</returns>
        public static List<TextValueItem> GetValueList<T>(string textPrefix = "") where T : Enum
        {
            var result = new List<TextValueItem>();

            foreach (var value in Enum.GetValues(typeof(T)))
            {
                var attribute = value.GetAttribute<DescriptionValueAttribute>();

                if (attribute == null)
                {
                    result.Add(new TextValueItem((int)value,
                        string.IsNullOrEmpty(textPrefix) ? value.ToString() : $"{textPrefix} {value}"));
                }
                else
                {
                    result.Add(new TextValueItem((int)value, attribute.ValueText)
                    {
                        AdditionalText = attribute.Description
                    });
                }
            }

            return result;
        }
    }
}
