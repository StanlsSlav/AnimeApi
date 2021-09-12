using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnimeApi.Models;
using AnimeApi.Service;
using MongoDB.Bson;
using NUnit.Framework;

namespace AnimeApi.UnitTests
{
    [TestFixture]
    public static class Tests
    {
        private static readonly Anime TestAnime = new()
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = "TestAnime",
            CurrentEpisode = 0,
            TotalEpisodes = 2,
            IsAiringFinished = true,
            IsFinished = true
        };

        private static string Database = "MAL";
        private static string Collection = "AnimesTest";

        [OneTimeSetUp]
        public static void OneTimeSetUp()
        {
            // Important!
            Environment.SetEnvironmentVariable("Database", Database);
            Environment.SetEnvironmentVariable("Collection", Collection);
        }

        [TearDown]
        public static async Task TearDown()
        {
            try
            {
                _ = await Api.DeleteWithIdAsync(TestAnime.Id!);
            }
            catch
            {
                // ignored
            }
        }

        [SetUp]
        public static async Task SetUp()
        {
            try
            {
                Assert.IsTrue(await Api.CreateAsync(TestAnime));
            }
            catch (Exception)
            {
                Assert.Fail($"Delete the id: {TestAnime.Id}");
            }
        }

        [Test]
        public static async Task GetMatches_Returns_All_Data()
        {
            List<Anime> data = await Api.GetMatchesAsync(new Anime());
            Assert.Greater(data.Count, 0);
        }

        [Test]
        public static async Task GetMatches_Returns_Custom_Data()
        {
            Anime modelAnime = new()
            {
                Name = "Test",
                CurrentEpisode = 0,
                TotalEpisodes = 12,
                IsFinished = true,
                IsAiringFinished = true
            };

            List<Anime> data = await Api.GetMatchesAsync(modelAnime);

            Assert.Greater(data.Count, 0);
        }

        [Test]
        public static async Task GetMatches_Returns_Single_Data_With_Id()
        {
            List<Anime> data = await Api.GetMatchesAsync(new Anime());
            var id = data.First().Id;

            Assert.AreEqual(1, data.Count(x => x.Id == id));
        }

        [Test]
        public static async Task Post_Allows_Only_Unique_Anime_Names()
        {
            for (var i = 0; i <= 2; i++)
            {
                _ = await Api.CreateAsync(TestAnime);
            }

            List<Anime> testAnimes = await Api.GetMatchesAsync(new Anime() { Name = TestAnime.Name });
            Assert.AreEqual(1, testAnimes.Count);
        }

        [Test]
        public static async Task Delete_With_Id()
        {
            Anime animeToHandle = new()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "Delete with Id UnitTest",
                CurrentEpisode = 0,
                TotalEpisodes = 1,
                IsFinished = false,
                IsAiringFinished = false
            };

            _ = await Api.CreateAsync(animeToHandle);

            Assert.IsTrue((await Api.DeleteWithIdAsync(animeToHandle.Id!)).isSuccess);
        }

        [Test]
        public static async Task Patch_With_Valid_String_Value()
        {
            const string fieldToUpdate = nameof(Anime.Name), newName = "!!";

            if ((await Api.PartialUpdateAsync(TestAnime.Id!, fieldToUpdate, newName)).isSuccess)
            {
                List<Anime> getNewAnime = await Api.GetMatchesAsync(new Anime()
                {
                    Id = TestAnime.Id,
                    Name = newName
                });

                if (getNewAnime.Count is not 1)
                {
                    Assert.Fail($"Anime not found or '{fieldToUpdate}' not changed");
                }

                Assert.Pass();
            }

            Assert.Fail("Couldn't update");
        }

        [Test]
        public static async Task Patch_With_Valid_Int_Value()
        {
            const string fieldToUpdate = nameof(Anime.TotalEpisodes), value = "999";
            var correctValue = int.Parse(value);

            if ((await Api.PartialUpdateAsync(TestAnime.Id!, fieldToUpdate, value)).isSuccess)
            {
                List<Anime> getUpdatedAnime = await Api.GetMatchesAsync(new Anime
                {
                    Id = TestAnime.Id,
                    TotalEpisodes = correctValue
                });

                if (getUpdatedAnime.Count is not 1)
                {
                    Assert.Fail($"Anime not found or '{fieldToUpdate}' not changed");
                }

                Assert.Pass();
            }

            Assert.Fail("Couldn't update");
        }

        [Test]
        public static async Task Patch_With_Bool_Value()
        {
            const string fieldToUpdate = nameof(Anime.IsAiringFinished), value = "true";
            var correctValue = bool.Parse(value);

            if ((await Api.PartialUpdateAsync(TestAnime.Id!, fieldToUpdate, value)).isSuccess)
            {
                List<Anime> getNewAnime = await Api.GetMatchesAsync(new Anime()
                {
                    Id = TestAnime.Id!,
                    IsAiringFinished = correctValue
                });

                if (getNewAnime.Count is 0 ||
                    getNewAnime[0].IsAiringFinished != correctValue)
                {
                    Assert.Fail($"Anime not found or '{fieldToUpdate}' not changed");
                }

                Assert.Pass();
            }

            Assert.Fail("Couldn't update");
        }

        [Test]
        public static async Task Full_Update()
        {
            Anime newAnimeValues = new()
            {
                IsFinished = false,
                TotalEpisodes = 100
            };

            Assert.IsTrue(await Api.UpdateAsync(TestAnime.Id!, newAnimeValues), $"Failed on Id: {TestAnime.Id}");
        }

        [Test]
        public static async Task Partial_Update_With_Wrong_Value_Type()
        {
            var validFieldToUpdate = nameof(Anime.TotalEpisodes);
            var invalidValue = "If this appears in the db, then something's terribly off";

            try
            {
                _ = await Api.PartialUpdateAsync(TestAnime.Id!, validFieldToUpdate, invalidValue);
            }
            catch (Exception e)
            {
                Assert.Fail($"An exception was thrown: {e.Message}");
            }

            Assert.Pass();
        }

        [Test]
        public static async Task Partial_Update_With_Wrong_Field_And_Value()
        {
            var invalidFieldToUpdate = "I'm on the highway to hell!";
            var invalidValue = @"It fails anyway ◑﹏◐";

            try
            {
                _ = await Api.PartialUpdateAsync(TestAnime.Id!, invalidFieldToUpdate, invalidValue);
            }
            catch (Exception e)
            {
                Assert.Fail($"An exception was thrown: {e.Message}");
            }

            Assert.Pass();
        }
    }
}