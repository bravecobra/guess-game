using stateless_guess_game;
using Xunit;
using Xunit.Abstractions;

namespace stateless_guess_game_tests.GuessGameTests
{
    public class GuessGameDiagramShould
    {
        private readonly ITestOutputHelper _output;

        public GuessGameDiagramShould(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void OutputADiagram()
        {
            var sut = new GuessGame();
            _output.WriteLine(sut.AsDiagram());
        }
    }
}
