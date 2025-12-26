using PicView.Core.Navigation.Interfaces;

namespace PicView.Core.ViewModels;

public class SharedNavigationViewModel
{
    public INavigationService? NavigationService { get; private set; }
    public IImageCache? SharedCache { get; private set; }

    public void Initialize(INavigationService navigationService, IImageCache cache)
    {
        if (navigationService is null)
        {
            return;
        }
        NavigationService = navigationService;
        SharedCache = cache;
    }
        
}