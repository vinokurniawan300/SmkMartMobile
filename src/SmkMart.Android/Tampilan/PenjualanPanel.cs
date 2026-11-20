using System.Globalization;

namespace SmkMart.Mobile;

public class PenjualanPanel : PanelDasar
{
    private readonly DataStore _store;
    private readonly Pengguna _user;
    private readonly List<ItemKeranjang> _keranjang = new();
    private readonly Entry _cari = Ui.Isian("Cari barang...");
    private readonly CollectionView _daftar;
    private readonly Label _ringkas = new() { TextColor = Tema.Putih, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center };

    public PenjualanPanel(DataStore store, Pengguna user)
    {
        _store = store;
        _user = user;

        _cari.TextChanged += (s, e) => Segarkan();
        _daftar = Ui.DaftarItem<Barang>(b => Ui.KartuBarang(b, false, b2 => _ = TambahKeKeranjang(b2)),
            "Barang tidak ditemukan.");
        _daftar.Margin = new Thickness(12, 4);

        var tombolBayar = Ui.TombolAsync("Keranjang / Bayar", BukaKeranjang, emas: true);
        tombolBayar.HeightRequest = 40;

        var bar = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            BackgroundColor = Tema.UtamaGelap,
            Padding = new Thickness(14, 8),
            ColumnSpacing = 8
        };
        bar.Add(_ringkas, 0, 0);
        bar.Add(tombolBayar, 1, 0);

        var atas = new VerticalStackLayout
        {
            Padding = new Thickness(12, 10, 12, 0),
            Spacing = 4,
            Children = { _cari, Ui.Lbl("Ketuk barang untuk memasukkan ke keranjang.", ukuran: 12, warna: Tema.TeksRedup) }
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
        };
        grid.Add(atas, 0, 0);
        grid.Add(_daftar, 0, 1);
        grid.Add(bar, 0, 2);
        Content = grid;

