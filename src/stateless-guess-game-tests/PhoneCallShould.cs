using System;
using stateless_guess_game.TelephoneCallExample;
using Xunit;
using Xunit.Abstractions;

namespace stateless_guess_game_tests
{
    public class PhoneCallShould
    {
        public ITestOutputHelper Output { get; }

        public PhoneCallShould(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void DoAPhoneCall()
        {
            var phoneCall = new PhoneCall("Lokesh", Output);

            phoneCall.Print();
            phoneCall.Dialed("Prameela");
            phoneCall.Print();
            phoneCall.Connected();
            phoneCall.Print();
            phoneCall.SetVolume(2);
            phoneCall.Print();
            phoneCall.Hold();
            phoneCall.Print();
            phoneCall.Mute();
            phoneCall.Print();
            phoneCall.Unmute();
            phoneCall.Print();
            phoneCall.Resume();
            phoneCall.Print();
            phoneCall.SetVolume(11);
            phoneCall.Print();

            Output.WriteLine(phoneCall.ToDotGraph());

            //Console.WriteLine("Press any key...");
            //Console.ReadKey(true);
        }
    }
}
