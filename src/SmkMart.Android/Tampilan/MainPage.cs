using System.IO.Compression;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;

namespace SmkMart.Mobile;

public class MainPage : ContentPage
{
    private readonly DataStore _store;
    private readonly Pengguna _user;
    private readonly List<PanelDasar> _panel = new();
    private readonly List<Button> _tombolTab = new();
    private readonly ContentView _konten = new();
    private readonly Label _banner = new()
    {
        FontSize = 13,
        Padding = new Thickness(14, 8),
        VerticalTextAlignment = TextAlignment.Center
    };
    private readonly Grid _barTab = new() { BackgroundColor = Tema.Putih, ColumnSpacing = 0 };

    public MainPage(DataStore store, Pengguna user)
    {
        _store = store;
        _user = user;
        BackgroundColor = Tema.Latar;
        NavigationPage.SetHasNavigationBar(this, false);

        Ui.Ketuk(_banner, () => _ = LihatStokMenipis());
        BuatTab();

        var badan = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            }
        };
        badan.Add(BuatHeader(), 0, 0);
        badan.Add(_konten, 0, 1);
        badan.Add(_banner, 0, 2);
        badan.Add(_barTab, 0, 3);
        Content = badan;

        SegarkanSemua();
        PilihTab(0);
    }

    private View BuatHeader()
    {
        var info = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label { Text = "SMK MART", FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Tema.EmasMuda },
                new Label { Text = $"{_user.Nama}  |  {_user.Role}", FontSize = 12, TextColor = Tema.TeksPadaGelap }
            }
        };

        var menu = new Button
        {
            Text = "☰",
            FontSize = 22,
            TextColor = Tema.EmasMuda,
            BackgroundColor = Colors.Transparent,
            WidthRequest = 52,
            HeightRequest = 48,
            VerticalOptions = LayoutOptions.Center
        };
        menu.Clicked += async (s, e) => await BukaMenu();

        var grid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            Padding = new Thickness(16, 10, 8, 10),
            BackgroundColor = Tema.UtamaGelap
        };
        grid.Add(info, 0, 0);
        grid.Add(menu, 1, 0);

        return new VerticalStackLayout
        {
            Children = { grid, new BoxView { HeightRequest = 3, Color = Tema.Emas } }
        };
    }

    private void BuatTab()
    {
        switch (_user.Role)
        {
            case Peran.Admin:
                TambahTab("Penjualan", new PenjualanPanel(_store, _user));
                TambahTab("Barang", new BarangPanel(_store, bisaUbah: true));
                TambahTab("Gudang", new GudangPanel(_store, _user));
                TambahTab("Laporan", new LaporanPanel(_store, penjualan: true, stok: true));
                TambahTab("Pengguna", new PenggunaPanel(_store, _user));
                break;
            case Peran.Kasir:
                TambahTab("Penjualan", new PenjualanPanel(_store, _user));
                TambahTab("Barang", new BarangPanel(_store, bisaUbah: false));
                break;
            case Peran.Gudang:
                TambahTab("Gudang", new GudangPanel(_store, _user));
                TambahTab("Barang", new BarangPanel(_store, bisaUbah: false));
                TambahTab("Laporan", new LaporanPanel(_store, penjualan: false, stok: true));
                break;
        }
    }

    private void TambahTab(string judul, PanelDasar panel)
    {
        int indeks = _panel.Count;
        _panel.Add(panel);
        panel.DataBerubah += SegarkanSemua;

        var b = new Button
        {
            Text = judul,
            FontSize = 12,
            CornerRadius = 0,
            Padding = new Thickness(2, 0),
            HeightRequest = 52,
            BackgroundColor = Tema.Putih,
            TextColor = Tema.TeksRedup
        };
        b.Clicked += (s, e) => PilihTab(indeks);
        _tombolTab.Add(b);

        _barTab.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _barTab.Add(b, indeks, 0);
    }

    private void PilihTab(int indeks)
    {
        if (indeks < 0 || indeks >= _panel.Count) return;
        _konten.Content = _panel[indeks];
        for (int i = 0; i < _tombolTab.Count; i++)
        {
            bool aktif = i == indeks;
            _tombolTab[i].BackgroundColor = aktif ? Tema.Utama : Tema.Putih;
            _tombolTab[i].TextColor = aktif ? Tema.Putih : Tema.TeksRedup;
            _tombolTab[i].FontAttributes = aktif ? FontAttributes.Bold : FontAttributes.None;
        }
    }

    private void SegarkanSemua()
    {
        foreach (var p in _panel) p.Segarkan();
        PerbaruiBanner();
    }

    private void PerbaruiBanner()
    {
        int n = _store.StokMenipis().Count;
        if (n == 0)
        {
            _banner.BackgroundColor = Tema.AmanLatar;
            _banner.TextColor = Tema.UtamaGelap;
            _banner.Text = "Semua stok barang dalam kondisi aman.";
        }
        else
        {
            _banner.BackgroundColor = Tema.PeringatanLatar;
            _banner.TextColor = Tema.Bahaya;
            _banner.Text = $"Peringatan: {n} jenis barang stoknya hampir habis (ketuk untuk melihat).";
        }
    }

    private async Task BukaMenu()
    {
        var pilih = await Ui.Pilih("Menu", "Stok Menipis", "Cadangkan Data", "Keluar Akun");
        switch (pilih)
        {
            case "Stok Menipis": await LihatStokMenipis(); break;
            case "Cadangkan Data": await Cadangkan(); break;
            case "Keluar Akun":
                var jendela = Ui.Jendela;
                if (jendela != null) jendela.Page = new NavigationPage(new LoginPage(_store));
                break;
        }
    }

    private async Task LihatStokMenipis()
    {
        var daftar = _store.StokMenipis();
        if (daftar.Count == 0)
        {
            await Ui.Info("Semua stok barang dalam kondisi aman.");
            return;
        }
        await Ui.Tampilkan(new StokMenipisDialog(daftar));
    }

    private async Task Cadangkan()
    {
        try
        {
            var zip = Path.Combine(FileSystem.CacheDirectory, $"smkmart-data-{DateTime.Now:yyyyMMdd-HHmm}.zip");
            if (File.Exists(zip)) File.Delete(zip);
            ZipFile.CreateFromDirectory(_store.Folder, zip);
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Cadangan data SMK MART",
                File = new ShareFile(zip)
            });
        }
        catch (Exception ex)
        {
            await Ui.Peringatan("Tidak dapat membuat cadangan:\n" + ex.Message);
        }
    }
}
