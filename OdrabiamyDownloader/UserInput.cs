using System;

namespace OdrabiamyDownloader
{
    public static class UserInput
    {
        /// <summary>
        /// Gets the integer from user's input.
        /// </summary>
        /// <param name="valueName">The name of value to get.</param>
        /// <param name="minValue">Minimum accepted value.</param>
        /// <param name="maxValue">Maximum accepted value.</param>
        /// <returns>The integer.</returns>
        public static int GetIntFromUser(string valueName, int minValue, int maxValue)
        {
            while (true)
            {
                Console.Write($"{valueName}: ");
                if (int.TryParse(Console.ReadLine(), out var value) && value >= minValue && value <= maxValue)
                    return value;
                Console.WriteLine("Wrong input, try again.");
            }
        }

        /// <summary>
        /// Gets the string from user's input.
        /// </summary>
        /// <param name="valueName">The name of value to get.</param>
        /// <returns>The string.</returns>
        public static string GetStringFromUser(string valueName)
        {
            Console.Write($"{valueName}: ");
            return Console.ReadLine();
        }

        /// <summary>
        /// Checks if the program can continue with its work.
        /// </summary>
        /// <returns>The boolean which specifies if the program can continue.</returns>
        public static bool CanContinue()
        {
            var input = Console.ReadLine();
            return input == "y";
        }
    }
}
