using System.Globalization;
using Microsoft.Maui.Controls.Shapes;

namespace SmkMart.Mobile;

/// <summary>Warna yang sama dengan versi Windows.</summary>
public static class Tema
{
    public static readonly Color Utama = Color.FromRgb(34, 84, 63);
    public static readonly Color UtamaGelap = Color.FromRgb(23, 56, 43);
    public static readonly Color Emas = Color.FromRgb(190, 154, 70);
    public static readonly Color EmasMuda = Color.FromRgb(228, 208, 150);
    public static readonly Color Latar = Color.FromRgb(244, 246, 240);
    public static readonly Color Garis = Color.FromRgb(214, 218, 204);
    public static readonly Color Teks = Color.FromRgb(33, 42, 36);
    public static readonly Color TeksRedup = Color.FromRgb(104, 113, 101);
    public static readonly Color TeksPadaGelap = Color.FromRgb(206, 220, 208);
    public static readonly Color Bahaya = Color.FromRgb(139, 46, 46);
    public static readonly Color PeringatanLatar = Color.FromRgb(247, 232, 226);
    public static readonly Color AmanLatar = Color.FromRgb(228, 236, 225);
    public static readonly Color Putih = Colors.White;
}

public static class Ui
{
    public static Window? Jendela => Application.Current?.Windows.FirstOrDefault();

    public static Page? Halaman => Jendela?.Page;

    // ---------- Pesan ----------

    public static Task Info(string pesan) => Peringatan(pesan, "SMK MART");

    public static async Task Peringatan(string pesan, string judul = "Perhatian")
    {
        var h = Halaman;
        if (h != null) await h.DisplayAlertAsync(judul, pesan, "OK");
    }

    public static async Task<bool> Konfirmasi(string pesan, string ya = "Ya")
    {
        var h = Halaman;
        return h != null && await h.DisplayAlertAsync("Konfirmasi", pesan, ya, "Batal");
    }

    public static async Task<string?> Prompt(string judul, string pesan, string awal)
    {
        var h = Halaman;
        if (h == null) return null;
        return await h.DisplayPromptAsync(judul, pesan, "OK", "Batal", keyboard: Keyboard.Numeric, initialValue: awal);
    }

    public static async Task<string?> Pilih(string judul, params string[] pilihan)
    {
        var h = Halaman;
        if (h == null) return null;
        return await h.DisplayActionSheetAsync(judul, "Batal", null, pilihan);
    }

    /// <summary>Jalankan aksi; kesalahan aturan bisnis ditampilkan sebagai peringatan.</summary>
    public static async Task<bool> Coba(Action aksi)
    {
        try
        {
            aksi();
            return true;
        }
        catch (AturanException ex)
        {
            await Peringatan(ex.Message);
        }
        catch (Exception ex)
        {
            await Peringatan("Terjadi kesalahan: " + ex.Message, "Kesalahan");
        }
        return false;
    }

    /// <summary>Tampilkan dialog modal dan tunggu sampai ditutup.</summary>
    public static async Task<bool> Tampilkan(DialogHalaman dialog)
    {
        var h = Halaman;
        if (h == null) return false;
        await h.Navigation.PushModalAsync(dialog);
        return await dialog.Selesai;
    }

    // ---------- Konversi ----------

    /// <summary>Baca bilangan bulat >= 0 dari teks isian. Kosong dianggap 0. Tidak valid = null.</summary>
    public static int? Angka(string? teks)
    {
        var t = (teks ?? "").Trim().Replace(".", "").Replace(",", "");
        if (t == "") return 0;
        return int.TryParse(t, NumberStyles.None, CultureInfo.InvariantCulture, out var v) ? v : null;
    }

    public static DateTime Tgl(DatePicker p)
    {
        object? o = p.Date;
        return o is DateTime d ? d : DateTime.Today;
    }

    // ---------- Pembuat kontrol ----------

    public static Label Lbl(string teks = "", bool tebal = false, double ukuran = 14, Color? warna = null) => new()
    {
        Text = teks,
        FontSize = ukuran,
        FontAttributes = tebal ? FontAttributes.Bold : FontAttributes.None,
        TextColor = warna ?? Tema.Teks
    };

    public static Entry Isian(string placeholder = "", bool angka = false, string teks = "") => new()
    {
        Placeholder = placeholder,
        Text = teks,
        Keyboard = angka ? Keyboard.Numeric : Keyboard.Text,
        TextColor = Tema.Teks,
        PlaceholderColor = Tema.TeksRedup,
        BackgroundColor = Tema.Putih
    };

