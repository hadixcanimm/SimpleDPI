using System.Diagnostics;
using System.Windows;
using System.Threading;

namespace SimpleDPI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private static Mutex? _mutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "SimpleDPI";
        
        // Single instance mantığı: Eski açık olanları kapat ve yenisini aç
        Process currentProcess = Process.GetCurrentProcess();
        Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);

        foreach (Process p in processes)
        {
            if (p.Id != currentProcess.Id)
            {
                try
                {
                    p.Kill();
                    p.WaitForExit(1000);
                }
                catch
                {
                    // Ignore errors during kill
                }
            }
        }

        // Initialize mutex for future checks, though the kill loop makes it almost redundant, 
        // it's good practice.
        _mutex = new Mutex(true, appName, out bool createdNew);

        MainWindow mainWindow = new MainWindow();
        mainWindow.Show();
    }
}
