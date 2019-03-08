using System;
using System.Collections.Generic;
using System.Linq;
using stateless_guess_game.Quiltoni.PixelBot.Commands;
using Stateless;
using Stateless.Graph;

namespace stateless_guess_game
{
    public enum GuessGameState
    {
        NotStarted,
        OpenTakingGuesses,
        GuessesClosed
    }

    public enum GuessGameTrigger
    {
        Help,
        Open,
        Close,
        Reset,
        TakeGuess,
        Mine
    }

    public class GuessGame
    {
        private readonly StateMachine<GuessGameState, GuessGameTrigger> _machine;
        readonly StateMachine<GuessGameState, GuessGameTrigger>.TriggerWithParameters<IChatService, ChatCommand> _setHelpTrigger;
        readonly StateMachine<GuessGameState, GuessGameTrigger>.TriggerWithParameters<IChatService,ChatCommand> _setGuessTrigger;
        readonly StateMachine<GuessGameState, GuessGameTrigger>.TriggerWithParameters<IChatService, ChatCommand> _setOpenTrigger;
        readonly StateMachine<GuessGameState, GuessGameTrigger>.TriggerWithParameters<IChatService, ChatCommand> _setCloseTrigger;
        readonly StateMachine<GuessGameState, GuessGameTrigger>.TriggerWithParameters<IChatService, ChatCommand> _setMineTrigger;
        readonly StateMachine<GuessGameState, GuessGameTrigger>.TriggerWithParameters<IChatService, ChatCommand> _setResetTrigger;
        private static readonly Dictionary<string, TimeSpan> Guesses = new Dictionary<string, TimeSpan>();

        public GuessGame()
        {
            _machine = new StateMachine<GuessGameState, GuessGameTrigger>(GuessGameState.NotStarted);
            _setHelpTrigger = _machine.SetTriggerParameters<IChatService,ChatCommand>(GuessGameTrigger.Help);
            _setGuessTrigger = _machine.SetTriggerParameters<IChatService, ChatCommand>(GuessGameTrigger.TakeGuess);
            _setOpenTrigger = _machine.SetTriggerParameters<IChatService, ChatCommand>(GuessGameTrigger.Open);
            _setCloseTrigger = _machine.SetTriggerParameters<IChatService, ChatCommand>(GuessGameTrigger.Close);
            _setMineTrigger = _machine.SetTriggerParameters<IChatService, ChatCommand>(GuessGameTrigger.Mine);
            _setResetTrigger = _machine.SetTriggerParameters<IChatService, ChatCommand>(GuessGameTrigger.Reset);

            _machine.Configure(GuessGameState.NotStarted)
                .OnEntry(OnNotStartedAction)
                .InternalTransition(_setHelpTrigger, OnHelpCommand)
                .PermitDynamicIf(_setOpenTrigger, WhenOpeningFromNotStarted, CanOpen);

            _machine.Configure(GuessGameState.OpenTakingGuesses)
                .InternalTransition(_setGuessTrigger, OnTakeGuessCommand)
                .InternalTransition(_setMineTrigger, OnMineCommand)
                .InternalTransition(_setHelpTrigger, OnHelpCommand)
                .PermitDynamicIf(_setCloseTrigger, WhenClosingFromTakingGuesses, CanClose)
                ;

            _machine.Configure(GuessGameState.GuessesClosed)
                .InternalTransition(_setMineTrigger, OnMineCommand)
                .PermitDynamicIf(_setOpenTrigger, WhenOpeningFromClosed, CanClose)
                .PermitDynamicIf(_setResetTrigger, WhenResetting, CanReset);
        }

