using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace SimpleDPI;

public class ProcessManager
{
    private Process? _dpiProcess;
    private readonly string _tempEnvDir = Path.Combine(Path.GetTempPath(), "SimpleDPI_Agent");
    
    public bool IsRunning => (_dpiProcess != null && !_dpiProcess.HasExited) || Process.GetProcessesByName("goodbyedpi").Length > 0;

    public event Action? ProcessExited;

    public void Start(string arguments)
    {
        if (IsRunning)
        {
            Stop();
        }

        // Extract embedded resources if they don't exist
        string exePath = Path.Combine(_tempEnvDir, "goodbyedpi.exe");
        EnsureResourcesExtracted();

        if (!File.Exists(exePath))
        {
            // Fallback (just in case)
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            exePath = Path.Combine(baseDir, "goodbyedpi.exe");
            if (!File.Exists(exePath)) exePath = Path.Combine(baseDir, "x86_64", "goodbyedpi.exe");
            if (!File.Exists(exePath)) exePath = Path.Combine(baseDir, "x86", "goodbyedpi.exe");
        }

        if (!File.Exists(exePath))
        {
            throw new FileNotFoundException("goodbyedpi.exe bulunamadı (Yerleşik kaynaklarda da yok)!");
        }

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = arguments,
            CreateNoWindow = true,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden,
            WorkingDirectory = Path.GetDirectoryName(exePath),
            RedirectStandardOutput = false,
            RedirectStandardError = false,
        };

        _dpiProcess = new Process { StartInfo = psi };
        _dpiProcess.EnableRaisingEvents = true;
        _dpiProcess.Exited += (s, e) => ProcessExited?.Invoke();

        _dpiProcess.Start();
    }

    private void EnsureResourcesExtracted()
    {
        if (!Directory.Exists(_tempEnvDir))
        {
            Directory.CreateDirectory(_tempEnvDir);
        }

        ExtractGzResource("SimpleDPI.Payload.goodbyedpi.exe.gz", Path.Combine(_tempEnvDir, "goodbyedpi.exe"));
        ExtractGzResource("SimpleDPI.Payload.WinDivert.dll.gz", Path.Combine(_tempEnvDir, "WinDivert.dll"));
        ExtractGzResource("SimpleDPI.Payload.WinDivert64.sys.gz", Path.Combine(_tempEnvDir, "WinDivert64.sys"));
    }

    private void ExtractGzResource(string resourceName, string outPath)
    {
        if (File.Exists(outPath))
        {
            try { File.Delete(outPath); } catch { return; }
        }

        using Stream? s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        if (s == null) return;

        using var gzip = new GZipStream(s, CompressionMode.Decompress);
        using var outFile = File.Create(outPath);
        gzip.CopyTo(outFile);
    }

    public void Stop()
    {
        if (_dpiProcess != null && !_dpiProcess.HasExited)
        {
            try
            {
                _dpiProcess.Kill();
                _dpiProcess.WaitForExit(1000);
            }
            catch { }
        }
        
        foreach (var p in Process.GetProcessesByName("goodbyedpi"))
        {
            try { p.Kill(); } catch { }
        }

        _dpiProcess?.Dispose();
        _dpiProcess = null;
    }
}
