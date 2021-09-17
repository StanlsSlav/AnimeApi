# アニメアプリケーションプログラミングインターフェース
### AniAPI for short
<div align="center">

  [![Target Framework Badge](https://img.shields.io/badge/dynamic/xml?color=%23512bd4&label=Target&query=%2F%2FTargetFramework%5B1%5D&url=https%3A%2F%2Fraw.githubusercontent.com%2FStanlsSlav%2FAnimeApi%2Fmaster%2FAnimeApi%2FAnimeApi.csproj&logo=.net)](https://dotnet.microsoft.com/download/dotnet/6.0)
  [![Build Badge](https://github.com/StanlsSlav/AnimeApi/actions/workflows/main.yml/badge.svg)]("https://github.com/StanlsSlav/AnimeApi/actions/workflows/main.yml/badge.svg")
  [![CodeFactor Badge](https://www.codefactor.io/repository/github/stanlsslav/animeapi/badge/main)](https://www.codefactor.io/repository/github/stanlsslav/animeapi/overview/main)

</div>

## What is it? :thinking:
AniAPI, as the name suggest, is a RESTful Aplication Programming Interface

## What does it do? :spider_web:
As of now, it allows the user to locally manage their anime watchlist via MongoDB and ASP[]().NET Core, more specifically HTTP requests

<details>
  <summary>
    Database schema can be found on <a target="_blank" href="https://localhost:5001/swagger">/swagger</a> or in file <a href="./AnimeApi/Models/Anime.cs">Anime.cs</a>
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
  DELETE /anime?id=1243
  ```

  ---

  ***Partially update an anime***
  ```http
  PATCH /anime?id=1234&field=name&value=Bleach
  ```

  <div align="center">
    :warning: The id's presented in the examples aren't valid and won't be accepted by the app :warning:
  </div>

</details>


<details>
  <summary>
    Get it up and running :gear:
  </summary>

  - Make sure you have MongoDB installed and there's an instance running
    - The app connects to a passwordless database so make sure of it


  - .Net 5! It's required, click on the *Target* badge


  - Configure your environment variables or set the appropiate flags at the last point
    - `Database` and `Collection`. They're case sensitive!


  - Clone the repo
  ```sh
  git clone https://github.com/StanlsSlav/AniAPI.git \
  cd AniAPI
  ```


  - Run the app and check on <a target="_blank" href="https://localhost:5001/">localhost:5001/</a> if it's up and running
  ```sh
  dotnet run -p ./AnimeAPI -c Release

  # Or with flags
  dotnet run -p ./AnimeAPI -c Release -- -d <DB> -c <col>
  ```

</details>

---

## P.S. Highly encouraging everybody to create their own validation rules and make daily exports of the data!

---

Seems fun? Contribute and help on making it ~~awesome~~ grander
1. Fork the project
2. Modify it
3. Open a pull request
