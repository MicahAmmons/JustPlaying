using PlayingAround;
using System;
using System.IO;


static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            using var game = new Game1(); // or your game class
            game.Run();
        }
        catch (Exception ex)
        {
            File.WriteAllText("error_log.txt", ex.ToString());
            throw;
        }
    }
}
