[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$ExecutablePath,
    [Parameter(Mandatory)]
    [string]$OutputPath,
    [int]$TimeoutSeconds = 45,
    [int]$ReadyDelaySeconds = 12
)

$ErrorActionPreference = 'Stop'

Add-Type @'
using System;
using System.Runtime.InteropServices;

public static class NexsusWindowCapture
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hWnd, out Rect rect);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetProcessDPIAware();
}
'@

$null = [NexsusWindowCapture]::SetProcessDPIAware()
$temporaryProfile = Join-Path ([System.IO.Path]::GetTempPath()) "NexsusVKVideo-Screenshot-$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $temporaryProfile -Force | Out-Null
$environment = @{ LOCALAPPDATA = $temporaryProfile; APPDATA = $temporaryProfile }
$process = Start-Process -FilePath $ExecutablePath -PassThru -Environment $environment

try {
    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
    do {
        Start-Sleep -Milliseconds 500
        $process.Refresh()
    } while ($process.MainWindowHandle -eq [IntPtr]::Zero -and [DateTime]::UtcNow -lt $deadline)

    if ($process.MainWindowHandle -eq [IntPtr]::Zero) {
        throw "NexsusVKVideo window did not appear within $TimeoutSeconds seconds."
    }

    Start-Sleep -Seconds $ReadyDelaySeconds
    $process.Refresh()

    $windowRect = [NexsusWindowCapture+Rect]::new()
    if (-not [NexsusWindowCapture]::GetWindowRect($process.MainWindowHandle, [ref]$windowRect)) {
        throw 'Could not obtain the NexsusVKVideo window bounds.'
    }

    $width = $windowRect.Right - $windowRect.Left
    $height = $windowRect.Bottom - $windowRect.Top
    if ($width -le 0 -or $height -le 0) {
        throw 'NexsusVKVideo window has invalid bounds.'
    }

    Add-Type -AssemblyName System.Drawing
    $bitmap = [System.Drawing.Bitmap]::new($width, $height)
    try {
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        try {
            $graphics.CopyFromScreen($windowRect.Left, $windowRect.Top, 0, 0, $bitmap.Size)
        }
        finally {
            $graphics.Dispose()
        }

        $directory = Split-Path -Parent $OutputPath
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
        $bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $bitmap.Dispose()
    }
}
finally {
    if (-not $process.HasExited) {
        $null = $process.CloseMainWindow()
        $process.WaitForExit(5000) | Out-Null
    }
}
