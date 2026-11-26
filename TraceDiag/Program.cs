namespace TraceDiag;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm(ResetRequested(args)));
    }

    internal static bool ResetRequested(string[] args) =>
        args.Any(arg => arg.TrimStart('-', '/').Equals(Strings.App.ResetArgument, StringComparison.OrdinalIgnoreCase));
}
