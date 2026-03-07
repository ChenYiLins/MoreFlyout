// Copyright (c) 0x5BFA. All rights reserved.
// Licensed under the MIT license.
using Microsoft.UI.Composition.SystemBackdrops;
using Windows.UI;

namespace MoreFlyout.Server.Helpers;

internal static class BackdropControllerHelpers
{
    internal static ISystemBackdropControllerWithTargets? CreateController(BackdropKind kind, bool isLight, bool isAccented, ResourceDictionary resources)
    {
        Color fallback,
            tint;
        float luminosity,
            tintOpacity;

        if (isAccented)
        {
            if (!resources.TryGetValue("SystemAccentColorDark2", out var raw) || raw is not Color accentColor)
            {
                return null;
            }
            fallback = tint = accentColor;
            luminosity = 0.8F;
            tintOpacity = 0.8F;
        }
        else if (isLight)
        {
            fallback = Color.FromArgb(0xFF, 0xEE, 0xEE, 0xEE);
            tint = Color.FromArgb(0xFF, 0xF3, 0xF3, 0xF3);
            luminosity = 0.9F;
            tintOpacity = 0.0F;
        }
        else
        {
            fallback = Color.FromArgb(0xFF, 0x2C, 0x2C, 0x2C);
            tint = Color.FromArgb(0xFF, 0x2C, 0x2C, 0x2C);
            luminosity = 0.96F;
            tintOpacity = 0.15F;
        }

        return kind is BackdropKind.Acrylic
            ? new DesktopAcrylicController
            {
                FallbackColor = fallback,
                LuminosityOpacity = luminosity,
                TintColor = tint,
                TintOpacity = tintOpacity,
            }
            : new MicaController
            {
                FallbackColor = fallback,
                LuminosityOpacity = luminosity,
                TintColor = tint,
                TintOpacity = tintOpacity,
            };
    }
}
