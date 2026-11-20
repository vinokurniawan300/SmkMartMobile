namespace SmkMart.Mobile;

public class UbahKeluarDialog : DialogHalaman
{
    private readonly DataStore _store;
    private readonly BarangKeluar _data;
    private readonly Entry _jumlah = Ui.Isian("1", angka: true);
    private readonly Picker _jenis = Ui.Daftar("Jenis");
    private readonly DatePicker _tanggal = Ui.Tanggal();
    private readonly Entry _keterangan = Ui.Isian("Keterangan (opsional)");

    public bool AdaPerubahan { get; private set; }

    public UbahKeluarDialog(DataStore store, BarangKeluar data) : base("Ubah Barang Keluar")
    {
        _store = store;
        _data = data;

        var jenis = new[] { "Barang Rusak", "Retur ke Supplier", "Lainnya" };
        _jenis.ItemsSource = jenis;
        int idx = Array.IndexOf(jenis, data.Jenis);
        _jenis.SelectedIndex = idx >= 0 ? idx : jenis.Length - 1;

        int stok = store.CariBarang(data.Kode)?.Stok ?? 0;
        _jumlah.Text = data.Jumlah.ToString();
        if (DateTime.TryParse(data.Tanggal, out var tgl)) _tanggal.Date = tgl;
        _keterangan.Text = data.Keterangan;

        Isi(
            Ui.Baris("No", Ui.Lbl(data.No)),
            Ui.Baris("Barang", Ui.Lbl($"{data.Kode} - {data.Nama}")),
            Ui.Lbl("Stok saat ini: " + stok, ukuran: 13, warna: Tema.TeksRedup),
            Ui.Baris("Jumlah keluar", _jumlah),
            Ui.Baris("Jenis", _jenis),
            Ui.Baris("Tanggal keluar", _tanggal),
            Ui.Baris("Keterangan", _keterangan));
    }

    protected override async Task<bool> Simpan()
    {
        var jumlah = Ui.Angka(_jumlah.Text);
        if (jumlah == null)
        {
            await Ui.Peringatan("Jumlah harus berupa angka bulat.");
            return false;
        }

        bool ok = await Ui.Coba(() => _store.UbahKeluar(_data.No, jumlah.Value,
            _jenis.SelectedItem as string ?? "Lainnya", Fmt.Tanggal(Ui.Tgl(_tanggal)), _keterangan.Text ?? ""));
        if (ok) AdaPerubahan = true;
        return ok;
    }
}
