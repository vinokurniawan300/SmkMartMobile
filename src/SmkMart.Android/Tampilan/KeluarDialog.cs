namespace SmkMart.Mobile;

public class KeluarDialog : DialogHalaman
{
    private readonly DataStore _store;
    private readonly string _petugas;
    private readonly Picker _barang = Ui.Daftar("Pilih barang");
    private readonly Label _info = Ui.Lbl("", ukuran: 13, warna: Tema.TeksRedup);
    private readonly Entry _jumlah = Ui.Isian("1", angka: true, teks: "1");
    private readonly Picker _jenis = Ui.Daftar("Jenis");
    private readonly DatePicker _tanggal = Ui.Tanggal();
    private readonly Entry _keterangan = Ui.Isian("Keterangan (opsional)");

    public bool AdaPerubahan { get; private set; }

    public KeluarDialog(DataStore store, string petugas) : base("Catat Barang Keluar")
    {
        _store = store;
        _petugas = petugas;

        _jenis.ItemsSource = new[] { "Barang Rusak", "Retur ke Supplier", "Lainnya" };
        _jenis.SelectedIndex = 0;

        var daftar = store.SemuaBarang().OrderBy(x => x.Kode, StringComparer.OrdinalIgnoreCase).ToList();
        _barang.ItemsSource = daftar;
        _barang.SelectedIndexChanged += (s, e) => PilihanBerubah();
        if (daftar.Count > 0) _barang.SelectedIndex = 0;

        Isi(
            Ui.Baris("Barang", _barang),
            _info,
            Ui.Baris("Jumlah keluar", _jumlah),
            Ui.Baris("Jenis", _jenis),
            Ui.Baris("Tanggal keluar", _tanggal),
            Ui.Baris("Keterangan", _keterangan));
    }

    private void PilihanBerubah()
    {
        if (_barang.SelectedItem is Barang b) _info.Text = "Stok saat ini: " + b.Stok;
    }

    protected override async Task<bool> Simpan()
    {
        if (_barang.SelectedItem is not Barang b)
        {
            await Ui.Peringatan("Pilih barang terlebih dahulu.");
            return false;
        }

        var jumlah = Ui.Angka(_jumlah.Text);
        if (jumlah == null)
        {
            await Ui.Peringatan("Jumlah harus berupa angka bulat.");
            return false;
        }

        bool ok = await Ui.Coba(() => _store.CatatKeluar(b.Kode, jumlah.Value,
            _jenis.SelectedItem as string ?? "Lainnya", Fmt.Tanggal(Ui.Tgl(_tanggal)), _petugas, _keterangan.Text ?? ""));
        if (ok) AdaPerubahan = true;
        return ok;
    }
}
