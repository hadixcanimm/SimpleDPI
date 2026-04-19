using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.Win32;

namespace SimpleDPI;

public static class SystemUtils
{
    // --- AUTORUN REGISTRY ---
    private const string AppName = "SimpleDPI";
    
    public static void SetStartOnBoot(bool enable)
    {
        try
        {
            // Clean up the old registry method if it exists
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                key?.DeleteValue(AppName, false);
            }
            catch { }

            string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = System.IO.Path.Combine(startupFolderPath, AppName + ".lnk");

            if (enable)
            {
                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
                
                Type? t = Type.GetTypeFromProgID("WScript.Shell");
                if (t != null)
                {
                    dynamic? shell = Activator.CreateInstance(t);
                    if (shell != null)
                    {
                        var shortcut = shell.CreateShortcut(shortcutPath);
                        shortcut.TargetPath = exePath;
                        shortcut.Arguments = "--autostart";
                        shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(exePath);
                        shortcut.Save();
                    }
                }
            }
            else
            {
                if (System.IO.File.Exists(shortcutPath))
                {
                    System.IO.File.Delete(shortcutPath);
                }
            }
        }
        catch { }
    }

    // --- BLUR / ACRYLIC ---
    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        WCA_ACCENT_POLICY = 19
    }

    internal enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
        ACCENT_INVALID_STATE = 5
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [DllImport("user32.dll")]
    internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

    public static void EnableBlur(System.Windows.Window window, bool enable)
    {
        var windowHelper = new WindowInteropHelper(window);
        IntPtr hwnd = windowHelper.EnsureHandle();

        var accent = new AccentPolicy();
        var accentStructSize = Marshal.SizeOf(accent);

        if (enable)
        {
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND; // Safe blur for win10/11
            accent.GradientColor = (0 << 24) | (0x1A1A1A); 
        }
        else
        {
            accent.AccentState = AccentState.ACCENT_DISABLED;
        }

        IntPtr accentPtr = Marshal.AllocHGlobal(accentStructSize);
        Marshal.StructureToPtr(accent, accentPtr, false);

        var data = new WindowCompositionAttributeData
        {
            Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
            SizeOfData = accentStructSize,
            Data = accentPtr
        };

        SetWindowCompositionAttribute(hwnd, ref data);

        Marshal.FreeHGlobal(accentPtr);
    }

    // --- EFFICIENCY MODE (Win 11) ---
    [StructLayout(LayoutKind.Sequential)]
    private struct PROCESS_POWER_THROTTLING_STATE
    {
        public uint Version;
        public uint ControlMask;
        public uint StateMask;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetProcessInformation(IntPtr hProcess, int ProcessInformationClass, ref PROCESS_POWER_THROTTLING_STATE ProcessInformation, int ProcessInformationSize);

    public static void SetEfficiencyMode(bool enable)
    {
        try
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            
            // 1. Priority Class
            process.PriorityClass = enable ? System.Diagnostics.ProcessPriorityClass.Idle : System.Diagnostics.ProcessPriorityClass.Normal;

            // 2. Power Throttling (Quality of Service - EcoQoS)
            var state = new PROCESS_POWER_THROTTLING_STATE
            {
                Version = 1, // PROCESS_POWER_THROTTLING_CURRENT_VERSION
                ControlMask = 1, // PROCESS_POWER_THROTTLING_EXECUTION_SPEED
                StateMask = enable ? 1u : 0u
            };

            SetProcessInformation(process.Handle, 4, ref state, Marshal.SizeOf(state));
        }
        catch { }
    }
}
