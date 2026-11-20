namespace SmkMart.Mobile;

public class GudangPanel : PanelDasar
{
    private readonly DataStore _store;
    private readonly Pengguna _user;
    private readonly CollectionView _daftarMasuk;
    private readonly CollectionView _daftarKeluar;
    private readonly ContentView _isi = new();
    private readonly Button _tabMasuk;
    private readonly Button _tabKeluar;

    public GudangPanel(DataStore store, Pengguna user)
    {
        _store = store;
        _user = user;

        _daftarMasuk = Ui.DaftarItem<BarangMasuk>(KartuMasuk, "Belum ada riwayat barang masuk.");
        _daftarKeluar = Ui.DaftarItem<BarangKeluar>(KartuKeluar, "Belum ada riwayat barang keluar.");
        _daftarMasuk.Margin = new Thickness(12, 4);
        _daftarKeluar.Margin = new Thickness(12, 4);

        _tabMasuk = Ui.Tombol("Riwayat Masuk", () => Pilih(true));
        _tabKeluar = Ui.Tombol("Riwayat Keluar", () => Pilih(false));

        var tombolAksi = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 8
        };
        tombolAksi.Add(Ui.TombolAsync("+ Barang Masuk", Masuk, utama: true), 0, 0);
        tombolAksi.Add(Ui.TombolAsync("+ Barang Keluar", Keluar, emas: true), 1, 0);

        var tab = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 8
        };
        tab.Add(_tabMasuk, 0, 0);
        tab.Add(_tabKeluar, 1, 0);

        var atas = new VerticalStackLayout
        {
            Spacing = 8,
            Padding = new Thickness(12, 10, 12, 0),
            Children =
            {
                tombolAksi,
                tab,
                Ui.Lbl("Ketuk salah satu riwayat untuk mengubahnya.", ukuran: 12, warna: Tema.TeksRedup)
            }
        };

        var grid = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) }
        };
        grid.Add(atas, 0, 0);
        grid.Add(_isi, 0, 1);
        Content = grid;

        Pilih(true);
    }

    private void Pilih(bool masuk)
    {
        _isi.Content = masuk ? _daftarMasuk : _daftarKeluar;
        Warnai(_tabMasuk, masuk);
        Warnai(_tabKeluar, !masuk);
    }

    private static void Warnai(Button b, bool aktif)
    {
        b.BackgroundColor = aktif ? Tema.Utama : Tema.Putih;
        b.TextColor = aktif ? Tema.Putih : Tema.Utama;
    }

    public override void Segarkan()
    {
        _daftarMasuk.ItemsSource = _store.SemuaMasuk().OrderByDescending(x => x.No).ToList();
        _daftarKeluar.ItemsSource = _store.SemuaKeluar().OrderByDescending(x => x.No).ToList();
    }

    private View KartuMasuk(BarangMasuk m)
    {
        var isi = new VerticalStackLayout
        {
            Spacing = 2,
            Children =
            {
                Ui.Lbl($"{m.Nama}  (+{m.Jumlah})", tebal: true, ukuran: 15),
                Ui.Lbl($"{m.No}  •  {m.Tanggal}  •  {m.Kode}", ukuran: 12, warna: Tema.TeksRedup),
                Ui.Lbl($"Supplier: {m.Supplier}  |  {Fmt.Rupiah(m.HargaSupplier)}/unit", ukuran: 13),
                Ui.Lbl($"Petugas: {m.Petugas}" + (m.Keterangan != "" ? $"  |  {m.Keterangan}" : ""), ukuran: 12, warna: Tema.TeksRedup)
            }
        };
        var kartu = Ui.Kartu(isi);
        Ui.Ketuk(kartu, () => _ = UbahMasuk(m));
        return kartu;
    }

    private View KartuKeluar(BarangKeluar k)
    {
        var isi = new VerticalStackLayout
        {
            Spacing = 2,
            Children =
            {
                Ui.Lbl($"{k.Nama}  (-{k.Jumlah})", tebal: true, ukuran: 15),
                Ui.Lbl($"{k.No}  •  {k.Tanggal}  •  {k.Kode}", ukuran: 12, warna: Tema.TeksRedup),
                Ui.Lbl($"Jenis: {k.Jenis}", ukuran: 13),
                Ui.Lbl($"Petugas: {k.Petugas}" + (k.Keterangan != "" ? $"  |  {k.Keterangan}" : ""), ukuran: 12, warna: Tema.TeksRedup)
            }
        };
        var kartu = Ui.Kartu(isi);
        Ui.Ketuk(kartu, () => _ = UbahKeluar(k));
        return kartu;
    }

    private async Task Masuk()
    {
        var d = new MasukDialog(_store, _user.Nama);
        await Ui.Tampilkan(d);
        if (d.AdaPerubahan) Beritahu();
    }

    private async Task Keluar()
    {
        var d = new KeluarDialog(_store, _user.Nama);
        await Ui.Tampilkan(d);
        if (d.AdaPerubahan) Beritahu();
    }

    private async Task UbahMasuk(BarangMasuk m)
    {
        var d = new UbahMasukDialog(_store, m);
        await Ui.Tampilkan(d);
        if (d.AdaPerubahan) Beritahu();
    }

    private async Task UbahKeluar(BarangKeluar k)
    {
        var d = new UbahKeluarDialog(_store, k);
        await Ui.Tampilkan(d);
        if (d.AdaPerubahan) Beritahu();
    }
}
