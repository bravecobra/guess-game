namespace stateless_guess_game
{
    public interface IChatService
    {
        void BroadcastMessageOnChannel(string message);
        void WhisperMessage(string username, string message);
    }
}