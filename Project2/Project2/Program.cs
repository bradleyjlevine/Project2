/*
 * Authors:  Bradley Levine and Aidan Helm
 * Project 2:  Quaking in Your Boots
 * Files: Game1.cs, Program.cs, Model.cs, MD3.cs, TargaImage.cs, AnimationTypes.cs
 */

using System;

namespace Project2
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new Game1())
                game.Run();
        }
    }
#endif
}