        private GuessGameState WhenResetting(IChatService twitch, ChatCommand cmd)
        {
            if (!TimeSpan.TryParse(cmd.ArgumentsAsList[0], out TimeSpan time)) return GuessGameState.NotStarted;
            if (Guesses.Any(kv => kv.Value == time))
            {

                var found = Guesses.FirstOrDefault(kv => kv.Value == time);
                twitch.BroadcastMessageOnChannel($"WINNER!!! - Congratulations {found.Key} - you have won!");

            }
            else
            {

                var closest = Guesses.Select(g => new { user = g.Key, seconds = g.Value.TotalSeconds, close = Math.Abs(g.Value.TotalSeconds - time.TotalSeconds) })
                    .OrderBy(g => g.close)
                    .First();

                twitch.BroadcastMessageOnChannel($"No winners THIS time, but {closest.user} was closest at just {closest.close} seconds off!");

            }
            return GuessGameState.NotStarted;
        }
        private GuessGameState WhenOpeningFromClosed(IChatService twitch, ChatCommand cmd)
        {
            twitch.BroadcastMessageOnChannel("Now taking guesses again!  You may log a new guess or change your guess now!");
            return GuessGameState.OpenTakingGuesses;
        }
        private GuessGameState WhenClosingFromTakingGuesses(IChatService twitch, ChatCommand cmd)
        {
            twitch.BroadcastMessageOnChannel($"No more guesses...  the race is about to start with {Guesses.Count} guesses from {Guesses.Min(kv => kv.Value).ToString()} to {Guesses.Max(kv => kv.Value).ToString()}");
            return GuessGameState.GuessesClosed;
        }
        private GuessGameState WhenOpeningFromNotStarted(IChatService twitch, ChatCommand cmd)
        {
            twitch.BroadcastMessageOnChannel("Now taking guesses. Submit your guess with !guess 1:23 where 1 is minutes and 23 is seconds.");
            return GuessGameState.OpenTakingGuesses;
        }
        private bool CanClose(IChatService service, ChatCommand cmd)
        {
            return cmd.ChatMessage.IsBroadcaster || cmd.ChatMessage.IsModerator;
        }
        private bool CanReset(IChatService service, ChatCommand cmd)
        {
            return cmd.ChatMessage.IsBroadcaster || cmd.ChatMessage.IsModerator;
        }
        private bool CanOpen(IChatService service, ChatCommand cmd)
        {
            return cmd.ChatMessage.IsBroadcaster || cmd.ChatMessage.IsModerator;
        }
        private void OnNotStartedAction()
        {
            Guesses.Clear();
        }
        private void OnMineCommand(IChatService twitch, ChatCommand cmd, StateMachine<GuessGameState, GuessGameTrigger>.Transition transition)
        {
            switch (transition.Destination)
            {
                case GuessGameState.NotStarted:
                    break;
                case GuessGameState.OpenTakingGuesses:
                    twitch.BroadcastMessageOnChannel(Guesses.Any(kv => kv.Key == cmd.ChatMessage.Username)
                        ? $"{cmd.ChatMessage.Username} guessed {Guesses[cmd.ChatMessage.Username].ToString()}"
                        : $"{cmd.ChatMessage.Username} has not guessed yet!");
                    break;
                case GuessGameState.GuessesClosed:
                    twitch.BroadcastMessageOnChannel(Guesses.Any(kv => kv.Key == cmd.ChatMessage.Username)
                        ? $"{cmd.ChatMessage.Username} guessed {Guesses[cmd.ChatMessage.Username].ToString()}"
                        : $"{cmd.ChatMessage.Username} has not guessed yet!");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void OnTakeGuessCommand(IChatService twitch, ChatCommand cmd, StateMachine<GuessGameState, GuessGameTrigger>.Transition transition)
        {
            if (cmd.ArgumentsAsList[0].Contains("-") ||
                !TimeSpan.TryParseExact(cmd.ArgumentsAsList[0], "m\\:ss", null, out TimeSpan time))
            {
                twitch.BroadcastMessageOnChannel($"Sorry {cmd.ChatMessage.Username}, guesses are only accepted in the format !guess 1:23");
                return;
            }
            if (Guesses.Any(kv => kv.Value == time))
            {
                var firstGuess = Guesses.First(kv => kv.Value == time);
                twitch.BroadcastMessageOnChannel($"Sorry {cmd.ChatMessage.Username}, {firstGuess.Key} already guessed {time.ToString()}");
            }
            else if (Guesses.Any(kv => kv.Key == cmd.ChatMessage.Username))
            {
                Guesses[cmd.ChatMessage.Username] = time;
            }
            else
            {
                Guesses.Add(cmd.ChatMessage.Username, time);
            }
        }
        private void OnHelpCommand(IChatService twitch, ChatCommand cmd, StateMachine<GuessGameState, GuessGameTrigger>.Transition transition)
        {
            switch (transition.Destination)
            {
                case GuessGameState.NotStarted:
                    twitch.WhisperMessage(cmd.ChatMessage.DisplayName, "The time-guessing game is not currently running.  To open the game for guesses, execute !guess open");
                    break;
                case GuessGameState.OpenTakingGuesses:
                    if (cmd.ChatMessage.IsBroadcaster || cmd.ChatMessage.IsModerator)
                    {
                        twitch.WhisperMessage(cmd.ChatMessage.Username, "The time-guessing game is currently taking guesses.  Guess a time with !guess 1:23 OR close the guesses with !guess close");
                        return;
                    }
                    twitch.BroadcastMessageOnChannel("The time-guessing game is currently taking guesses.  Guess a time with !guess 1:23  Your last guess will stand, and you can check your guess with !guess mine");
                    break;
                case GuessGameState.GuessesClosed:
                    if(!cmd.ChatMessage.IsBroadcaster && !cmd.ChatMessage.IsModerator)
                        twitch.BroadcastMessageOnChannel("The time-guessing game is currently CLOSED.  You can check your guess with !guess mine");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public GuessGameState CurrentState()
        {
            return _machine.State;
        }

        public string AsDiagram()
        {
            return UmlDotGraph.Format(_machine.GetInfo());
        }

        public void Help(IChatService service, ChatCommand cmd)
        {
            _machine.Fire(_setHelpTrigger, service, cmd);
        }

        public void Open(IChatService service, ChatCommand cmd)
        {
            _machine.Fire(_setOpenTrigger, service, cmd);
        }

        public void Close(IChatService service, ChatCommand cmd)
        {
            _machine.Fire(_setCloseTrigger, service, cmd);
        }

        public void Reset(IChatService service, ChatCommand cmd)
        {
            _machine.Fire(_setResetTrigger, service, cmd);
        }

        public void Guess(IChatService service, ChatCommand cmd)
        {
            _machine.Fire(_setGuessTrigger, service, cmd);
        }

        public void Mine(IChatService service, ChatCommand cmd)
        {
            _machine.Fire(_setMineTrigger, service, cmd);
        }
    }
}
