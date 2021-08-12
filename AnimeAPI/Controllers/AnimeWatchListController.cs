#nullable enable

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AnimeAPI.Models;
using AnimeAPI.Service;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace AnimeAPI.Controllers
{
    /// <summary>
    ///     Anime API controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class AnimeApi : ControllerBase
    {
        /// <summary>
        ///     For testing purposes
        /// </summary>
        /// <returns> An message </returns>
        [HttpGet("/")]
        [ProducesResponseType(200)]
        public IActionResult Ping()
        {
            // await Task.Delay(TimeSpan.FromMinutes(30)); 😈
            return Ok("OK");
        }

        /// <summary>
        ///     Get all or matching anime from query parameters
        /// </summary>
        /// <param name="anime"> The anime scheme to query on </param>
        /// <returns> Ok with all the anime or that matches the query </returns>
        [HttpGet("/anime")]
        [ProducesResponseType(typeof(List<Anime>), 200)]
        [ProducesResponseType(204)]
        public async Task<IActionResult> GetMatchingAnime([FromQuery] Anime anime)
        {
            List<Anime> data = await Api.GetMatchesAsync(anime);

            if (data.Any())
            {
                return Ok(data);
            }

            return NoContent();
        }

        /// <summary>
        ///     Create an anime from query parameters
        /// </summary>
        /// <param name="name"> The name of the new anime </param>
        /// <param name="currentEpisode"> Last seen episode of the new anime </param>
        /// <param name="totalEpisodes"> Total episodes count of the new anime </param>
        /// <param name="hasFinishedAiring"> If the new anime has completed airing </param>
        /// <param name="doneWatching"> If the user marked the new anime as completed </param>
        /// <returns> Ok with the anime if successful; otherwise a bad request with the encountered errors</returns>
        [HttpPost("/anime")]
        [ProducesResponseType(typeof(Anime), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> PostAnime(
            [Required] [FromQuery(Name = "name")] string name,
            [Required] [FromQuery(Name = "current_episode")] int currentEpisode,
            [Required] [FromQuery(Name = "total_episodes")] int totalEpisodes,
            [Required] [FromQuery(Name = "finished_airing")] bool hasFinishedAiring,
            [FromQuery(Name = "finished")] bool? doneWatching)
        {
            Anime animeToCreate = new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = name.Trim(),
                CurrentEpisode = currentEpisode,
                TotalEpisodes = totalEpisodes,
                IsAiringFinished = hasFinishedAiring,
                DoneWatching = doneWatching ?? currentEpisode == totalEpisodes
            };

            var (isValid, errors) = await Api.ValidateAnimeAsync(animeToCreate, false);

            if (isValid)
            {
                if (await Api.CreateAsync(animeToCreate))
                {
                    return Ok(animeToCreate);
                }

                return Problem("Couldn't create the anime");
            }

            return BadRequest(errors);
        }

        /// <summary>
        ///     Delete an anime from id query
        /// </summary>
        /// <param name="id"> The anime id to delete </param>
        /// <returns> Ok if successful </returns>
        [HttpDelete("/anime")]
        [ProducesResponseType(typeof(Anime), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteAnime(
            [Required] [FromQuery(Name = "id")] string id)
        {
            var (isSuccess, anime) = await Api.DeleteWithIdAsync(id);

            if (isSuccess)
            {
                return Ok(anime);
            }

            return NotFound();
        }

        /// <summary>
        ///     Update a field with a new value with the specified id
        /// </summary>
        /// <param name="id"> The anime id to find </param>
        /// <param name="field"> The field to update </param>
        /// <param name="newValue"> The new value that will replace the older one </param>
        /// <returns> Ok with the newly updated anime </returns>
        [HttpPatch("/anime")]
        [ProducesResponseType(typeof(Anime), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> PartialUpdate(
            [Required] [FromQuery(Name = "id")] string id,
            [Required] [FromQuery(Name = "field")] string field,
            [Required] [FromQuery(Name = "value")] string newValue)
        {
            var (isSuccess, anime) = await Api.PartialUpdateAsync(id, field, newValue);

            if (isSuccess)
            {
                return Ok(anime);
            }

            return BadRequest();
        }

        /// <summary>
        ///     Same as <see cref="PartialUpdate" />, but allows various fields to get updated
        /// </summary>
        /// <remarks> Awesome! </remarks>
        /// <param name="id"> The anime id to find </param>
        /// <param name="newAnime"> The anime object to update against </param>
        /// <returns> Ok with the newly updated anime </returns>
        [HttpPut("/anime")]
        [ProducesResponseType(typeof(Anime), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> FullUpdate(
            [Required] [FromQuery(Name = "id")] string id,
            [FromQuery] Anime newAnime)
        {
            var (isValid, errors) = await Api.ValidateAnimeAsync(newAnime, true);

            if (isValid)
            {
                if (await Api.UpdateAsync(id, newAnime))
                {
                    return Ok(await Api.GetMatchesAsync(new Anime() { Id = id }));
                }

                return BadRequest("🦆");
            }

            return BadRequest(errors);
        }
    }
}