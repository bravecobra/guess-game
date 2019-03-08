namespace stateless_guess_game.Quiltoni.PixelBot.Commands
{
    public class ChatMessage
    {
        public bool IsBroadcaster { get; set; }
        public bool IsModerator { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
    }
}