#nullable enable

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AnimeApi.Models;
using AnimeApi.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace AnimeApi.Controllers
{
    /// <summary>
    ///     Anime API controller
    /// </summary>
    [Produces("application/json", "text/plain")]
    [ApiController]
    [Route("[controller]")]
    public class AnimeApi : ControllerBase
    {
        /// <summary>
        ///     Get all or matching anime
        /// </summary>
        /// <param name="anime"> The anime scheme to query on </param>
        /// <returns> Ok with all the anime or that matches the query </returns>
        /// <response code="200"> Returns the matching animes or all </response>
        /// <response code="204"> Returns when no matching anime was found </response>
        [HttpGet("/anime")]
        [ProducesResponseType(typeof(List<Anime>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
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
        ///     Redirect the user to the anime website
        /// </summary>
        /// <param name="anime"> The anime scheme to query on </param>
        /// <returns> Redirects to the link of the first anime found </returns>
        /// <response code="204"> Returns when no matching anime was found </response>
        /// <response code="302">
        ///     Returns a redirect to the link of the anime,
        ///     replacing any "%ep" within the link with the current episode
        /// </response>
        [HttpGet("/redirect")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public async Task<IActionResult> GetRedirectToAnime([Required] [FromQuery] Anime anime)
        {
            List<Anime> data = await Api.GetMatchesAsync(anime);

            if (!data.Any())
            {
                return NoContent();
            }

            var redirectAnime = data[0];

            if (redirectAnime.Link is null)
            {
                return NoContent();
            }

            if (redirectAnime.Link.Contains("%ep") is false)
            {
                return Redirect(redirectAnime.Link);
            }

            return Redirect(redirectAnime.Link.Replace("%ep",
                redirectAnime.CurrentEpisode.ToString()));
        }

        /// <summary>
        ///     Create an anime
        /// </summary>
        /// <param name="anime">The anime to create</param>
        /// <returns> Ok with the anime if successful; otherwise a bad request with the encountered errors</returns>
        /// <response code="200"> Returns the newly created anime </response>
        /// <response code="400"> Returns the possible errors caused by the request with the occured errors </response>
        /// <response code="500"> Returns when an internal error occured </response>
        [HttpPost("/anime")]
        [ProducesResponseType(typeof(Anime), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostAnime(
            [Required] [FromQuery] Anime anime)
        {
            Anime animeToCreate = new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = anime.Name?.Trim(),
                Link = anime.Link,
                CurrentEpisode = anime.CurrentEpisode,
                TotalEpisodes = anime.TotalEpisodes,
                IsAiringFinished = anime.IsAiringFinished,
                IsFinished = anime.IsFinished
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
        ///     Delete an anime
        /// </summary>
        /// <param name="id"> The anime id to delete </param>
        /// <returns> Ok if successful </returns>
        /// <response code="200"> Returns the anime that was found and deleted </response>
        /// <response code="404"> Returns when id was not found </response>
        [HttpDelete("/anime")]
        [ProducesResponseType(typeof(Anime), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        ///     Update a field with a new value
        /// </summary>
        /// <param name="id"> The anime id to find </param>
        /// <param name="field"> The field to update </param>
        /// <param name="newValue"> The new value that will replace the older one </param>
        /// <returns> Ok with the newly updated anime </returns>
        /// <response code="200"> Returns the newly updated anime </response>
        /// <response code="400"> Returns when the request couldn't be processed </response>
        [HttpPatch("/anime")]
        [ProducesResponseType(typeof(Anime), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
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
        ///     Fully updates an anime
        /// </summary>
        /// <param name="id"> The anime id to find </param>
        /// <param name="newAnime"> The anime object to update against </param>
        /// <returns> Ok with the newly updated anime </returns>
        /// <response code="200"> Returns the newly updated anime </response>
        /// <response code="400"> Returns when the request couldn't be processed with the occured errors </response>
        [HttpPut("/anime")]
        [ProducesResponseType(typeof(Anime), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> FullUpdate(
            [Required] [FromQuery(Name = "id")] string id,
            [FromQuery] Anime newAnime)
        {
            var (isValid, errors) = await Api.ValidateAnimeAsync(newAnime, true);

            if (isValid)
            {
                if (await Api.UpdateAsync(id, newAnime))
                {
                    return Ok(await Api.GetMatchesAsync(new Anime { Id = id }));
                }

                return BadRequest("🦆");
            }

            return BadRequest(errors);
        }
    }
}