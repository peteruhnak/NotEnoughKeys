namespace NotEnoughKeys;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new NekForm(args.Length == 1 ? args[0] : "conf.json"));
    }
}