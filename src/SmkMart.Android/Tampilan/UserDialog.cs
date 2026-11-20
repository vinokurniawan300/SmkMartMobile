namespace SmkMart.Mobile;

public class UserDialog : DialogHalaman
{
    private readonly DataStore _store;
    private readonly Pengguna? _ubah;
    private readonly Entry _username = Ui.Isian("Username");
    private readonly Entry _password = Ui.Isian("Password");
    private readonly CheckBox _tampil = new() { Color = Tema.Utama };
    private readonly Entry _nama = Ui.Isian("Nama lengkap");
    private readonly Picker _role = Ui.Daftar("Role");

    public UserDialog(DataStore store, Pengguna? ubah = null)
        : base(ubah == null ? "Tambah Pengguna" : "Ubah Pengguna")
    {
        _store = store;
        _ubah = ubah;

        _username.Keyboard = Keyboard.Plain;
        _password.IsPassword = true;
        _tampil.CheckedChanged += (s, e) => _password.IsPassword = !e.Value;

        _role.ItemsSource = Peran.Semua;
        _role.SelectedItem = Peran.Kasir;

        if (ubah != null)
        {
            _username.Text = ubah.Username;
            _username.IsReadOnly = true;
            _nama.Text = ubah.Nama;
            _role.SelectedItem = ubah.Role;
            _role.IsEnabled = false;
            _password.Placeholder = "(kosongkan jika tidak diubah)";
        }

        var tampilBaris = new HorizontalStackLayout
        {
            Spacing = 6,
            Children = { _tampil, new Label { Text = "Tampilkan password", VerticalOptions = LayoutOptions.Center, TextColor = Tema.Teks } }
        };

        Isi(
            Ui.Baris("Username", _username),
            Ui.Baris("Password", _password),
            tampilBaris,
            Ui.Baris("Nama lengkap", _nama),
            Ui.Baris("Role", _role));
    }

    protected override Task<bool> Simpan() => Ui.Coba(() =>
    {
        if (_ubah == null)
            _store.TambahPengguna(_username.Text ?? "", _password.Text ?? "", _nama.Text ?? "", _role.SelectedItem as string ?? "");
        else
            _store.UbahPengguna(_ubah.Username, _nama.Text ?? "", _password.Text ?? "");
    });
}
