using System.Collections.Generic;

namespace stateless_guess_game
{
    public class GuessGameCommand
    {
        public List<string> ArgumentsAsList { get; set; }
        public ChatUser ChatUser { get; set; }
    }
}