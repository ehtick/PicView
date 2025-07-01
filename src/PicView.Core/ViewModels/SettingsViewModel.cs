using R3;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace PicView.Core.ViewModels;

public class SettingsViewModel : IDisposable
{
    public BindableReactiveProperty<double> WindowMargin { get; } = new();

    public void Dispose()
    {
        Disposable.Dispose(IsBackButtonEnabled, IsForwardButtonEnabled, GoForwardCommand, GoBackCommand);
        Disposable.Dispose(IsShowingRecycleDialog, IsShowingPermanentDeletionDialog, WindowMargin);
    }

    #region Tab history navigation

    public BindableReactiveProperty<bool> IsBackButtonEnabled { get; } = new();

    public BindableReactiveProperty<bool> IsForwardButtonEnabled { get; } = new();
    public ReactiveCommand? GoForwardCommand { get; set; }

    public ReactiveCommand? GoBackCommand { get; set; }

    public void InitializeNavigation (Action goBack, Action goForward)
    {
        GoForwardCommand = IsForwardButtonEnabled.ToReactiveCommand(_ => { goForward(); });
        GoBackCommand = IsBackButtonEnabled.ToReactiveCommand(_ => { goBack(); });
    }

    #endregion

    #region UI

    public BindableReactiveProperty<bool> IsShowingRecycleDialog { get; } =
        new(Settings.UIProperties.ShowRecycleConfirmation);

    public BindableReactiveProperty<bool> IsShowingPermanentDeletionDialog { get; } =
        new(Settings.UIProperties.ShowPermanentDeletionConfirmation);

    #endregion
}