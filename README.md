# アニメアプリケーションプログラミングインターフェース
### AniAPI for short
<div align="center">

  [![Target Framework Badge](https://img.shields.io/badge/dynamic/xml?color=%23512bd4&label=Target&query=%2F%2FTargetFramework%5B1%5D&url=https%3A%2F%2Fraw.githubusercontent.com%2FStanlsSlav%2FAniAPI%2Fmaster%2FAnimeAPI%2AnimeAPI.csproj&logo=.net)](https://dotnet.microsoft.com/download/dotnet/5.0)
  [![Build Badge](https://github.com/StanlsSlav/AniAPI/actions/workflows/main.yml/badge.svg)]("https://github.com/StanlsSlav/AniAPI/actions/workflows/main.yml/badge.svg")
  [![CodeFactor Badge](https://www.codefactor.io/repository/github/stanlsslav/aniapi/badge/main)](https://www.codefactor.io/repository/github/stanlsslav/aniapi/overview/main)

</div>

## What is it? :thinking:
AniAPI, as the name suggest, is a RESTful Aplication Programming Interface

## What does it do? :spider_web:
As of now, it allows the user to locally manage their anime watchlist via MongoDB and ASP[]().NET Core, more specifically HTTP requests

<details>
  <summary>
    Database schema can be found on <a href="https://localhost:5001/swagger">/swagger</a> or in file <a href="./AnimeAPI/Models/Anime.cs">Anime.cs</a>
  </summary>

  ```cs
  class Anime
  {
    [BsonElement("_id")]
    string Id

    [BsonElement("name")]
    string Name

    [BsonElement("finished")]
    bool DoneWatching

    [BsonElement("finished_airing")]
    bool IsAiringFinished

    [BsonElement("current_episode")]
    int CurrentEpisode

    [BsonElement("total_episodes")]
    int TotalEpisodes
  }
  ```

</details>

<details>
  <summary>
    Usage examples :test_tube:
  </summary>

  ***Get all animes***
  ```http
  GET /anime
  ```

  ---

  ***Delete a specific one***
  ```http
  DEL /anime?id=1243
  ```

  ---

  ***Partially update an anime***
  ```http
  PATCH /anime?id=1234&field=name&value=Bleach
  ```

  <div align="center" style="color: #f0f022; font-size: large;">
    :warning: The id's presented in the examples aren't valid and won't be accepted by the app :warning:
  </div>

</details>


<details>
  <summary>
    Get it up and running :gear:
  </summary>

  1. Make sure you have MongoDB installed and there's an instance running

  1.1 The app connects to a passwordless database so make sure of it

  2. .Net 5! It's required, click on the *Target* badge

  3. Configure your environment variables

  3.1 `Database` and `Collection`

  4. Clone the repo
  ```sh
  git clone https://github.com/StanlsSlav/AniAPI.git \
  cd AniAPI
  ```

  5. Run the app and check on <a href="https://localhost:5001/">localhost:5001/</a> if it's up and running
  ```sh
  dotnet build \
  dotnet run -c Release
  ```

</details>

---

Seems fun? Contribute and help on making it ~~awesome~~ grander
1. Fork the project
2. Modify it
3. Open a pull request
