namespace SmkMart.Mobile;

public class PenggunaPanel : PanelDasar
{
    private readonly DataStore _store;
    private readonly Pengguna _user;
    private readonly CollectionView _daftar;

    public PenggunaPanel(DataStore store, Pengguna user)
    {
        _store = store;
        _user = user;

        _daftar = Ui.DaftarItem<Pengguna>(KartuPengguna, "Belum ada pengguna.");
        _daftar.Margin = new Thickness(12, 4);

        var atas = new VerticalStackLayout
        {
            Padding = new Thickness(12, 10, 12, 0),
            Spacing = 4,
            Children =
            {
                Ui.TombolAsync("+ Tambah Pengguna", Tambah, utama: true),
                Ui.Lbl("Ketuk pengguna untuk mengubah atau menghapus.", ukuran: 12, warna: Tema.TeksRedup)
            }
        };

        var grid = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) }
        };
        grid.Add(atas, 0, 0);
        grid.Add(_daftar, 0, 1);
        Content = grid;
    }

    public override void Segarkan() => _daftar.ItemsSource = _store.SemuaPengguna();

    private View KartuPengguna(Pengguna u)
    {
        var isi = new VerticalStackLayout
        {
            Spacing = 2,
            Children =
            {
                Ui.Lbl(u.Nama, tebal: true, ukuran: 15),
                Ui.Lbl($"{u.Username}  •  {u.Role}", ukuran: 12, warna: Tema.TeksRedup)
            }
        };
        var kartu = Ui.Kartu(isi);
        Ui.Ketuk(kartu, () => _ = Aksi(u));
        return kartu;
    }

    private async Task Tambah()
    {
        if (await Ui.Tampilkan(new UserDialog(_store))) Beritahu();
    }

    private async Task Aksi(Pengguna u)
    {
        var pilih = await Ui.Pilih(u.Nama, "Ubah", "Hapus");
        if (pilih == "Ubah")
        {
            if (await Ui.Tampilkan(new UserDialog(_store, u))) Beritahu();
        }
        else if (pilih == "Hapus")
        {
            if (!await Ui.Konfirmasi($"Yakin menghapus pengguna '{u.Nama}'?", "Hapus")) return;
            if (await Ui.Coba(() => _store.HapusPengguna(u.Username, _user.Username))) Beritahu();
        }
    }
}
