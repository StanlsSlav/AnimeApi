#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace AnimeApi.HelperMethods
{
    /// <summary>
    ///     Holds the custom parsing methods for the anime api
    /// </summary>
    public static class ApiParsing
    {
        // Fuck it
        private static readonly Dictionary<string, string> ModelToProperty
            = new(StringComparer.InvariantCultureIgnoreCase)
            {
                ["Id"] = "_id",
                ["Name"] = "name",
                ["DoneWatching"] = "finished",
                ["IsAiringFinished"] = "finished_airing",
                ["CurrentEpisode"] = "current_episode",
                ["TotalEpisodes"] = "total_episodes"
            };

        // And this as well
        private static readonly Dictionary<string, object> PropertyToType
            = new(StringComparer.InvariantCultureIgnoreCase)
            {
                ["id"] = typeof(string),
                ["name"] = typeof(string),
                ["finished"] = typeof(bool),
                ["finished_airing"] = typeof(bool),
                ["current_episode"] = typeof(int),
                ["total_episodes"] = typeof(int)
            };

        /// <summary>
        ///     Try to parse an property name to the correct db one
        /// </summary>
        /// <param name="propertyName"> The name to check against </param>
        /// <param name="validName"> When this method returns true, contains an valid property from the db </param>
        /// <returns> True if <paramref name="propertyName" /> was parsed successfully; otherwise, false </returns>
        public static bool TryParseName(string propertyName, out string? validName)
        {
            validName = null;

            var containsKey = ModelToProperty.ContainsKey(propertyName);
            var containsValue = ModelToProperty.ContainsValue(propertyName.ToLower());

            if (containsKey)
            {
                validName = ModelToProperty[propertyName];
                return true;
            }
            else if (containsValue)
            {
                validName = ModelToProperty.Values.First(x => x == propertyName.ToLower());
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Try to parse an property name to the data type from db
        ///     <para>Should always be used with <see cref="TryParseName"/></para>
        /// </summary>
        /// <param name="propertyName"> The name to check against </param>
        /// <param name="value"> The value from input </param>
        /// <param name="validValue"> When this method returns true, contains the value converted to its correct type </param>
        /// <returns> True if <paramref name="value" /> was converted to the correct type of the property from db; otherwise false </returns>
        public static bool TryParseTypeValue(string propertyName, string value, out object? validValue)
        {
            validValue = null;

            if (PropertyToType.ContainsKey(propertyName))
            {
                var validType = PropertyToType[propertyName] as Type;

                validValue = validType!.Name switch
                {
                    "String" => value,
                    "Int32" => int.TryParse(value, out var validated) ? validated : null,
                    "Boolean" => bool.TryParse(value, out var validated) ? validated : null,
                    _ => null
                };

                // Don't allow negative values
                if (validType.Name is "Int32" && validValue is < 0)
                {
                    validValue = null;
                    return false;
                }
            }

            return validValue is not null;
        }
    }
}