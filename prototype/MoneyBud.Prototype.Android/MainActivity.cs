using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia;
using Avalonia.Android;
using MoneyBud.Prototype.Platform;

[assembly: UsesPermission(Android.Manifest.Permission.Vibrate)]

namespace MoneyBud.Prototype.Android;

[Application]
public sealed class PrototypeApplication(IntPtr handle, global::Android.Runtime.JniHandleOwnership ownership)
    : AvaloniaAndroidApplication<App>(handle, ownership)
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder).WithInterFont();
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
        var vibrator = ((VibratorManager?)GetSystemService(VibratorManagerService))?.DefaultVibrator;
        Haptics.Tick = () => vibrator?.Vibrate(VibrationEffect.CreatePredefined(VibrationEffect.EffectTick));
    }
}
