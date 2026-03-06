global using static global::Windows.Win32.ManualDefinitions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

namespace Windows.Win32;

[UnmanagedFunctionPointer(CallingConvention.Winapi)]
internal delegate LRESULT WNDPROC([In] HWND hWnd, [In] uint uMsg, [In] WPARAM wParam, [In] LPARAM lParam);

internal static class ManualDefinitions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SUCCEEDED(HRESULT hr) => hr >= 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool FAILED(HRESULT hr) => hr < 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort LOWORD(nint l) => unchecked((ushort)(((nuint)l) & 0xFFFF));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort HIWORD(nint l) => (ushort)((((nuint)l) >> 16) & 0xFFFF);
}
