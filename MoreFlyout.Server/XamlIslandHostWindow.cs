// NOTE: Thanks for 0x5bfa! For the code of Flyout display, please refer to: https://github.com/0x5bfa/TrayIconFlyout
// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.
using System.Runtime.CompilerServices;
using Microsoft.UI;
using Microsoft.UI.Xaml.Hosting;
using MoreFlyout.Server.Helpers;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace MoreFlyout.Server;

internal unsafe partial class XamlIslandHostWindow : IDisposable
{
    private const string WindowClassName = "FlyoutControlHostClass";
    private const string WindowName = "FlyoutControlHostWindow";

    private readonly Windows.Win32.UI.WindowsAndMessaging.WNDPROC _wndProc;
    private HWND _xamlHwnd = default;

    internal HWND HWnd
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private set;
    }

    internal DesktopWindowXamlSource? DesktopWindowXamlSource
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private set;
    }

    internal Rect WindowSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            RECT rect;
            PInvoke.GetWindowRect(HWnd, &rect);
            return new(rect.X, rect.Y, rect.Width, rect.Height);
        }
    }

    internal double XamlIslandRasterizationScale
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => DesktopWindowXamlSource?.SiteBridge.SiteView.RasterizationScale ?? 1.0D;
    }

    internal event EventHandler? WindowInactivated;

    internal XamlIslandHostWindow()
    {
        _wndProc = new(WndProc);

        WNDCLASSW wndClass = default;
        wndClass.lpfnWndProc = _wndProc;
        wndClass.hInstance = (HINSTANCE)(void*)PInvoke.GetModuleHandle((PCWSTR)(void*)null);
        wndClass.lpszClassName = (PCWSTR)Unsafe.AsPointer(ref Unsafe.AsRef(in WindowClassName.GetPinnableReference()));
        PInvoke.RegisterClass(wndClass);

        HWnd = PInvoke.CreateWindowEx(
            WINDOW_EX_STYLE.WS_EX_NOREDIRECTIONBITMAP | WINDOW_EX_STYLE.WS_EX_TOOLWINDOW | WINDOW_EX_STYLE.WS_EX_TOPMOST,
            (PCWSTR)Unsafe.AsPointer(ref Unsafe.AsRef(in WindowClassName.GetPinnableReference())),
            (PCWSTR)Unsafe.AsPointer(ref Unsafe.AsRef(in WindowName.GetPinnableReference())),
            WINDOW_STYLE.WS_POPUP,
            0,
            0,
            0,
            0,
            HWND.Null,
            HMENU.Null,
            wndClass.hInstance,
            null
        );

        InitializeDesktopWindowXamlSource();
    }

    internal void SetContent(UIElement content)
    {
        DesktopWindowXamlSource!.Content = content;
    }

    internal void MoveAndResize(RectInt32 rect)
    {
        PInvoke.SetWindowPos(HWnd, HWND.HWND_TOP, rect.X, rect.Y, rect.Width, rect.Height, 0U);
        PInvoke.SetWindowPos(_xamlHwnd, HWND.HWND_TOP, 0, 0, rect.Width, rect.Height, 0U);
    }

    internal void Maximize()
    {
        var bottomRightPoint = WindowHelpers.GetBottomRightCornerPoint();
        PInvoke.SetWindowPos(HWnd, HWND.HWND_TOP, 0, 0, bottomRightPoint.X, bottomRightPoint.Y, 0U);
        PInvoke.SetWindowPos(_xamlHwnd, HWND.HWND_TOP, 0, 0, bottomRightPoint.X, bottomRightPoint.Y, 0U);
    }

    internal void SetHWndRectRegion(RectInt32 rect)
    {
        var region = PInvoke.CreateRectRgn(rect.X, rect.Y, rect.Width, rect.Height);
        PInvoke.SetWindowRgn(HWnd, region, false);
        PInvoke.SetWindowRgn(_xamlHwnd, region, false);
    }

    internal void UpdateWindowVisibility(bool isVisible)
    {
        PInvoke.ShowWindow(HWnd, isVisible ? SHOW_WINDOW_CMD.SW_SHOW : SHOW_WINDOW_CMD.SW_HIDE);
        if (isVisible)
        {
            DesktopWindowXamlSource?.SiteBridge.Show();
        }
        else
        {
            DesktopWindowXamlSource?.SiteBridge.Hide();
        }
    }

    private void InitializeDesktopWindowXamlSource()
    {
        DesktopWindowXamlSource = new();
        DesktopWindowXamlSource.Initialize(Win32Interop.GetWindowIdFromWindow(HWnd));
        _xamlHwnd = (HWND)(nint)DesktopWindowXamlSource.SiteBridge.WindowId.Value;
    }

    private LRESULT WndProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam)
    {
        if (uMsg == PInvoke.WM_ACTIVATE && ((int)wParam.Value & 0xFFFF) == PInvoke.WA_INACTIVE)
        {
            WindowInactivated?.Invoke(this, EventArgs.Empty);
        }

        return PInvoke.DefWindowProc(hWnd, uMsg, wParam, lParam);
    }

    public void Dispose()
    {
        PInvoke.DestroyWindow(HWnd);
        PInvoke.UnregisterClass(WindowClassName, PInvoke.GetModuleHandle(null));
        DesktopWindowXamlSource?.Dispose();
        DesktopWindowXamlSource = null;
    }
}
