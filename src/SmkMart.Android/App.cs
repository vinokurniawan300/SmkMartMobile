namespace SmkMart.Mobile;

public class App : Application
{
    private readonly DataStore? _store;
    private readonly string? _galat;

    public App()
    {
        UserAppTheme = AppTheme.Light;

        try
        {
            _store = new DataStore(Path.Combine(FileSystem.AppDataDirectory, "SmkMart"));
        }
        catch (Exception ex)
        {
            _galat = ex.Message;
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        if (_store == null)
        {
            return new Window(new ContentPage
            {
                Content = new Label
                {
                    Text = "Tidak dapat menyiapkan folder data:\n" + _galat,
                    Margin = new Thickness(24),
                    VerticalOptions = LayoutOptions.Center
                }
            });
        }

        return new Window(new NavigationPage(new LoginPage(_store)));
    }
}
