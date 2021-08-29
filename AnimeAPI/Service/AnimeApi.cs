#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnimeAPI.HelperMethods;
using AnimeAPI.Models;
using AnimeAPI.Models.ModelValidators;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AnimeAPI.Service
{
    /// <summary>
    ///     Holds the CRUD methods that operate on the <a href="https://www.mongodb.com/">MongoDB</a>
    /// </summary>
    public static class Api
    {
        private static readonly string Database = Environment.GetEnvironmentVariable("Database")!;
        private static readonly string Collection = Environment.GetEnvironmentVariable("Collection")!;

        private static readonly MongoClient MongoClient = new("mongodb://localhost:27017");
        private static readonly IMongoCollection<Anime> Db = MongoClient.GetDatabase(Database)
            .GetCollection<Anime>(Collection);

        private static readonly AnimeValidator Validator = new();

        /// <summary>
        ///     Executes the fluent validation for parameter <paramref name="toValidate"/>
        /// </summary>
        /// <param name="toValidate"> The anime to fluent validate </param>
        /// <param name="allowNullValues"> Indicate if null values are allowed </param>
        /// <returns> The status of the operation and the occurred failures, if any </returns>
        public static async Task<(bool isSuccess, string failure)> ValidateAnimeAsync(
            Anime toValidate,
            bool allowNullValues)
        {
            Validator.AllowNullValues = allowNullValues;
            var validationResult = await Validator.ValidateAsync(toValidate);

            List<string> failures = new();

            foreach (var resultError in validationResult.Errors)
            {
                failures.Add(resultError.ErrorMessage);
            }

            return (validationResult.IsValid, failures.ToJson());
        }

        /// <summary>
        ///     Dump matching anime
        /// </summary>
        /// <param name="animeToMatch"> The anime "blueprint" to find </param>
        /// <param name="isExactMatch"> Indicate if the search should return animes with exact names, casing excluded </param>
        /// <returns> All the animes that match the blueprint </returns>
        public static async Task<List<Anime>> GetMatchesAsync(
            Anime animeToMatch,
            bool isExactMatch = false)
        {
            IAsyncCursor<Anime> results = await Db.FindAsync(_ => true);
            List<Anime> data = results.ToList();
            var allNullValues = true;

            // Check whenever user wants all the animes or the matches
            foreach (var property in animeToMatch.GetType().GetProperties())
            {
                if (property.GetValue(animeToMatch) is not null)
                {
                    allNullValues = false;
                }
            }

            if (allNullValues)
            {
                return data;
            }

            if (isExactMatch)
            {
                return data
                    .Where(x =>
                        x.Id == animeToMatch.Id ||
                        x.Name!.Equals(animeToMatch.Name, StringComparison.InvariantCultureIgnoreCase) ||
                        x.DoneWatching == animeToMatch.DoneWatching ||
                        x.IsAiringFinished == animeToMatch.IsAiringFinished ||
                        x.CurrentEpisode == animeToMatch.CurrentEpisode ||
                        x.TotalEpisodes == animeToMatch.TotalEpisodes)
                    .ToList();
            }

            // Must return an list of animes, other methods rely on this
            return data
                .Where(x =>
                    x.Id == animeToMatch.Id ||

                    // So it doesn't search for an existing name
                    x.Name!.Contains(animeToMatch.Name ?? "abc123def", StringComparison.InvariantCultureIgnoreCase) ||
                    x.DoneWatching == animeToMatch.DoneWatching ||
                    x.IsAiringFinished == animeToMatch.IsAiringFinished ||
                    x.CurrentEpisode == animeToMatch.CurrentEpisode ||
                    x.TotalEpisodes == animeToMatch.TotalEpisodes)
                .ToList();
        }

        /// <summary>
        ///     Create an anime object in the db
        /// </summary>
        /// <param name="anime"> The anime to create </param>
        /// <returns> The action taken by the server </returns>
        public static async Task<bool> CreateAsync(Anime anime)
        {
            // Avoid name conflicts and user's confusion
            if (await IsDuplicateAsync(anime.Name!))
            {
                return false;
            }

            await Db.InsertOneAsync(anime);
            return true;
        }

        /// <summary>
        ///     Delete an anime object from the database
        /// </summary>
        /// <param name="id"> The anime id to find and delete </param>
        /// <returns> True if the anime was deleted </returns>
        public static async Task<(bool isSuccess, Anime? anime)> DeleteWithIdAsync(string id)
        {
            Anime toDelete = new() { Id = id };
            List<Anime> data = await GetMatchesAsync(toDelete);

            if (data.Any() is false)
            {
                return (false, null);
            }

            await Db.DeleteOneAsync(x => x.Id == id);
            return (true, data.First());
        }

        /// <summary>
        ///     Partially update an anime from db
        /// </summary>
        /// <param name="animeId"> The anime id to find and update </param>
        /// <param name="propertyToUpdate"> The field to apply the update </param>
        /// <param name="newValue"> The value that is going to go there </param>
        /// <returns> If method succeeded true and the updated anime; otherwise false and null for anime </returns>
        public static async Task<(bool isSuccess, Anime? anime)> PartialUpdateAsync(
            string animeId,
            string propertyToUpdate,
            string newValue)
        {
            var animeToUpdate = (await GetMatchesAsync(new Anime() { Id = animeId }))[0];

            if (ApiParsing.TryParseName(propertyToUpdate, out var validProperty) &&
                ApiParsing.TryParseTypeValue(validProperty!, newValue, out var validValue))
            {
                // Don't let current episode be > than total episodes
                var isCurrentEpHigher =
                    validProperty!.Equals(nameof(Anime.CurrentEpisode)) &&
                    (int)validValue! > animeToUpdate.TotalEpisodes;

                // Don't let total episodes be < than current episode
                var isTotalEpsLower =
                    validProperty.Contains("total") && (int)validValue! < animeToUpdate.CurrentEpisode;

                // Don't let anime name duplicates
                var isNameDuplicate =
                    validProperty.Equals(nameof(Anime.Name), StringComparison.InvariantCultureIgnoreCase) &&
                    await IsDuplicateAsync(newValue);

                if (isCurrentEpHigher ||
                    isTotalEpsLower ||
                    isNameDuplicate)
                {
                    return (false, null);
                }

                await Db.FindOneAndUpdateAsync(x => x.Id == animeId,
                    Builders<Anime>.Update.Set(validProperty, validValue));

                List<Anime> data = await GetMatchesAsync(new Anime() { Id = animeId });
                Anime anime = data[0];

                return (true, anime);
            }

            return (false, null);
        }

        /// <summary>
        ///     Fully updates an anime from the db, might be slower than <see cref="PartialUpdateAsync" />
        /// </summary>
        /// <param name="animeId"> The anime id to find and update </param>
        /// <param name="newAnimeValues"> </param>
        /// <returns> If method succeeded true and the updated anime; otherwise false and null for anime </returns>
        public static async Task<bool> UpdateAsync(
            string animeId,
            Anime newAnimeValues)
        {
            foreach (var property in newAnimeValues.GetType().GetProperties())
            {
                // Will add a new property to object if id gets updated
                // And the user should never mess with it, the GUIDs will do
                if (property.Name.ToLower() is "id" ||
                    property.GetValue(newAnimeValues) is null)
                {
                    continue;
                }

                if ((await PartialUpdateAsync(animeId, property.Name.ToLower(),
                    property.GetValue(newAnimeValues)!.ToString()!)).isSuccess is false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Checks if <paramref name="animeName"/> is already present in the db
        /// </summary>
        /// <param name="animeName"> The name to look out for </param>
        /// <returns> True if the search returns any object; otherwise false </returns>
        private static async Task<bool> IsDuplicateAsync(string animeName)
        {
            List<Anime> matches = await GetMatchesAsync(new Anime() { Name = animeName }, true);
            return matches.Any();
        }
    }
}