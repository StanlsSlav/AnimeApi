// ReSharper disable UnnecessaryWhitespace
// ReSharper disable RedundantBlankLines

using FluentValidation;

namespace AnimeApi.Models.ModelValidators
{
    /// <summary>
    ///     Validator model for <see cref="Anime"/>
    /// </summary>
    public class AnimeValidator : AbstractValidator<Anime>
    {
        /// <summary>
        ///     Indicate if null values should be allowed or not
        /// </summary>
        public bool AllowNullValues { get; set; }

        /// <summary>
        ///     Constructor for <see cref="AnimeValidator"/> that holds the validation rules
        /// </summary>
        public AnimeValidator()
        {
            if (AllowNullValues is false)
            {
                foreach (var property in typeof(Anime).GetProperties())
                {
                    RuleFor(prop => prop.GetType().GetProperty(property.Name))
                        .NotNull().WithMessage($"Property {property.Name} cannot be empty");
                }
            }

            RuleFor(anime => anime.Id)
                .Must(value => value is not "")
                    .WithMessage($"{nameof(Anime.Id)} must not be empty")
                
                .Length(24)
                    .WithMessage($"{nameof(Anime.Id)} must be 24 characters long");


            RuleFor(anime => anime.Name)
                .Must(value => value is not "")
                    .WithMessage($"{nameof(Anime.Name)} must not be empty")
                
                .MinimumLength(2)
                    .WithMessage($"{nameof(Anime.Name)} must be at least 2 characters long");


            RuleFor(anime => anime.Link)
                .Must(BeAnEmptyOrValidLink)
                    .WithMessage("The link must be similar to 'https://duckduckgo.com/' or be empty");


            RuleFor(anime => anime.IsFinished)
                .LessThanOrEqualTo(anime => anime.IsAiringFinished)
                    .WithMessage("Anime cannot be finished if it's still airing");
            
            
            RuleFor(anime => anime.CurrentEpisode)
                .GreaterThanOrEqualTo(0)
                    .WithMessage($"{nameof(Anime.CurrentEpisode)} must hold a positive value");


            RuleFor(anime => anime.TotalEpisodes)
                .GreaterThanOrEqualTo(0)
                    .WithMessage($"{nameof(Anime.TotalEpisodes)} must hold a positive value")
                
                .GreaterThanOrEqualTo(anime => anime.CurrentEpisode)
                    .WithMessage($"{nameof(Anime.TotalEpisodes)} must be greater or equal to current_episode");
        }

        private static bool BeAnEmptyOrValidLink(string link)
        {
            return string.IsNullOrWhiteSpace(link) || link.StartsWith("https://");
        }
    }
}