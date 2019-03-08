namespace stateless_guess_game.Quiltoni.PixelBot.Commands
{
    public interface IChatService
    {
        void BroadcastMessageOnChannel(string p0);
        void WhisperMessage(object username, string s);
    }
}