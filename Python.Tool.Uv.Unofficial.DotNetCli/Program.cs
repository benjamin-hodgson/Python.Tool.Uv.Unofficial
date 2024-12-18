using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

var dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), GetUvDirectoryName());
var exe = Path.Combine(dir, OperatingSystem.IsWindows() ? "uv.exe" : "uv");

var p = Process.Start(exe, args);
await p.WaitForExitAsync();
return p.ExitCode;

// keep in sync with logic in Python.Tool.Uv.Unofficial.props
string GetUvDirectoryName()
    => $"uv-{GetArchitecture()}-{GetPlatform()}";

string GetPlatform()
    => OperatingSystem.IsLinux()
        ? $"unknown-linux-{GetAbi()}"
        : OperatingSystem.IsMacOS()
            ? "apple-darwin"
            : OperatingSystem.IsWindows()
                ? "pc-windows-msvc"
                : throw new NotImplementedException();

string GetAbi()
    => (IsGnu() ? "gnu" : "musl")
        + (RuntimeInformation.OSArchitecture == Architecture.Arm ? "eabihf" : "");

unsafe bool IsGnu()
{
    try
    {
        gnu_get_libc_version();
    }
    catch (EntryPointNotFoundException)
    {
        return false;
    }
    return true;
}

[DllImport("libc")]
static unsafe extern char* gnu_get_libc_version();

string GetArchitecture()
    => RuntimeInformation.OSArchitecture switch
    {
        Architecture.X86 => "i686",
        Architecture.X64 => "x86_64",
        Architecture.Arm => "arm",
        Architecture.Arm64 => "aarch64",
        Architecture.Wasm => throw new NotImplementedException(),
        Architecture.S390x => "s390x",
        Architecture.LoongArch64 => throw new NotImplementedException(),
        Architecture.Armv6 => throw new NotImplementedException(),
        Architecture.Ppc64le => "powerpc64le",
        _ => throw new NotImplementedException()
    };