    public static DatePicker Tanggal(DateTime? awal = null) => new()
    {
        Format = "yyyy-MM-dd",
        Date = awal ?? DateTime.Today,
        TextColor = Tema.Teks
    };

    public static Picker Daftar(string judul) => new()
    {
        Title = judul,
        TextColor = Tema.Teks,
        TitleColor = Tema.TeksRedup
    };

    public static Button Tombol(string teks, Action aksi, bool utama = false, bool bahaya = false, bool emas = false) =>
        TombolAsync(teks, () => { aksi(); return Task.CompletedTask; }, utama, bahaya, emas);

    public static Button TombolAsync(string teks, Func<Task> aksi, bool utama = false, bool bahaya = false, bool emas = false)
    {
        var warna = bahaya ? Tema.Bahaya : emas ? Tema.Emas : utama ? Tema.Utama : Tema.Putih;
        var b = new Button
        {
            Text = teks,
            BackgroundColor = warna,
            TextColor = (utama || bahaya || emas) ? Tema.Putih : Tema.Utama,
            BorderColor = Tema.Utama,
            BorderWidth = (utama || bahaya || emas) ? 0 : 1,
            CornerRadius = 8,
            FontSize = 14,
            Padding = new Thickness(14, 0),
            HeightRequest = 44
        };
        b.Clicked += async (s, e) => await aksi();
        return b;
    }

    public static View Baris(string label, View isi) => new VerticalStackLayout
    {
        Spacing = 4,
        Children = { Lbl(label, ukuran: 12, warna: Tema.TeksRedup), isi }
    };

