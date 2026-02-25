using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace MoreFlyout.Server.Utils;

internal class SimulationKeyboard
{
    public static void ToggleNumberLock()
    {
        var inputs = new[]
        {
            // Press
            new INPUT
            {
                type = INPUT_TYPE.INPUT_KEYBOARD,
                Anonymous = new INPUT._Anonymous_e__Union { ki = new KEYBDINPUT { wVk = VIRTUAL_KEY.VK_NUMLOCK } },
            },
            // Release
            new INPUT
            {
                type = INPUT_TYPE.INPUT_KEYBOARD,
                Anonymous = new INPUT._Anonymous_e__Union
                {
                    ki = new KEYBDINPUT { wVk = VIRTUAL_KEY.VK_NUMLOCK, dwFlags = KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP },
                },
            },
        };
        PInvoke.SendInput(inputs, System.Runtime.InteropServices.Marshal.SizeOf<INPUT>());
    }

    public static void ToggleCapsLock()
    {
        var inputs = new[]
        {
            // Press
            new INPUT
            {
                type = INPUT_TYPE.INPUT_KEYBOARD,
                Anonymous = new INPUT._Anonymous_e__Union { ki = new KEYBDINPUT { wVk = VIRTUAL_KEY.VK_CAPITAL } },
            },
            // Release
            new INPUT
            {
                type = INPUT_TYPE.INPUT_KEYBOARD,
                Anonymous = new INPUT._Anonymous_e__Union
                {
                    ki = new KEYBDINPUT { wVk = VIRTUAL_KEY.VK_CAPITAL, dwFlags = KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP },
                },
            },
        };
        PInvoke.SendInput(inputs, System.Runtime.InteropServices.Marshal.SizeOf<INPUT>());
    }

    public static void MediaTogglePlayPause()
    {
        var inputs = new[]
        {
            // Press
            new INPUT
            {
                type = INPUT_TYPE.INPUT_KEYBOARD,
                Anonymous = new INPUT._Anonymous_e__Union { ki = new KEYBDINPUT { wVk = VIRTUAL_KEY.VK_MEDIA_PLAY_PAUSE } },
            },
            // Release
            new INPUT
            {
                type = INPUT_TYPE.INPUT_KEYBOARD,
                Anonymous = new INPUT._Anonymous_e__Union
                {
                    ki = new KEYBDINPUT { wVk = VIRTUAL_KEY.VK_MEDIA_PLAY_PAUSE, dwFlags = KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP },
                },
            },
        };

        PInvoke.SendInput(inputs, System.Runtime.InteropServices.Marshal.SizeOf<INPUT>());
    }

    public static void MediaPrevious()
    {
        var inputs = new[]
        {
            // Press
            new INPUT
            {
                type = INPUT_TYPE.INPUT_KEYBOARD,
                Anonymous = new INPUT._Anonymous_e__Union { ki = new KEYBDINPUT { wVk = VIRTUAL_KEY.VK_MEDIA_PREV_TRACK } },
            },
            // Release
            new INPUT
            {
                type = INPUT_TYPE.INPUT_KEYBOARD,
                Anonymous = new INPUT._Anonymous_e__Union
                {
                    ki = new KEYBDINPUT { wVk = VIRTUAL_KEY.VK_MEDIA_PREV_TRACK, dwFlags = KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP },
                },
            },
        };

        PInvoke.SendInput(inputs, System.Runtime.InteropServices.Marshal.SizeOf<INPUT>());
    }

    public static void MediaNext()
    {
        var inputs = new[]
        {
            // Press
            new INPUT
            {
                type = INPUT_TYPE.INPUT_KEYBOARD,
                Anonymous = new INPUT._Anonymous_e__Union { ki = new KEYBDINPUT { wVk = VIRTUAL_KEY.VK_MEDIA_NEXT_TRACK } },
            },
            // Release
            new INPUT
            {
                type = INPUT_TYPE.INPUT_KEYBOARD,
                Anonymous = new INPUT._Anonymous_e__Union
                {
                    ki = new KEYBDINPUT { wVk = VIRTUAL_KEY.VK_MEDIA_NEXT_TRACK, dwFlags = KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP },
                },
            },
        };

        PInvoke.SendInput(inputs, System.Runtime.InteropServices.Marshal.SizeOf<INPUT>());
    }
}