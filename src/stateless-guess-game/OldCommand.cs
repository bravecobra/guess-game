﻿namespace stateless_guess_game
{
    using System;
    using System.Collections.Generic;
    using System.Linq;


    namespace Quiltoni.PixelBot.Commands
    {
        public class GuessTimeCommandOld
        {

            // Cheer 400 cpayette 05/3/19 
            // Cheer 100 roberttables 05/3/19 

            public enum GuessGameState
            {
                NotStarted,
                OpenTakingGuesses,
                GuessesClosed
            }

            public string CommandText => "guess";

            public static GuessGameState State = GuessGameState.NotStarted;

            private static readonly Dictionary<string, TimeSpan> _Guesses = new Dictionary<string, TimeSpan>();

            private static readonly Dictionary<GuessGameState, Action<GuessTimeCommandOld, GuessGameCommand, IChatService>> _GameHandlers = new Dictionary<GuessGameState, Action<GuessTimeCommandOld, GuessGameCommand, IChatService>> {
            { GuessGameState.NotStarted, NotStarted },
            { GuessGameState.OpenTakingGuesses, TakeGuess },
            { GuessGameState.GuessesClosed, Closed }
        };

            public void Execute(GuessGameCommand command, IChatService twitch)
            {

                _GameHandlers[State](this, command, twitch);

            }

            private static void NotStarted(GuessTimeCommandOld guess, GuessGameCommand cmd, IChatService twitch)
            {

                // This is a moderator / broadcaster ONLY command
                if (!cmd.ChatUser.IsBroadcaster && !cmd.ChatUser.IsModerator)
                   return;

               if (cmd.ArgumentsAsList.Count == 0 || cmd.ArgumentsAsList[0] == "help")
               {
                   twitch.WhisperMessage(cmd.ChatUser.DisplayName, "The time-guessing game is not currently running.  To open the game for guesses, execute !guess open");
                   return;
               }

                if (cmd.ArgumentsAsList[0] == "open" && State != GuessGameState.OpenTakingGuesses)
                {

                   State = GuessGameState.OpenTakingGuesses;
                   _Guesses.Clear();
                   twitch.BroadcastMessageOnChannel("Now taking guesses. Submit your guess with !guess 1:23 where 1 is minutes and 23 is seconds.");

                }

            }

            private static void TakeGuess(GuessTimeCommandOld guess, GuessGameCommand cmd, IChatService twitch)
            {

                if ((cmd.ChatUser.IsBroadcaster || cmd.ChatUser.IsModerator) && (cmd.ArgumentsAsList.Count == 0 || cmd.ArgumentsAsList[0] == "help"))
                {
                   twitch.WhisperMessage(cmd.ChatUser.Username, "The time-guessing game is currently taking guesses.  Guess a time with !guess 1:23 OR close the guesses with !guess close");
                   return;
                }
                else if (cmd.ArgumentsAsList.Count == 0 || cmd.ArgumentsAsList[0] == "help")
                {
                   twitch.BroadcastMessageOnChannel("The time-guessing game is currently taking guesses.  Guess a time with !guess 1:23  Your last guess will stand, and you can check your guess with !guess mine");
                   return;
                }

                if ((cmd.ChatUser.IsBroadcaster || cmd.ChatUser.IsModerator) && (cmd.ArgumentsAsList[0] == "close"))
                {
                   State = GuessGameState.GuessesClosed;
                   twitch.BroadcastMessageOnChannel($"No more guesses...  the race is about to start with {_Guesses.Count} guesses from {_Guesses.Min(kv => kv.Value).ToString()} to {_Guesses.Max(kv => kv.Value).ToString()}");
                   return;
                }

                if (!cmd.ArgumentsAsList[0].Contains("-") && TimeSpan.TryParseExact(cmd.ArgumentsAsList[0], "m\\:ss", null, out TimeSpan time))
                {

                   if (_Guesses.Any(kv => kv.Value == time))
                   {
                       var firstGuess = _Guesses.First(kv => kv.Value == time);
                       twitch.BroadcastMessageOnChannel($"Sorry {cmd.ChatUser.Username}, {firstGuess.Key} already guessed {time.ToString()}");
                       return;
                   }
                   else if (_Guesses.Any(kv => kv.Key == cmd.ChatUser.Username))
                   {
                       _Guesses[cmd.ChatUser.Username] = time;
                   }
                   else
                   {
                       _Guesses.Add(cmd.ChatUser.Username, time);
                   }

                }
                else if (cmd.ArgumentsAsList[0] == "mine")
                {
                   if (_Guesses.Any(kv => kv.Key == cmd.ChatUser.Username))
                   {
                       twitch.BroadcastMessageOnChannel($"{cmd.ChatUser.Username} guessed {_Guesses[cmd.ChatUser.Username].ToString()}");
                   }
                   else
                   {
                       twitch.BroadcastMessageOnChannel($"{cmd.ChatUser.Username} has not guessed yet!");
                   }

                }
                else
                {
                   twitch.BroadcastMessageOnChannel($"Sorry {cmd.ChatUser.Username}, guesses are only accepted in the format !guess 1:23");
                   return;
                }

            }

            private static void Closed(GuessTimeCommandOld guess, GuessGameCommand cmd, IChatService twitch)
            {

                if ((cmd.ArgumentsAsList.Count == 0 || cmd.ArgumentsAsList[0] == "help") && (!cmd.ChatUser.IsBroadcaster && !cmd.ChatUser.IsModerator))
                {
                   twitch.BroadcastMessageOnChannel("The time-guessing game is currently CLOSED.  You can check your guess with !guess mine");
                   return;
                }

                if (cmd.ArgumentsAsList[0] == "mine")
                {
                    if (_Guesses.Any(kv => kv.Key == cmd.ChatUser.Username))
                    {
                       twitch.BroadcastMessageOnChannel($"{cmd.ChatUser.Username} guessed {_Guesses[cmd.ChatUser.Username].ToString()}");
                    }
                    else
                    {
                       twitch.BroadcastMessageOnChannel($"{cmd.ChatUser.Username} has not guessed yet!");
                    }
                    return;
                }

                // This is a moderator / broadcaster ONLY command
                if (!cmd.ChatUser.IsBroadcaster && !cmd.ChatUser.IsModerator)
                    return;

                if (cmd.ArgumentsAsList[0] == "help")
                {
                    twitch.WhisperMessage(cmd.ChatUser.Username, $"The time-guessing game is currently CLOSED with {_Guesses.Count} guesses awaiting an outcome.  Guess a time with !guess 1:23 OR close the guesses with !guess close");
                    return;
                }

                if ((cmd.ChatUser.IsBroadcaster || cmd.ChatUser.IsModerator) && cmd.ArgumentsAsList[0] == "reopen")
                {
                   State = GuessGameState.OpenTakingGuesses;
                   twitch.BroadcastMessageOnChannel("Now taking guesses again!  You may log a new guess or change your guess now!");
                   return;
                }
                else if (cmd.ArgumentsAsList[0] == "end")
                {

                   State = GuessGameState.NotStarted;
                   _Guesses.Clear();
                   return;

                }
                else if (TimeSpan.TryParse(cmd.ArgumentsAsList[0], out TimeSpan time))
                {

                    if (_Guesses.Any(kv => kv.Value == time))
                    {

                        var found = _Guesses.FirstOrDefault(kv => kv.Value == time);
                        twitch.BroadcastMessageOnChannel($"WINNER!!! - Congratulations {found.Key} - you have won!");

                    }
                    else
                    {

                        var closest = _Guesses.Select(g => new { user = g.Key, seconds = g.Value.TotalSeconds, close = Math.Abs(g.Value.TotalSeconds - time.TotalSeconds) })
                            .OrderBy(g => g.close)
                            .First();

                        twitch.BroadcastMessageOnChannel($"No winners THIS time, but {closest.user} was closest at just {closest.close} seconds off!");

                    }

                }

            }

        }

    }
}