    public static Border Kartu(View isi, Color? latar = null) => new()
    {
        Content = isi,
        Padding = new Thickness(12, 10),
        BackgroundColor = latar ?? Tema.Putih,
        Stroke = Tema.Garis,
        StrokeThickness = 1,
        StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) }
    };

    public static CollectionView DaftarItem<T>(Func<T, View> bangun, string? kosong = "Tidak ada data.") => new()
    {
        SelectionMode = SelectionMode.None,
        ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 8 },
        EmptyView = kosong == null
            ? null
            : new Label
            {
                Text = kosong,
                TextColor = Tema.TeksRedup,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 24)
            },
        ItemTemplate = new DataTemplate(() =>
        {
            var v = new ContentView();
            v.BindingContextChanged += (s, e) =>
            {
                v.Content = v.BindingContext is T t ? bangun(t) : null;
            };
            return v;
        })
    };

    /// <summary>Kartu satu barang (dipakai di beberapa layar).</summary>
    public static View KartuBarang(Barang b, bool tampilBeli, Action<Barang>? ketuk)
    {
        bool menipis = b.Stok <= b.StokMin;
        var kiri = new VerticalStackLayout
        {
            Spacing = 2,
            Children =
            {
                Lbl(b.Nama, tebal: true, ukuran: 15, warna: menipis ? Tema.Bahaya : Tema.Teks),
                Lbl($"{b.Kode}  •  {b.Kategori}", ukuran: 12, warna: Tema.TeksRedup),
                Lbl(tampilBeli
                    ? $"Beli {Fmt.Rupiah(b.HargaBeli)}  |  Jual {Fmt.Rupiah(b.HargaJual)}"
                    : Fmt.Rupiah(b.HargaJual), ukuran: 13)
            }
        };
        var kanan = new VerticalStackLayout
        {
            Spacing = 2,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                Lbl(b.Stok.ToString(CultureInfo.InvariantCulture), tebal: true, ukuran: 20,
                    warna: menipis ? Tema.Bahaya : Tema.Utama),
                Lbl("min " + b.StokMin, ukuran: 11, warna: Tema.TeksRedup)
            }
        };
        var grid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 8
        };
        grid.Add(kiri, 0, 0);
        grid.Add(kanan, 1, 0);

        var kartu = Kartu(grid, menipis ? Tema.PeringatanLatar : Tema.Putih);
        Ketuk(kartu, () => ketuk?.Invoke(b), ketuk != null);
        return kartu;
    }

    public static void Ketuk(View v, Action aksi, bool aktif = true)
    {
        if (!aktif) return;
        var g = new TapGestureRecognizer();
        g.Tapped += (s, e) => aksi();
        v.GestureRecognizers.Add(g);
    }

    public static IEnumerable<Barang> CariBarang(IEnumerable<Barang> sumber, string kata)
    {
        kata = kata.Trim();
        if (kata == "") return sumber;
        return sumber.Where(b => b.Kode.Contains(kata, StringComparison.OrdinalIgnoreCase)
                              || b.Nama.Contains(kata, StringComparison.OrdinalIgnoreCase)
                              || b.Kategori.Contains(kata, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>Panel isi satu tab di jendela utama.</summary>
public abstract class PanelDasar : ContentView
{
    public event Action? DataBerubah;

    public abstract void Segarkan();

    protected void Beritahu() => DataBerubah?.Invoke();
}

/// <summary>Halaman dialog layar penuh dengan tombol Simpan / Batal.</summary>
public abstract class DialogHalaman : ContentPage
{
    private readonly TaskCompletionSource<bool> _selesai = new();
    private readonly VerticalStackLayout _isi = new() { Spacing = 12, Padding = new Thickness(16) };

    public Task<bool> Selesai => _selesai.Task;

    public bool Berhasil { get; private set; }

    protected DialogHalaman(string judul, string simpan = "Simpan", string? batal = "Batal")
    {
        BackgroundColor = Tema.Latar;
        NavigationPage.SetHasNavigationBar(this, false);

        var header = new VerticalStackLayout
        {
            BackgroundColor = Tema.UtamaGelap,
            Children =
            {
                new Label
                {
                    Text = judul,
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Tema.EmasMuda,
                    Padding = new Thickness(16, 14)
                },
                new BoxView { HeightRequest = 3, Color = Tema.Emas }
            }
        };

        var tombol = new Grid { ColumnSpacing = 10, Padding = new Thickness(16, 10), BackgroundColor = Tema.Latar };
        if (batal != null)
        {
            tombol.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            tombol.Add(Ui.TombolAsync(batal, () => Tutup(false)), 0, 0);
        }
        tombol.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        tombol.Add(Ui.TombolAsync(simpan, KlikSimpan, utama: true), batal != null ? 1 : 0, 0);

        var badan = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
        };
        badan.Add(header, 0, 0);
        badan.Add(new ScrollView { Content = _isi }, 0, 1);
        badan.Add(tombol, 0, 2);
        Content = badan;
    }

    protected void Isi(params View[] tampilan)
    {
        foreach (var v in tampilan) _isi.Add(v);
    }

    /// <summary>Kembalikan true bila berhasil sehingga dialog ditutup.</summary>
    protected abstract Task<bool> Simpan();

    private async Task KlikSimpan()
    {
        if (await Simpan()) await Tutup(true);
    }

    protected async Task Tutup(bool ok)
    {
        Berhasil = ok;
        await Navigation.PopModalAsync();
        _selesai.TrySetResult(ok);
    }

    protected override bool OnBackButtonPressed()
    {
        _selesai.TrySetResult(Berhasil);
        return base.OnBackButtonPressed();
    }
}

/// <summary>Pemilih rentang tanggal; mati = semua tanggal.</summary>
public class FilterTanggal : ContentView
{
    private readonly Switch _pakai = new() { OnColor = Tema.Utama };
    private readonly DatePicker _awal = Ui.Tanggal();
    private readonly DatePicker _akhir = Ui.Tanggal();

    public string? Awal => _pakai.IsToggled ? Fmt.Tanggal(Ui.Tgl(_awal)) : null;

    public string? Akhir => _pakai.IsToggled ? Fmt.Tanggal(Ui.Tgl(_akhir)) : null;

    public FilterTanggal()
    {
        _awal.IsEnabled = false;
        _akhir.IsEnabled = false;
        _pakai.Toggled += (s, e) =>
        {
            _awal.IsEnabled = e.Value;
            _akhir.IsEnabled = e.Value;
        };

        var atas = new HorizontalStackLayout
        {
            Spacing = 8,
            Children = { _pakai, new Label { Text = "Filter tanggal (kosong = semua)", VerticalOptions = LayoutOptions.Center, TextColor = Tema.Teks, FontSize = 13 } }
        };

        var tgl = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 8
        };
        tgl.Add(Ui.Baris("Dari", _awal), 0, 0);
        tgl.Add(Ui.Baris("Sampai", _akhir), 1, 0);

        Content = new VerticalStackLayout { Spacing = 4, Children = { atas, tgl } };
    }
}
