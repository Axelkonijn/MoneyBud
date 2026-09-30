using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;

namespace MoneyBud.Phone.Android;

/// <summary>
/// MoneyBud as an Android app. It tells the screens where the data and the settings live, and
/// gives them the phone's tick in the hand, before anything starts.
/// </summary>
[Application]
public sealed class MoneyBudApplication(IntPtr handle, JniHandleOwnership ownership)
    : AvaloniaAndroidApplication<App>(handle, ownership)
{
    public override void OnCreate()
    {
        PhoneHost.Current = new PhoneHost
        {
            // The app's own folder on shared storage, Android/data/app.moneybud/files: where Axel
            // copies the data from over USB, and never anywhere he has to choose (ADR 0014, decision 3).
            // Empty when Android cannot give it; MoneyBud then says it cannot read the data.
            DataFolder = GetExternalFilesDir(null)?.AbsolutePath ?? string.Empty,

            // The app's private folder, which the USB folder does not show: the settings are about the
            // phone, not the money, and are not in the data file (plan for increment 14, D3).
            SettingsFolder = FilesDir?.AbsolutePath ?? string.Empty,

            Tick = Haptics.Tick,
            Quit = MainActivity.Quit,
        };

        base.OnCreate();
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder).WithInterFont()
            // Kintsugi keeps several screen-sized pictures on the GPU (the table, the slabs, the
            // plate's textures). Avalonia's default budget for them is smaller than that, and a
            // picture pushed out of it is sent again every frame (arc42 §8.5).
            .With(new SkiaOptions { MaxGpuResourceSizeBytes = 256L * 1024 * 1024 });
}

[Activity(
    Label = "MoneyBud",
    Theme = "@style/MoneyBudTheme",
    MainLauncher = true,
    ScreenOrientation = ScreenOrientation.Portrait,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode
        | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Keyboard
        | ConfigChanges.KeyboardHidden | ConfigChanges.Density)]
public sealed class MainActivity : AvaloniaMainActivity
{
    private static MainActivity? current;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        current = this;
        base.OnCreate(savedInstanceState);
        Haptics.Use(this);
    }

    // Android stops an app in the background and may end it there without warning, so going to the
    // background is the last moment MoneyBud can count on (arc42 §12, Android's lifecycle).
    protected override void OnPause()
    {
        PhoneHost.Current.GoToBackground();
        base.OnPause();
    }

    protected override void OnResume()
    {
        base.OnResume();
        PhoneHost.Current.ComeBack();
    }

    /// <summary>
    /// After the one thing MoneyBud says when it cannot read its data: the app closes, all of it, so
    /// the next start reads the data afresh (§12, <i>Android's lifecycle</i>).
    /// </summary>
    public static void Quit()
    {
        current?.FinishAndRemoveTask();
        Process.KillProcess(Process.MyPid());
    }
}

/// <summary>
/// A light tick in the hand when the finger passes to the next slice. It follows the phone's own
/// setting for touch feedback: Android drops it when that is off (§12, <i>Touching the ring</i>).
/// </summary>
internal static class Haptics
{
    private static Vibrator? vibrator;

    public static void Use(Activity activity) =>
        vibrator = ((VibratorManager?)activity.GetSystemService(Activity.VibratorManagerService))?.DefaultVibrator;

    public static void Tick() => vibrator?.Vibrate(VibrationEffect.CreatePredefined(VibrationEffect.EffectTick));
}
