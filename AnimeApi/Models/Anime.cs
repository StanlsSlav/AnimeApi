#nullable enable

using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AnimeApi.Models
{
    /// <summary>
    ///     Model of anime object for db
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Anime
    {
        /// <summary>
        ///     Representation of an <a href="https://docs.mongodb.com/manual/reference/method/ObjectId/"> object id </a>
        /// </summary>
        [BsonElement("_id")]
        [FromQuery(Name = "id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; init; }

        /// <summary>
        ///     Name or abbreviation of an anime
        /// </summary>
        [FromQuery(Name = "name")]
        public string? Name { get; init; }

        /// <summary>
        ///     Indicator of the completion status the anime (finished / unfinished)
        /// </summary>
        [FromQuery(Name = "is_finished")]
        public bool? IsFinished { get; init; }

        /// <summary>
        ///     Indicator of the airing status of the anime (airing / aired)
        /// </summary>
        [FromQuery(Name = "is_airing_finished")]
        public bool? IsAiringFinished { get; init; }

        /// <summary>
        ///     Tracks the latest episode the user watched
        /// </summary>
        [BsonRepresentation(BsonType.Int32, AllowTruncation = true)]
        [FromQuery(Name = "current_episode")]
        public int? CurrentEpisode { get; init; }

        /// <summary>
        ///     Tracks the total amount of episodes in the anime
        /// </summary>
        [BsonRepresentation(BsonType.Int32, AllowTruncation = true)]
        [FromQuery(Name = "total_episodes")]
        public int? TotalEpisodes { get; init; }
    }
}