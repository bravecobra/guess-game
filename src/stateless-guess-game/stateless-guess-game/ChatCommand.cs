using System.Collections.Generic;

namespace stateless_guess_game.Quiltoni.PixelBot.Commands
{
    public class ChatCommand
    {
        public List<string> ArgumentsAsList { get; set; }
        public ChatMessage ChatMessage { get; set; }
    }
}