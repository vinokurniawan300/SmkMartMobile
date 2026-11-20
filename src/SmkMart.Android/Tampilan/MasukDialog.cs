namespace SmkMart.Mobile;

public class MasukDialog : DialogHalaman
{
    private readonly DataStore _store;
    private readonly string _petugas;
    private readonly Picker _barang = Ui.Daftar("Pilih barang");
    private readonly Entry _jumlah = Ui.Isian("1", angka: true, teks: "1");
    private readonly Entry _harga = Ui.Isian("0", angka: true);
    private readonly DatePicker _tanggal = Ui.Tanggal();
    private readonly Entry _supplier = Ui.Isian("Nama supplier");
    private readonly Entry _keterangan = Ui.Isian("Keterangan (opsional)");
    private string? _kodeBaru;

    public bool AdaPerubahan { get; private set; }

    public MasukDialog(DataStore store, string petugas) : base("Catat Barang Masuk")
    {
        _store = store;
        _petugas = petugas;

        Isi(
            Ui.Baris("Barang", _barang),
            Ui.TombolAsync("+ Daftarkan barang baru...", BarangBaru),
            Ui.Baris("Jumlah masuk", _jumlah),
            Ui.Baris("Harga dari supplier (Rp)", _harga),
            Ui.Baris("Tanggal masuk", _tanggal),
            Ui.Baris("Supplier", _supplier),
            Ui.Baris("Keterangan", _keterangan));

        _barang.SelectedIndexChanged += (s, e) => PilihanBerubah();
        Muat(null);
    }

    private void Muat(string? pilihKode)
    {
        var daftar = _store.SemuaBarang().OrderBy(x => x.Kode, StringComparer.OrdinalIgnoreCase).ToList();
        _barang.ItemsSource = daftar;

        int idx = 0;
        if (pilihKode != null)
        {
            int i = daftar.FindIndex(b => b.Kode == pilihKode);
            if (i >= 0) idx = i;
        }
        if (daftar.Count > 0) _barang.SelectedIndex = idx;
        PilihanBerubah();
    }

    private void PilihanBerubah()
    {
        if (_barang.SelectedItem is not Barang b) return;

        _harga.Text = b.HargaBeli.ToString();
        if (_kodeBaru != null && b.Kode == _kodeBaru)
        {
            _jumlah.Text = Math.Max(b.Stok, 1).ToString();
            _jumlah.IsEnabled = false;
        }
        else
        {
            _jumlah.IsEnabled = true;
        }
    }

    private async Task BarangBaru()
    {
        var d = new BarangDialog(_store);
        bool ok = await Ui.Tampilkan(d);
        if (!ok || d.Hasil == null) return;

        AdaPerubahan = true;
        var b = d.Hasil;
        if (b.Stok <= 0)
        {
            _kodeBaru = null;
            Muat(b.Kode);
            await Ui.Info($"Barang '{b.Nama}' tersimpan dengan stok awal 0, tidak ada barang masuk yang perlu dicatat.");
            return;
        }

        _kodeBaru = b.Kode;
        Muat(b.Kode);
    }

    protected override async Task<bool> Simpan()
    {
        if (_barang.SelectedItem is not Barang b)
        {
            await Ui.Peringatan("Pilih barang terlebih dahulu.");
            return false;
        }

        var jumlah = Ui.Angka(_jumlah.Text);
        var harga = Ui.Angka(_harga.Text);
        if (jumlah == null || harga == null)
        {
            await Ui.Peringatan("Jumlah dan harga harus berupa angka bulat.");
            return false;
        }

        bool tambahStok = b.Kode != _kodeBaru;
        bool ok = await Ui.Coba(() => _store.CatatMasuk(b.Kode, jumlah.Value, Fmt.Tanggal(Ui.Tgl(_tanggal)),
            _supplier.Text ?? "", _petugas, _keterangan.Text ?? "", tambahStok, harga.Value));
        if (ok) AdaPerubahan = true;
        return ok;
    }
}
