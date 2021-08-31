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
            DoneWatching = true
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

        [SetUp]
        public static async Task SetUp()
        {
            // Would result in errors about the test anime's Id otherwise
            try
            {
                Assert.IsTrue(await Api.CreateAsync(TestAnime));
            }
            catch (Exception)
            {
                Assert.Fail($"Check id: {TestAnime.Id}");
            }
        }

        [TearDown]
        public static async Task Teardown()
        {
            // Always delete the test anime after using it
            // So SetUp() wouldn't result in a fail
            try
            {
                _ = await Api.DeleteWithIdAsync(TestAnime.Id!);
            }
            catch (Exception)
            {
                Assert.Fail($"Couldn't delete id: {TestAnime.Id}");
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
                DoneWatching = true,
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
                DoneWatching = false,
                IsAiringFinished = false
            };

            _ = await Api.CreateAsync(animeToHandle);

            Assert.IsTrue((await Api.DeleteWithIdAsync(animeToHandle.Id!)).isSuccess);
        }

        [Test]
        public static async Task Patch_With_Valid_String_Value()
        {
            const string fieldToUpdate = "name", newName = "!!";

            if ((await Api.PartialUpdateAsync(TestAnime.Id!, fieldToUpdate, newName)).isSuccess)
            {
                List<Anime> getNewAnime = await Api.GetMatchesAsync(new Anime()
                {
                    Id = TestAnime.Id,
                    Name = newName
                });

                if (getNewAnime.Count is 0 ||
                    getNewAnime[0].Name is not newName)
                {
                    Assert.Fail("Anime not found or 'name' not changed");
                }

                Assert.Pass();
            }

            Assert.Fail("Couldn't update");
        }

        [Test]
        public static async Task Patch_With_Valid_Int_Value()
        {
            const string fieldToUpdate = "total_episodes", value = "999";
            var correctValue = int.Parse(value);

            if ((await Api.PartialUpdateAsync(TestAnime.Id!, fieldToUpdate, value)).isSuccess)
            {
                List<Anime> getNewAnime = await Api.GetMatchesAsync(new Anime()
                {
                    Id = TestAnime.Id,
                    TotalEpisodes = correctValue
                });

                if (getNewAnime.Count is 0 ||
                    getNewAnime[0].TotalEpisodes != correctValue)
                {
                    Assert.Fail("Anime not found or 'total_episodes' not changed");
                }

                Assert.Pass();
            }

            Assert.Fail("Couldn't update");
        }

        [Test]
        public static async Task Patch_With_Bool_Value()
        {
            const string fieldToUpdate = "finished", value = "true";
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
                    Assert.Fail("Anime not found or 'finished_airing' not changed");
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
                CurrentEpisode = 69,
                TotalEpisodes = 100
            };

            Assert.IsTrue(await Api.UpdateAsync(TestAnime.Id!, newAnimeValues), $"Failed on Id: {TestAnime.Id}");
        }

        [Test]
        public static async Task Partial_Update_With_Wrong_Value_Type()
        {
            var validFieldToUpdate = "total_episodes";
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