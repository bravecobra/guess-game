using FluentValidation;

namespace stateless_guess_game
{
    public class GuessGameCommandValidator: AbstractValidator<GuessGameCommand>
    {
        public GuessGameCommandValidator()
        {
            RuleFor(a => a.ArgumentsAsList).NotNull();
            RuleFor(a => a.ChatUser).NotNull();
            RuleFor(a => a.ChatUser).SetValidator(new ChatUserValidator());
        }
    }
}