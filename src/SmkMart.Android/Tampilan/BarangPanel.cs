namespace SmkMart.Mobile;

public class BarangPanel : PanelDasar
{
    private readonly DataStore _store;
    private readonly bool _bisaUbah;
    private readonly Entry _cari = Ui.Isian("Cari kode / nama / kategori...");
    private readonly Picker _urut = Ui.Daftar("Urutkan");
    private readonly CollectionView _daftar;

    public BarangPanel(DataStore store, bool bisaUbah)
    {
        _store = store;
        _bisaUbah = bisaUbah;

        _urut.ItemsSource = new[]
        {
            "Nama (A-Z)", "Harga jual (termurah)", "Harga jual (termahal)",
            "Stok (paling sedikit)", "Stok (paling banyak)"
        };
        _urut.SelectedIndex = 0;
        _cari.TextChanged += (s, e) => Segarkan();
        _urut.SelectedIndexChanged += (s, e) => Segarkan();

        _daftar = Ui.DaftarItem<Barang>(b => Ui.KartuBarang(b, true, bisaUbah ? b2 => _ = Aksi(b2) : null),
            "Belum ada barang.");

        var atas = new VerticalStackLayout { Spacing = 8, Padding = new Thickness(12, 10, 12, 4) };
        atas.Add(_cari);
        atas.Add(_urut);
        if (bisaUbah) atas.Add(Ui.TombolAsync("+ Tambah Barang", Tambah, utama: true));

        var grid = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) }
        };
        grid.Add(atas, 0, 0);
        _daftar.Margin = new Thickness(12, 4);
        grid.Add(_daftar, 0, 1);
        Content = grid;
    }

    public override void Segarkan()
    {
        IEnumerable<Barang> q = Ui.CariBarang(_store.SemuaBarang(), _cari.Text ?? "");
        q = _urut.SelectedIndex switch
        {
            1 => q.OrderBy(b => b.HargaJual),
            2 => q.OrderByDescending(b => b.HargaJual),
            3 => q.OrderBy(b => b.Stok),
            4 => q.OrderByDescending(b => b.Stok),
            _ => q.OrderBy(b => b.Nama, StringComparer.OrdinalIgnoreCase)
        };
        _daftar.ItemsSource = q.ToList();
    }

    private async Task Tambah()
    {
        if (await Ui.Tampilkan(new BarangDialog(_store))) Beritahu();
    }

    private async Task Aksi(Barang b)
    {
        var pilih = await Ui.Pilih(b.Nama, "Ubah", "Hapus");
        if (pilih == "Ubah")
        {
            if (await Ui.Tampilkan(new BarangDialog(_store, b))) Beritahu();
        }
        else if (pilih == "Hapus")
        {
            if (!await Ui.Konfirmasi($"Yakin menghapus '{b.Nama}'?", "Hapus")) return;
            if (await Ui.Coba(() => _store.HapusBarang(b.Kode))) Beritahu();
        }
    }
}
