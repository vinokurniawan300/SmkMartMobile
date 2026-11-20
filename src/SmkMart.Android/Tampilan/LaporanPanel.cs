namespace SmkMart.Mobile;

public record BarisPenjualan(string No, string Tanggal, string Kasir, int JumlahItem, int Total);

public record BarisRiwayatHarga(string Tanggal, string Barang, string HargaBeli, string HargaJual, string Margin, string Keterangan, int Selisih);

public class LaporanPanel : PanelDasar
{
    private readonly DataStore _store;
    private readonly bool _penjualan;
    private readonly bool _stok;

    private readonly ContentView _isi = new();
    private readonly List<(string Judul, View Tampilan, Button Tombol)> _bagian = new();

    // Penjualan
    private readonly FilterTanggal _fJual = new();
    private readonly Picker _urutJual = Ui.Daftar("Urutan");
    private readonly Label _ringkasJual = Ui.Lbl("", tebal: true, ukuran: 14, warna: Tema.UtamaGelap);
    private readonly Label _terlaris = Ui.Lbl("", ukuran: 13);
    private readonly CollectionView _daftarJual;

    // Laba / rugi
    private readonly FilterTanggal _fLaba = new();
    private readonly Label _ringkasLaba = Ui.Lbl("", tebal: true, ukuran: 13);
    private readonly Border _kartuLaba;
    private readonly CollectionView _daftarLaba;

    // Riwayat harga
    private readonly CollectionView _daftarHarga;

    // Stok
    private readonly Label _ringkasStok = Ui.Lbl("", ukuran: 13);
    private readonly CollectionView _daftarStok;

