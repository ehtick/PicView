using System.Reactive;
using ReactiveUI;

namespace PicView.Core.ViewModels;


public class SettingsViewModel : ReactiveObject
{
    public bool IsBackButtonEnabled
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsForwardButtonEnabled
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    public ReactiveCommand<Unit, Unit>? GoForwardCommand
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ReactiveCommand<Unit, Unit>? GoBackCommand
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
}