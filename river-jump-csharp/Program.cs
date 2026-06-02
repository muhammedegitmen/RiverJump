namespace RiverJump;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        using var game = new GameForm();
        Application.Run(game);
    }
}