        PerbaruiBar();
    }

    public override void Segarkan()
    {
        _daftar.ItemsSource = Ui.CariBarang(_store.SemuaBarang(), _cari.Text ?? "")
            .OrderBy(b => b.Nama, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private void PerbaruiBar()
    {
        int item = _keranjang.Sum(i => i.Qty);
        int total = _keranjang.Sum(i => i.Subtotal);
        _ringkas.Text = item == 0 ? "Keranjang kosong" : $"{item} item  |  {Fmt.Rupiah(total)}";
    }

    private async Task TambahKeKeranjang(Barang pilih)
    {
        var b = _store.CariBarang(pilih.Kode);
        if (b == null)
        {
            await Ui.Peringatan("Barang tidak ditemukan.");
            Segarkan();
            return;
        }

        var ada = _keranjang.FirstOrDefault(i => i.Kode == b.Kode);
        int tersedia = b.Stok - (ada?.Qty ?? 0);
        if (tersedia <= 0)
        {
            await Ui.Peringatan($"Stok '{b.Nama}' tidak tersedia!");
            return;
        }

        var teks = await Ui.Prompt(b.Nama, $"Jumlah dibeli (tersedia {tersedia}):", "1");
        if (teks == null) return;

        var qty = Ui.Angka(teks);
        if (qty == null || qty.Value < 1)
        {
            await Ui.Peringatan("Jumlah beli minimal 1.");
            return;
        }
        if (qty.Value > tersedia)
        {
            await Ui.Peringatan($"Stok tidak mencukupi! Stok tersedia hanya {tersedia}.");
            return;
        }

        if (ada == null)
            _keranjang.Add(new ItemKeranjang { Kode = b.Kode, Nama = b.Nama, Qty = qty.Value, Harga = b.HargaJual });
        else
            ada.Qty += qty.Value;

        PerbaruiBar();
    }

    private async Task BukaKeranjang()
    {
        if (_keranjang.Count == 0)
        {
            await Ui.Peringatan("Keranjang masih kosong. Ketuk barang untuk menambahkannya.");
            return;
        }

        bool selesai = await Ui.Tampilkan(new KeranjangDialog(_store, _user, _keranjang));
        PerbaruiBar();
        if (selesai) Beritahu();
    }
}

/// <summary>Layar keranjang: ubah isi, isi pembayaran, proses transaksi.</summary>
public class KeranjangDialog : DialogHalaman
{
    private readonly DataStore _store;
    private readonly Pengguna _user;
    private readonly List<ItemKeranjang> _keranjang;
    private readonly VerticalStackLayout _isiKeranjang = new() { Spacing = 8 };
    private readonly DatePicker _tanggal = Ui.Tanggal();
    private readonly Label _total = Ui.Lbl("Rp0", tebal: true, ukuran: 22, warna: Tema.UtamaGelap);
    private readonly Entry _bayar = Ui.Isian("0", angka: true);
    private readonly Label _kembali = Ui.Lbl("Rp0", tebal: true, ukuran: 16);

    public KeranjangDialog(DataStore store, Pengguna user, List<ItemKeranjang> keranjang)
        : base("Keranjang Belanja", "Proses Transaksi", "Tutup")
    {
        _store = store;
        _user = user;
        _keranjang = keranjang;

        _bayar.TextChanged += (s, e) => HitungKembalian();

        Isi(
            _isiKeranjang,
            Ui.Lbl("Ketuk item untuk menghapusnya.", ukuran: 12, warna: Tema.TeksRedup),
            Ui.TombolAsync("Kosongkan Keranjang", Kosongkan, bahaya: true),
            Ui.Baris("Tanggal", _tanggal),
            Ui.Baris("Total", _total),
            Ui.Baris("Uang dibayar (Rp)", _bayar),
            Ui.Baris("Kembalian", _kembali));

        Segarkan();
    }

    private void Segarkan()
    {
        _isiKeranjang.Clear();
        foreach (var item in _keranjang.ToList())
        {
            var kiri = new VerticalStackLayout
            {
                Children =
                {
                    Ui.Lbl(item.Nama, tebal: true),
                    Ui.Lbl($"{item.Qty} x {Fmt.Rupiah(item.Harga)}", ukuran: 12, warna: Tema.TeksRedup)
                }
            };
            var grid = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }
            };
            grid.Add(kiri, 0, 0);
            var sub = Ui.Lbl(Fmt.Rupiah(item.Subtotal), tebal: true);
            sub.VerticalOptions = LayoutOptions.Center;
            grid.Add(sub, 1, 0);

            var kartu = Ui.Kartu(grid);
            var barang = item;
            Ui.Ketuk(kartu, () => _ = HapusItem(barang));
            _isiKeranjang.Add(kartu);
        }

        if (_keranjang.Count == 0) _isiKeranjang.Add(Ui.Lbl("Keranjang kosong.", warna: Tema.TeksRedup));

        _total.Text = Fmt.Rupiah(_keranjang.Sum(i => i.Subtotal));
        HitungKembalian();
    }

    private async Task HapusItem(ItemKeranjang item)
    {
        if (!await Ui.Konfirmasi($"Hapus '{item.Nama}' dari keranjang?", "Hapus")) return;
        _keranjang.Remove(item);
        Segarkan();
    }

    private async Task Kosongkan()
    {
        if (_keranjang.Count == 0) return;
        if (!await Ui.Konfirmasi("Kosongkan seluruh keranjang?", "Kosongkan")) return;
        _keranjang.Clear();
        Segarkan();
    }

    private void HitungKembalian()
    {
        int total = _keranjang.Sum(i => i.Subtotal);
        int bayar = Ui.Angka(_bayar.Text) ?? 0;

        if (total == 0)
        {
            _kembali.Text = "Rp0";
            _kembali.TextColor = Tema.Teks;
        }
        else if (bayar >= total)
        {
            _kembali.Text = Fmt.Rupiah(bayar - total);
            _kembali.TextColor = Tema.Utama;
        }
        else
        {
            _kembali.Text = "Kurang " + Fmt.Rupiah(total - bayar);
            _kembali.TextColor = Tema.Bahaya;
        }
    }

    protected override async Task<bool> Simpan()
    {
        if (_keranjang.Count == 0)
        {
            await Ui.Peringatan("Keranjang masih kosong.");
            return false;
        }

        var bayar = Ui.Angka(_bayar.Text);
        if (bayar == null)
        {
            await Ui.Peringatan("Uang dibayar harus berupa angka.");
            return false;
        }

        var items = _keranjang.ToList();
        Transaksi? trx = null;
        bool ok = await Ui.Coba(() =>
            trx = _store.ProsesPenjualan(Fmt.Tanggal(Ui.Tgl(_tanggal)), _user.Nama, items, bayar.Value));
        if (!ok || trx == null) return false;

        _keranjang.Clear();
        await Ui.Tampilkan(new StrukDialog(
            StrukBuilder.Buat(trx.No, trx.Tanggal, trx.Kasir, items, trx.Total, trx.Bayar, trx.Kembali),
            "Struk " + trx.No));
        return true;
    }
}
