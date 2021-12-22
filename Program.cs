using System;

namespace DontSleep
{
    public class Program
    {
        private static void Main(string[] args)
        {
            PowerUtilities.PreventPowerSave();

            Console.WriteLine($"Tap any key to stop program!");
            Console.ReadKey();

            PowerUtilities.Shutdown();
        }
    }
}