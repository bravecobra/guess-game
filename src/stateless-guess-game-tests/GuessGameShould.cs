using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using stateless_guess_game;
using stateless_guess_game.Quiltoni.PixelBot.Commands;
using Xunit;
using Xunit.Abstractions;

namespace stateless_guess_game_tests
{
    public class GuessGameShould
    {
        private readonly ITestOutputHelper _output;

        public GuessGameShould(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void StartInNotStartedState_WhenConstructed()
        {
            var sut = new GuessGame();
            Assert.Equal(GuessGameState.NotStarted, sut.CurrentState());
        }

        [Fact]
        public void OutputADiagram()
        {
            var sut = new GuessGame();
            _output.WriteLine(sut.AsDiagram());
        }

        [Fact]
        public void CallReturnHelp_WhenCommandIsTriggeredWithHelp_GivenStateIsNotStarted()
        {
            var sut = new GuessGame();
            Mock<IChatService> chatService = new Mock<IChatService>();
            chatService.Setup(x => x.WhisperMessage(It.IsAny<string>(), It.IsAny<string>()));
            var cmd = new ChatCommand()
            {
                ArgumentsAsList = new List<string>()
                {
                    "help",
                },
                ChatMessage = new ChatMessage()
                {
                    DisplayName = "User1",
                    Username = "user1"
                }
            };
            sut.Help(chatService.Object, cmd);
            chatService.Verify(service => service.WhisperMessage(
                It.Is<string>(u => u== "User1"), 
                It.Is<string>(m =>m == "The time-guessing game is not currently running.  To open the game for guesses, execute !guess open")),Times.Once);            
        }

        [Fact]
        public void CallReturnHelp_WhenCommandIsTriggeredWihoutArgs_GivenStateIsNotStarted()
        {
            var sut = new GuessGame();
            Mock<IChatService> chatserviceMock = new Mock<IChatService>();
            chatserviceMock.Setup(x => x.WhisperMessage(It.IsAny<string>(), It.IsAny<string>()));
            var cmd = new ChatCommand()
            {
                ArgumentsAsList = new List<string>()
                {
                    //"",
                },
                ChatMessage = new ChatMessage()
                {
                    DisplayName = "User1",
                    Username = "user1"
                }
            };
            sut.Help(chatserviceMock.Object, cmd);
            chatserviceMock.Verify(service => service.WhisperMessage(
                It.Is<string>(u => u == "User1"),
                It.Is<string>(m => m == "The time-guessing game is not currently running.  To open the game for guesses, execute !guess open")), Times.Once);
        }

        [Fact]
        public void TransitionToTakingGuessesAndBroadcast_WhenOpenIsTriggered_GivenStateIsNotStarted()
        {
            var sut = new GuessGame();
            Mock<IChatService> chatserviceMock = new Mock<IChatService>();
            chatserviceMock.Setup(x => x.BroadcastMessageOnChannel(It.IsAny<string>()));
            var cmd = new ChatCommand()
            {
                ArgumentsAsList = new List<string>()
                {
                    "open",
                },
                ChatMessage = new ChatMessage()
                {
                    IsBroadcaster = true,
                    IsModerator = false,
                    DisplayName = "User1",
                    Username = "user1"
                }
            };

            sut.Open(chatserviceMock.Object, cmd);
        }

        [Fact]
        public void GamePlay()
        {
            var sut = new GuessGame();
            Assert.Equal(GuessGameState.NotStarted, sut.CurrentState());
            sut.Help(new stubChat(_output), new ChatCommand()
            {
                ArgumentsAsList = new List<string>() { "help" },
                ChatMessage = new ChatMessage() { IsModerator = true, IsBroadcaster = false, DisplayName = "bravecobra", Username = "bravecobra2" }
            });
            Assert.Equal(GuessGameState.NotStarted, sut.CurrentState());
            sut.Open(new stubChat(_output), new ChatCommand(){
                ArgumentsAsList = new List<string>(){ "open" },
                ChatMessage = new ChatMessage(){IsModerator = true, IsBroadcaster = false, DisplayName = "bravecobra", Username = "bravecobra2"}});
            Assert.Equal(GuessGameState.OpenTakingGuesses, sut.CurrentState());
            sut.Guess(new stubChat(_output), new ChatCommand()
            {
                ArgumentsAsList = new List<string>() { "1:23" },
                ChatMessage = new ChatMessage() { IsModerator = true, IsBroadcaster = false, DisplayName = "bravecobra", Username = "bravecobra2" }
            });
            sut.Guess(new stubChat(_output), new ChatCommand()
            {
                ArgumentsAsList = new List<string>() { "mine" },
                ChatMessage = new ChatMessage() { IsModerator = true, IsBroadcaster = false, DisplayName = "bravecobra", Username = "bravecobra2" }
            });
            Assert.Equal(GuessGameState.OpenTakingGuesses, sut.CurrentState());
            sut.Guess(new stubChat(_output), new ChatCommand()
            {
                ArgumentsAsList = new List<string>() { "1:22" },
                ChatMessage = new ChatMessage() { IsModerator = false, IsBroadcaster = true, DisplayName = "csharpfritz", Username = "csharpfritz" }
            });
            Assert.Equal(GuessGameState.OpenTakingGuesses, sut.CurrentState());
            sut.Guess(new stubChat(_output), new ChatCommand()
            {
                ArgumentsAsList = new List<string>() { "1:61" },
                ChatMessage = new ChatMessage() { IsModerator = false, IsBroadcaster = false, DisplayName = "someone", Username = "someone" }
            });
            sut.Mine(new stubChat(_output), new ChatCommand()
            {
                ArgumentsAsList = new List<string>() { "mine" },
                ChatMessage = new ChatMessage() { IsModerator = false, IsBroadcaster = false, DisplayName = "someone", Username = "someone" }
            });
            sut.Guess(new stubChat(_output), new ChatCommand()
            {
                ArgumentsAsList = new List<string>() { "1:41" },
                ChatMessage = new ChatMessage() { IsModerator = false, IsBroadcaster = false, DisplayName = "someone", Username = "someone" }
            });
            sut.Mine(new stubChat(_output), new ChatCommand()
            {
                ArgumentsAsList = new List<string>() { "mine" },
                ChatMessage = new ChatMessage() { IsModerator = false, IsBroadcaster = false, DisplayName = "someone", Username = "someone" }
            });
            Assert.Equal(GuessGameState.OpenTakingGuesses, sut.CurrentState());
            sut.Guess(new stubChat(_output), new ChatCommand()
            {
                ArgumentsAsList = new List<string>() { "1:25" },
                ChatMessage = new ChatMessage() { IsModerator = false, IsBroadcaster = false, DisplayName = "someone", Username = "someone" }
            });
            Assert.Equal(GuessGameState.OpenTakingGuesses, sut.CurrentState());
            sut.Close(new stubChat(_output), new ChatCommand()
            {
                ArgumentsAsList = new List<string>() { "close" },
                ChatMessage = new ChatMessage() { IsModerator = false, IsBroadcaster = true, DisplayName = "csharpfritz", Username = "csharpfritz" }
            });
            Assert.Equal(GuessGameState.GuessesClosed, sut.CurrentState());
            sut.Open(new stubChat(_output), new ChatCommand()
            {
                ArgumentsAsList = new List<string>() { "open" },
                ChatMessage = new ChatMessage() { IsModerator = false, IsBroadcaster = true, DisplayName = "csharpfritz", Username = "csharpfritz" }
            });
            Assert.Equal(GuessGameState.OpenTakingGuesses, sut.CurrentState());
            sut.Close(new stubChat(_output), new ChatCommand()
            {
                ArgumentsAsList = new List<string>() { "close" },
                ChatMessage = new ChatMessage() { IsModerator = false, IsBroadcaster = true, DisplayName = "csharpfritz", Username = "csharpfritz" }
            });
            Assert.Equal(GuessGameState.GuessesClosed, sut.CurrentState());
            sut.Reset(new stubChat(_output), new ChatCommand()
            {
                ArgumentsAsList = new List<string>() { "reset" },
                ChatMessage = new ChatMessage() { IsModerator = false, IsBroadcaster = true, DisplayName = "csharpfritz", Username = "csharpfritz" }
            });
            Assert.Equal(GuessGameState.NotStarted, sut.CurrentState());
        }
    }

    class stubChat: IChatService
    {
        private readonly ITestOutputHelper _output;

        public stubChat(ITestOutputHelper output)
        {
            _output = output;
        }
        public void BroadcastMessageOnChannel(string p0)
        {
            _output.WriteLine(p0);
        }

        public void WhisperMessage(object username, string s)
        {
            _output.WriteLine($"{username}: {s}");
        }
    }
}
