using System.Reactive;
using ReactiveUI;

namespace PicView.Core.ViewModels;


public class SettingsViewModel : ReactiveObject
{

    #region Tab history navigation
    
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
    
    #endregion

    #region UI
    
    public bool IsShowingRecycleDialog
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            Settings.UIProperties.ShowRecycleConfirmation = value;
        } 
    } = Settings.UIProperties.ShowRecycleConfirmation;

    public bool IsShowingPermanentDeletionDialog
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            Settings.UIProperties.ShowPermanentDeletionConfirmation = value;
        }
    } = Settings.UIProperties.ShowPermanentDeletionConfirmation;

    #endregion
}