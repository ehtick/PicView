namespace PicView.Core.Config;

public class SettingsWindowConfig
{
    public const string ConfigFileName = "SettingsWindow.json";
    
    public class WindowProperties
    {
        public double Top { get; set; } = 0;
        public double Left { get; set; } = 0;
        public double Width { get; set; } = 677;
        public double Height { get; set; } = 750;
        public bool Maximized { get; set; } = false;
        public bool Fullscreen { get; set; } = false;
    }
}