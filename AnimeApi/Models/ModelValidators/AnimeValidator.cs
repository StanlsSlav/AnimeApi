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
                    .WithMessage("Id must not be empty")
                
                .Length(24)
                    .WithMessage("Id must be 24 characters long");


            RuleFor(anime => anime.Name)
                .Must(value => value is not "")
                    .WithMessage("Name must not be empty")
                
                .MinimumLength(2)
                    .WithMessage("Name must be at least 2 characters long");


            RuleFor(anime => anime.IsFinished)
                .LessThanOrEqualTo(anime => anime.IsAiringFinished)
                    .WithMessage("Anime cannot be finished if it's still airing");
            
            
            RuleFor(anime => anime.CurrentEpisode)
                .GreaterThanOrEqualTo(0)
                    .WithMessage("Current_episode must hold a positive value");


            RuleFor(anime => anime.TotalEpisodes)
                .GreaterThanOrEqualTo(0)
                    .WithMessage("Total_episodes must hold a positive value")
                
                .GreaterThanOrEqualTo(anime => anime.CurrentEpisode)
                    .WithMessage("Total_episodes must be greater or equal to current_episode");
        }
    }
}