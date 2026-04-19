using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using PicView.Avalonia.ColorManagement;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.ColorHandling;
using R3;

namespace PicView.Avalonia.Views.Config;

public partial class AppearanceView : UserControl
{
    private readonly CompositeDisposable _disposables = new();
    
    private const string ActiveColorBtnClassName = "activeColorBtn";
    private const string ActiveColorAccentBtnClassName = "activeColorAccentBtn";
    public AppearanceView()
    {
        InitializeComponent();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            RestartButton.IsEnabled = false;
        }
        Loaded += AppearanceView_Loaded;
    }

    private void AppearanceView_Loaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }
        GalleryStretchMode.DetermineStretchMode(vm);

        if (Settings.Theme.GlassTheme)
        {
            ThemeBox.SelectedItem = GlassThemeBox;
        }
        else
        {
            ThemeBox.SelectedItem = Settings.Theme.Dark ? DarkThemeBox : LightThemeBox;
        }
        ThemeBox.SelectionChanged += delegate
        {
            // Adjust based on which theme is selected
            if (Equals(ThemeBox.SelectedItem, GlassThemeBox))
            {
                Settings.Theme.GlassTheme = true;
            }
            else if (Equals(ThemeBox.SelectedItem, DarkThemeBox))
            {
                Settings.Theme.GlassTheme = false;
                Settings.Theme.Dark = true;
            }
            else
            {
                Settings.Theme.GlassTheme = false;
                Settings.Theme.Dark = false;
            }

            var selectedTheme = Settings.Theme.GlassTheme
                ? ThemeManager.Theme.Glass
                : Settings.Theme.Dark
                    ? ThemeManager.Theme.Dark
                    : ThemeManager.Theme.Light;

            ThemeManager.SetTheme(selectedTheme);
        };

        // Set button colors dynamically from ColorManager
        UpdateColorButtons();
        
        // Set active color button based on current theme
        ClearColorButtonsActiveState();
        switch ((ColorOptions)Settings.Theme.ColorTheme)
        {
            case ColorOptions.Raspberry:
                RaspberryButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Teal:
                TealButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Emerald:
                EmeraldButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Golden:
                GoldButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Orange:
                OrangeButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Pink:
                PinkButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Purple:
                PurpleButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Red:
                RedButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Ruby:
                RubyButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Magenta:
                MagentaButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Blue:
                BlueButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Cyan:
                CyanButton.Classes.Add(ActiveColorBtnClassName);
                break;
        }
        
        CheckerboardButton.Background = BackgroundManager.CreateCheckerboardBrush(default, default,10);
        CheckerboardAltButton.Background = BackgroundManager.CreateCheckerboardBrushAlt(25);
    }
    
    // New method to update color buttons with values from ColorManager
    private void UpdateColorButtons()
    {
        BlueButton.Background = new SolidColorBrush(ColorManager.GetColor(Blue));
        CyanButton.Background = new SolidColorBrush(ColorManager.GetColor(Cyan));
        RaspberryButton.Background = new SolidColorBrush(ColorManager.GetColor(Raspberry));
        TealButton.Background = new SolidColorBrush(ColorManager.GetColor(Teal));
        EmeraldButton.Background = new SolidColorBrush(ColorManager.GetColor(Emerald));
        RubyButton.Background = new SolidColorBrush(ColorManager.GetColor(Ruby));
        GoldButton.Background = new SolidColorBrush(ColorManager.GetColor(Golden));
        OrangeButton.Background = new SolidColorBrush(ColorManager.GetColor(Orange));
        RedButton.Background = new SolidColorBrush(ColorManager.GetColor(Red));
        PinkButton.Background = new SolidColorBrush(ColorManager.GetColor(Pink));
        MagentaButton.Background = new SolidColorBrush(ColorManager.GetColor(Magenta));
        PurpleButton.Background = new SolidColorBrush(ColorManager.GetColor(Purple));
    }
    
    // Add constants for color theme indices for easier reference
    private const int Blue = 0;
    private const int Pink = 2;
    private const int Orange = 3;
    private const int Ruby = 4;
    private const int Red = 5;
    private const int Teal = 6;
    private const int Raspberry = 7;
    private const int Golden = 8;
    private const int Purple = 9;
    private const int Cyan = 10;
    private const int Magenta = 11;
    private const int Emerald = 12;

    private void ClearColorButtonsActiveState()
    {
        var buttons = new List<Button>
        {
            BlueButton, CyanButton, RubyButton, MagentaButton, RedButton, RaspberryButton,
            TealButton, EmeraldButton, GoldButton, OrangeButton, PinkButton, PurpleButton
        };

        foreach (var button in buttons)
        {
            button.Classes.Remove(ActiveColorBtnClassName);
        }
    }
    
    private void SetColorTheme(ColorOptions colorTheme)
    {
        ClearColorButtonsActiveState();
        switch (colorTheme)
        {
            default:
                BlueButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Pink:
                PinkButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Orange:
                OrangeButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Ruby:
                RubyButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Red:
                RedButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Teal:
                TealButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Raspberry:
                RaspberryButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Golden:
                GoldButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Purple:
                PurpleButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Cyan:
                CyanButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Magenta:
                MagentaButton.Classes.Add(ActiveColorBtnClassName);
                break;
            case ColorOptions.Emerald:
                EmeraldButton.Classes.Add(ActiveColorBtnClassName);
                break;
        }

        ColorManager.UpdateAccentColors((int)colorTheme);
    }

    private void ColorButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button clickedButton)
        {
            return;
        }

        // Map the button to the corresponding ColorOptions enum
        var selectedColor = clickedButton.Name switch
        {
            nameof(BlueButton) => ColorOptions.Blue,
            nameof(CyanButton) => ColorOptions.Cyan,
            nameof(RubyButton) => ColorOptions.Ruby,
            nameof(MagentaButton) => ColorOptions.Magenta,
            nameof(RedButton) => ColorOptions.Red,
            nameof(RaspberryButton) => ColorOptions.Raspberry,
            nameof(TealButton) => ColorOptions.Teal,
            nameof(EmeraldButton) => ColorOptions.Emerald,
            nameof(GoldButton) => ColorOptions.Golden,
            nameof(OrangeButton) => ColorOptions.Orange,
            nameof(PinkButton) => ColorOptions.Pink,
            nameof(PurpleButton) => ColorOptions.Purple,
            _ => ColorOptions.Blue
        };

        // Set the new active theme
        SetColorTheme(selectedColor);
    }
    
    private void BgButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button clickedButton)
        {
            return;
        }

        // Map the button to the corresponding ColorOptions enum
        var selectedBg = clickedButton.Name switch
        {
            nameof(TransparentBgButton) => 0,
            nameof(NoiseTextureButton) => 1,
            nameof(CheckerboardButton) => 2,
            nameof(CheckerboardAltButton) => 3,
            nameof(WhiteBgButton) => 4,
            nameof(GrayBgButton) => 5,
            nameof(MediumGrayBgButton) => 6,
            nameof(DarkGraySemiTransparentBgButton) => 7,
            nameof(DarkGraySemiTransparentAltBgButton) => 8,
            nameof(BlackBgButton) => 9,
            nameof(DarkGrayBgButton) => 10,
            _ => 0
        };

        // Set the new active theme
        SetBackgroundTheme(selectedBg);
    }
    
    private void SetBackgroundTheme(int selectedBg)
    {
        ClearBackgroundButtonsActiveState();
        switch (selectedBg)
        {
            default:
                TransparentBgButton.Classes.Add(ActiveColorAccentBtnClassName);
                break;
            case 1:
                NoiseTextureButton.Classes.Add(ActiveColorAccentBtnClassName);
                break;
            case 2:
                CheckerboardButton.Classes.Add(ActiveColorAccentBtnClassName);
                break;
            case 3:
                CheckerboardAltButton.Classes.Add(ActiveColorAccentBtnClassName);
                break;
            case 4:
                WhiteBgButton.Classes.Add(ActiveColorAccentBtnClassName);
                break;
            case 5:
                GrayBgButton.Classes.Add(ActiveColorAccentBtnClassName);
                break;
            case 6:
                MediumGrayBgButton.Classes.Add(ActiveColorAccentBtnClassName);
                break;
            case 7:
                DarkGraySemiTransparentBgButton.Classes.Add(ActiveColorAccentBtnClassName);
                break;
            case 8:
                DarkGraySemiTransparentAltBgButton.Classes.Add(ActiveColorAccentBtnClassName);
                break;
            case 9:
                BlackBgButton.Classes.Add(ActiveColorAccentBtnClassName);
                break;
            case 10:
                DarkGrayBgButton.Classes.Add(ActiveColorAccentBtnClassName);
                break;
        }

        if (DataContext is not MainViewModel vm)
        {
            return;
        }
        
        BackgroundManager.SetBackground(vm, selectedBg);
    }

    private void ClearBackgroundButtonsActiveState()
    {
        var buttons = new List<Button>
        {
            TransparentBgButton, NoiseTextureButton, CheckerboardButton, CheckerboardAltButton,
            WhiteBgButton, GrayBgButton, DarkGrayBgButton, DarkGraySemiTransparentBgButton,
            DarkGraySemiTransparentAltBgButton, BlackBgButton, MediumGrayBgButton
        };

        foreach (var button in buttons)
        {
            button.Classes.Remove(ActiveColorAccentBtnClassName);
        }
    }
}