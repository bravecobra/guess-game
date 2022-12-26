using stateless_guess_game;
using Xunit.Abstractions;

namespace stateless_guess_game_tests
{
    class StubChat: IChatService
    {
        private readonly ITestOutputHelper _output;

        public StubChat(ITestOutputHelper output)
        {
            _output = output;
        }
        public void BroadcastMessageOnChannel(string message)
        {
            _output.WriteLine(message);
        }

        public void WhisperMessage(string username, string message)
        {
            _output.WriteLine($"{username}: {message}");
        }
    }
}