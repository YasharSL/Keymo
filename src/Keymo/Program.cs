namespace Keymo;

/// <summary>Entry point: one instance per desktop, living in the tray.</summary>
internal static class Program
{
    [STAThread]
    private static void Main()
    {
        using var mutex = new Mutex(initiallyOwned: true, "Keymo.SingleInstance", out bool isFirstInstance);
        if (!isFirstInstance)
        {
            return;
        }

        ApplicationConfiguration.Initialize();
        using var app = new TrayApp();
        Application.Run(app);
    }
}
