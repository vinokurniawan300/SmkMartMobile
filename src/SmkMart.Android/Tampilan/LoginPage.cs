namespace SmkMart.Mobile;

public class LoginPage : ContentPage
{
    private readonly DataStore _store;
    private readonly Entry _username = Ui.Isian("Username");
    private readonly Entry _password = Ui.Isian("Password");
    private readonly Label _galat = Ui.Lbl("", warna: Tema.Bahaya);

    public LoginPage(DataStore store)
    {
        _store = store;
        BackgroundColor = Tema.Latar;
        NavigationPage.SetHasNavigationBar(this, false);

        _username.Keyboard = Keyboard.Plain;
        _password.IsPassword = true;
        _password.Completed += async (s, e) => await Masuk();

        var judul = new VerticalStackLayout
        {
            BackgroundColor = Tema.UtamaGelap,
            Children =
            {
                new Label
                {
                    Text = "SMK MART",
                    FontSize = 30,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Tema.EmasMuda,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 48, 0, 4)
                },
                new Label
                {
                    Text = "Aplikasi Kasir dan Gudang",
                    TextColor = Tema.TeksPadaGelap,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 24)
                },
                new BoxView { HeightRequest = 3, Color = Tema.Emas }
            }
        };

        var form = new VerticalStackLayout
        {
            Spacing = 12,
            Padding = new Thickness(24, 24),
            Children =
            {
                Ui.Baris("Username", _username),
                Ui.Baris("Password", _password),
                _galat,
                Ui.TombolAsync("Masuk", Masuk, utama: true)
            }
        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout { Children = { judul, form } }
        };
    }

    private async Task Masuk()
    {
        var u = _store.Login(_username.Text ?? "", _password.Text ?? "");
        if (u == null)
        {
            _galat.Text = "Username atau password salah!";
            _password.Text = "";
            _password.Focus();
            return;
        }

        _galat.Text = "";
        _password.Text = "";
        var jendela = Ui.Jendela;
        if (jendela != null) jendela.Page = new NavigationPage(new MainPage(_store, u));
        await Task.CompletedTask;
    }
}
