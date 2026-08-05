using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Soundpost.App.Interop;

/// <summary>
/// Pulls the real Windows icon for a running process so the mixer shows Spotify's logo rather than
/// a letter tile. Uses the shell's icon cache via SHGetFileInfo (no System.Drawing dependency) and
/// memoises per executable path — icon extraction is far too slow for the session refresh loop.
/// Returns null when the process is gone or protected; callers fall back to a letter tile.
/// </summary>
internal static class AppIconLoader
{
    private static readonly Dictionary<string, ImageSource?> ByPath = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<int, ImageSource?> ByProcess = new();

    /// <summary>Best-effort icon for a process id. Cached; safe to call on every refresh.</summary>
    public static ImageSource? ForProcess(int processId)
    {
        if (ByProcess.TryGetValue(processId, out ImageSource? cached))
        {
            return cached;
        }

        ImageSource? icon = Load(ExecutablePath(processId));
        ByProcess[processId] = icon;
        return icon;
    }

    private static string? ExecutablePath(int processId)
    {
        if (processId <= 0)
        {
            return null; // The system-sounds session has no owning executable.
        }

        try
        {
            using Process process = Process.GetProcessById(processId);
            return process.MainModule?.FileName;
        }
        catch
        {
            // Exited, or a protected/elevated process we can't open — fall back to a letter tile.
            return null;
        }
    }

    private static ImageSource? Load(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        if (ByPath.TryGetValue(path, out ImageSource? cached))
        {
            return cached;
        }

        ImageSource? icon = Extract(path);
        ByPath[path] = icon;
        return icon;
    }

    private static ImageSource? Extract(string path)
    {
        var info = new ShellFileInfo();
        IntPtr result = SHGetFileInfo(path, 0, ref info, (uint)Marshal.SizeOf<ShellFileInfo>(), ShgfiIcon | ShgfiLargeIcon);
        if (result == IntPtr.Zero || info.hIcon == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            ImageSource source = Imaging.CreateBitmapSourceFromHIcon(
                info.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            source.Freeze(); // cached and handed to the UI thread later
            return source;
        }
        catch
        {
            return null;
        }
        finally
        {
            DestroyIcon(info.hIcon);
        }
    }

    private const uint ShgfiIcon = 0x000000100;
    private const uint ShgfiLargeIcon = 0x000000000;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ShellFileInfo
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SHGetFileInfo(
        string pszPath, uint dwFileAttributes, ref ShellFileInfo psfi, uint cbFileInfo, uint uFlags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr hIcon);
}
