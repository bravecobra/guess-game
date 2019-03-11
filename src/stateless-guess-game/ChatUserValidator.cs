using FluentValidation;

namespace stateless_guess_game
{
    public class ChatUserValidator : AbstractValidator<ChatUser>
    {
        public ChatUserValidator()
        {
            RuleFor(x => x.DisplayName).NotNull().NotEmpty();
            RuleFor(x => x.Username).NotNull().NotEmpty();
        }
    }
}