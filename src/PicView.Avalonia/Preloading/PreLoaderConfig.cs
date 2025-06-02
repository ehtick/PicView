namespace PicView.Avalonia.Preloading;

public class PreLoaderConfig
{
    public static int PositiveIterations => Settings.Navigation.PositiveIterations;
    public static int NegativeIterations => Settings.Navigation.NegativeIterations;
    public static int MaxCount => PositiveIterations + NegativeIterations + 2;
    public int MaxParallelism { get; } = Math.Max(1, Environment.ProcessorCount - 3);
}