    public LaporanPanel(DataStore store, bool penjualan, bool stok)
    {
        _store = store;
        _penjualan = penjualan;
        _stok = stok;

        _daftarJual = Ui.DaftarItem<BarisPenjualan>(KartuPenjualan, null);
        _daftarLaba = Ui.DaftarItem<BarisLaba>(KartuLaba, null);
        _daftarHarga = Ui.DaftarItem<BarisRiwayatHarga>(KartuHarga, "Belum ada perubahan harga.");
        _daftarStok = Ui.DaftarItem<Barang>(b => Ui.KartuBarang(b, true, null), null);
        _kartuLaba = Ui.Kartu(_ringkasLaba);

        if (penjualan)
        {
            Tambah("Penjualan", BuatPenjualan());
            Tambah("Laba/Rugi", BuatLaba());
            Tambah("Harga", BuatRiwayatHarga());
        }
        if (stok) Tambah("Stok", BuatStok());

        var tombol = new Grid { ColumnSpacing = 6, Padding = new Thickness(12, 8, 12, 4) };
        for (int i = 0; i < _bagian.Count; i++)
        {
            tombol.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            tombol.Add(_bagian[i].Tombol, i, 0);
        }

        var grid = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) }
        };
        grid.Add(tombol, 0, 0);
        grid.Add(_isi, 0, 1);
        Content = grid;

        Buka(0);
    }

    private void Tambah(string judul, View tampilan)
    {
        int indeks = _bagian.Count;
        var b = Ui.Tombol(judul, () => Buka(indeks));
        b.FontSize = 12;
        b.Padding = new Thickness(2, 0);
        b.HeightRequest = 38;
        _bagian.Add((judul, tampilan, b));
    }

    private void Buka(int indeks)
    {
        if (indeks < 0 || indeks >= _bagian.Count) return;
        _isi.Content = _bagian[indeks].Tampilan;
        for (int i = 0; i < _bagian.Count; i++)
        {
            bool aktif = i == indeks;
            _bagian[i].Tombol.BackgroundColor = aktif ? Tema.Utama : Tema.Putih;
            _bagian[i].Tombol.TextColor = aktif ? Tema.Putih : Tema.Utama;
        }
    }

    // ---------- Penjualan ----------

    private View BuatPenjualan()
    {
        _urutJual.ItemsSource = new[] { "Terbaru dulu", "Terlama dulu" };
        _urutJual.SelectedIndex = 0;
        _urutJual.SelectedIndexChanged += (s, e) => TampilkanPenjualan(false);

        var kontrol = new VerticalStackLayout
        {
            Spacing = 6,
            Children =
            {
                _fJual,
                Ui.Baris("Urutan", _urutJual),
                Ui.TombolAsync("Tampilkan", () => { TampilkanPenjualan(true); return Task.CompletedTask; }, utama: true),
                Ui.Lbl("Ketuk transaksi untuk melihat struk.", ukuran: 12, warna: Tema.TeksRedup)
            }
        };

        var ringkas = Ui.Kartu(new VerticalStackLayout
        {
            Spacing = 4,
            Children = { _ringkasJual, Ui.Lbl("Barang terlaris", tebal: true, ukuran: 13), _terlaris }
        });

        // seluruh bagian atas bisa digulir bersama daftar agar muat di layar kecil
        var atas = new VerticalStackLayout { Spacing = 8, Children = { kontrol, ringkas } };
        _daftarJual.Header = atas;
        var g = new Grid { Padding = new Thickness(12, 4) };
        g.Add(_daftarJual);
        return g;
    }

    private async void LihatStruk(BarisPenjualan baris)
    {
        var t = _store.CariTransaksi(baris.No);
        if (t == null) return;

        var items = _store.DetailDari(t.No)
            .Select(d => new ItemKeranjang { Kode = d.Kode, Nama = d.Nama, Qty = d.Qty, Harga = d.Harga })
            .ToList();

        await Ui.Tampilkan(new StrukDialog(
            StrukBuilder.Buat(t.No, t.Tanggal, t.Kasir, items, t.Total, t.Bayar, t.Kembali), "Struk " + t.No));
    }

    private View KartuPenjualan(BarisPenjualan b)
    {
        var kiri = new VerticalStackLayout
        {
            Spacing = 2,
            Children =
            {
                Ui.Lbl(b.No, tebal: true, ukuran: 15),
                Ui.Lbl($"{b.Tanggal}  •  {b.Kasir}  •  {b.JumlahItem} item", ukuran: 12, warna: Tema.TeksRedup)
            }
        };
        var total = Ui.Lbl(Fmt.Rupiah(b.Total), tebal: true, warna: Tema.Utama);
        total.VerticalOptions = LayoutOptions.Center;

        var grid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
        };
        grid.Add(kiri, 0, 0);
        grid.Add(total, 1, 0);

        var kartu = Ui.Kartu(grid);
        Ui.Ketuk(kartu, () => LihatStruk(b));
        return kartu;
    }

    private async void TampilkanPenjualan(bool daritombol)
    {
        string? awal = _fJual.Awal, akhir = _fJual.Akhir;
        if (daritombol && awal != null && akhir != null && string.CompareOrdinal(awal, akhir) > 0)
        {
            await Ui.Peringatan("Tanggal awal tidak boleh setelah tanggal akhir.");
            return;
        }

        var lap = _store.BuatLaporanPenjualan(awal, akhir);

        bool terbaru = _urutJual.SelectedIndex <= 0;
        var urut = terbaru
            ? lap.Daftar.OrderByDescending(t => t.Tanggal, StringComparer.Ordinal).ThenByDescending(t => t.No, StringComparer.Ordinal)
            : lap.Daftar.OrderBy(t => t.Tanggal, StringComparer.Ordinal).ThenBy(t => t.No, StringComparer.Ordinal);

        _daftarJual.ItemsSource = urut
            .Select(t => new BarisPenjualan(t.No, t.Tanggal, t.Kasir, lap.ItemPerTransaksi.GetValueOrDefault(t.No), t.Total))
            .ToList();

        _ringkasJual.Text = lap.Daftar.Count == 0
            ? "Tidak ada transaksi pada rentang tanggal tersebut."
            : $"Total transaksi: {lap.Daftar.Count}\nTotal pendapatan: {Fmt.Rupiah(lap.TotalPendapatan)}";

        int no = 1;
        _terlaris.Text = lap.Terlaris.Count == 0
            ? "-"
            : string.Join("\n", lap.Terlaris.Select(p => $"{no++}. {p.Key}: {p.Value} unit terjual"));
    }

    // ---------- Laba / Rugi ----------

    private View BuatLaba()
    {
        var kontrol = new VerticalStackLayout
        {
            Spacing = 6,
            Children =
            {
                _fLaba,
                Ui.TombolAsync("Tampilkan", () => { TampilkanLaba(true); return Task.CompletedTask; }, utama: true),
                Ui.Lbl("Laba/rugi bersih = pendapatan penjualan - pembelian dari supplier (barang masuk).", ukuran: 12, warna: Tema.TeksRedup)
            }
        };

        _daftarLaba.Header = new VerticalStackLayout { Spacing = 8, Children = { kontrol, _kartuLaba } };
        var g = new Grid { Padding = new Thickness(12, 4) };
        g.Add(_daftarLaba);
        return g;
    }

    private View KartuLaba(BarisLaba b)
    {
        var warna = b.Laba < 0 ? Tema.Bahaya : Tema.UtamaGelap;
        var isi = new VerticalStackLayout
        {
            Spacing = 2,
            Children =
            {
                Ui.Lbl($"{b.Nama}  (x{b.Jumlah})", tebal: true, ukuran: 14, warna: b.Laba < 0 ? Tema.Bahaya : Tema.Teks),
                Ui.Lbl($"{b.No}  •  {b.Tanggal}", ukuran: 12, warna: Tema.TeksRedup),
                Ui.Lbl($"Pendapatan {Fmt.Rupiah(b.Pendapatan)}  |  Modal {Fmt.Rupiah(b.Modal)}", ukuran: 12),
                Ui.Lbl((b.Laba < 0 ? "Rugi " : "Laba ") + Fmt.Rupiah(Math.Abs(b.Laba)), tebal: true, ukuran: 13, warna: warna)
            }
        };
        return Ui.Kartu(isi, b.Laba < 0 ? Tema.PeringatanLatar : Tema.Putih);
    }

    private async void TampilkanLaba(bool daritombol)
    {
        string? awal = _fLaba.Awal, akhir = _fLaba.Akhir;
        if (daritombol && awal != null && akhir != null && string.CompareOrdinal(awal, akhir) > 0)
        {
            await Ui.Peringatan("Tanggal awal tidak boleh setelah tanggal akhir.");
            return;
        }

        var lap = _store.BuatLaporanLaba(awal, akhir);
        _daftarLaba.ItemsSource = lap.Baris;

        if (lap.Baris.Count == 0 && lap.Pembelian == 0)
        {
            _ringkasLaba.TextColor = Tema.Teks;
            _ringkasLaba.Text = "Tidak ada penjualan pada rentang tanggal tersebut.";
            return;
        }

        string status = lap.LabaBersih > 0 ? "UNTUNG" : lap.LabaBersih < 0 ? "RUGI" : "IMPAS";
        _ringkasLaba.TextColor = lap.LabaBersih > 0 ? Tema.UtamaGelap : lap.LabaBersih < 0 ? Tema.Bahaya : Tema.Teks;
        _ringkasLaba.Text =
            $"Pendapatan penjualan: {Fmt.Rupiah(lap.Pendapatan)}\n" +
            $"Laba kotor (setelah modal barang terjual): {Fmt.Rupiah(lap.Laba)}\n" +
            $"Pembelian dari supplier (barang masuk): {Fmt.Rupiah(lap.Pembelian)}\n" +
            $"Laba / Rugi bersih: {Fmt.Rupiah(lap.LabaBersih)}  ({status})";
    }

    // ---------- Riwayat harga ----------

    private View BuatRiwayatHarga()
    {
        var g = new Grid { Padding = new Thickness(12, 4) };
        g.Add(_daftarHarga);
        return g;
    }

    private View KartuHarga(BarisRiwayatHarga b)
    {
        var isi = new VerticalStackLayout
        {
            Spacing = 2,
            Children =
            {
                Ui.Lbl(b.Barang, tebal: true, ukuran: 14, warna: b.Selisih < 0 ? Tema.Bahaya : Tema.Teks),
                Ui.Lbl(b.Tanggal, ukuran: 12, warna: Tema.TeksRedup),
                Ui.Lbl("Harga beli: " + b.HargaBeli, ukuran: 12),
                Ui.Lbl("Harga jual: " + b.HargaJual, ukuran: 12),
                Ui.Lbl("Untung/unit: " + b.Margin, ukuran: 12),
                Ui.Lbl(b.Keterangan, ukuran: 12, warna: b.Selisih < 0 ? Tema.Bahaya : Tema.UtamaGelap)
            }
        };
        return Ui.Kartu(isi, b.Selisih < 0 ? Tema.PeringatanLatar : Tema.Putih);
    }

    private void TampilkanRiwayatHarga()
    {
        _daftarHarga.ItemsSource = _store.SemuaRiwayatHarga()
            .OrderByDescending(r => r.No)
            .Select(r =>
            {
                int marginLama = r.JualLama - r.BeliLama;
                int marginBaru = r.JualBaru - r.BeliBaru;
                int selisih = marginBaru - marginLama;

                var perubahan = new List<string>();
                if (r.BeliBaru > r.BeliLama) perubahan.Add("Harga beli naik");
                else if (r.BeliBaru < r.BeliLama) perubahan.Add("Harga beli turun");
                if (r.JualBaru > r.JualLama) perubahan.Add("Harga jual naik");
                else if (r.JualBaru < r.JualLama) perubahan.Add("Harga jual turun");

                string dampak =
                    marginBaru < 0 ? $"RUGI {Fmt.Rupiah(-marginBaru)} per unit"
                    : selisih > 0 ? $"untung naik {Fmt.Rupiah(selisih)} per unit"
                    : selisih < 0 ? $"untung turun {Fmt.Rupiah(-selisih)} per unit"
                    : "untung per unit tetap";

                return new BarisRiwayatHarga(
                    r.Tanggal,
                    $"{r.Kode} - {r.Nama}",
                    $"{Fmt.Rupiah(r.BeliLama)} → {Fmt.Rupiah(r.BeliBaru)}",
                    $"{Fmt.Rupiah(r.JualLama)} → {Fmt.Rupiah(r.JualBaru)}",
                    $"{Fmt.Rupiah(marginLama)} → {Fmt.Rupiah(marginBaru)}",
                    string.Join(", ", perubahan) + "; " + dampak,
                    selisih);
            })
            .ToList();
    }

    // ---------- Stok ----------

    private View BuatStok()
    {
        _daftarStok.Header = Ui.Kartu(_ringkasStok);
        var g = new Grid { Padding = new Thickness(12, 4) };
        g.Add(_daftarStok);
        return g;
    }

    private void TampilkanStok()
    {
        var r = _store.Ringkasan();
        _ringkasStok.Text =
            $"Total barang masuk (gudang): {r.TotalMasuk} unit\n" +
            $"Total barang keluar non-penjualan: {r.TotalKeluar} unit\n" +
            $"Total barang terjual (penjualan): {r.TotalTerjual} unit\n" +
            (r.JumlahMenipis > 0
                ? $"Jenis barang di bawah stok minimum: {r.JumlahMenipis}"
                : "Semua stok barang dalam kondisi aman.");

        _daftarStok.ItemsSource = _store.SemuaBarang().OrderBy(b => b.Nama, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public override void Segarkan()
    {
        if (_penjualan)
        {
            TampilkanPenjualan(false);
            TampilkanLaba(false);
            TampilkanRiwayatHarga();
        }
        if (_stok) TampilkanStok();
    }
}
