using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia;
using Avalonia.Android;
using MoneyBud.Prototype.Platform;
using MoneyBud.Prototype.Themes;

[assembly: UsesPermission(Android.Manifest.Permission.Vibrate)]

namespace MoneyBud.Prototype.Android;

[Application]
public sealed class PrototypeApplication(IntPtr handle, global::Android.Runtime.JniHandleOwnership ownership)
    : AvaloniaAndroidApplication<App>(handle, ownership)
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder).WithInterFont()
            // Kintsugi keeps several screen-sized pictures on the GPU (the table, the slabs, the
            // plate's textures). Avalonia's default budget for them is smaller than that, and a
            // picture pushed out of it is sent again every frame.
            .With(new SkiaOptions { MaxGpuResourceSizeBytes = 256L * 1024 * 1024 });
}

[Activity(
    Label = "MoneyBud",
    Theme = "@style/MoneyBudTheme",
    MainLauncher = true,
    ScreenOrientation = ScreenOrientation.Portrait,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public sealed class MainActivity : AvaloniaMainActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // For trying a theme from the PC: adb shell am start -n <activity> --es look Kintsugi
        if (Intent?.GetStringExtra("look") is { } name && Looks.All.FirstOrDefault(l => l.Name == name) is { } look)
        {
            Looks.Use(look);
        }

        var vibrator = ((VibratorManager?)GetSystemService(VibratorManagerService))?.DefaultVibrator;
        Haptics.Tick = () => vibrator?.Vibrate(VibrationEffect.CreatePredefined(VibrationEffect.EffectTick));
    }
}
