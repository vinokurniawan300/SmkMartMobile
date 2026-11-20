namespace SmkMart.Mobile;

public class UbahMasukDialog : DialogHalaman
{
    private readonly DataStore _store;
    private readonly BarangMasuk _data;
    private readonly Entry _jumlah = Ui.Isian("1", angka: true);
    private readonly Entry _harga = Ui.Isian("0", angka: true);
    private readonly DatePicker _tanggal = Ui.Tanggal();
    private readonly Entry _supplier = Ui.Isian("Nama supplier");
    private readonly Entry _keterangan = Ui.Isian("Keterangan (opsional)");

    public bool AdaPerubahan { get; private set; }

    public UbahMasukDialog(DataStore store, BarangMasuk data) : base("Ubah Barang Masuk")
    {
        _store = store;
        _data = data;

        _jumlah.Text = data.Jumlah.ToString();
        _harga.Text = data.HargaSupplier.ToString();
        if (DateTime.TryParse(data.Tanggal, out var tgl)) _tanggal.Date = tgl;
        _supplier.Text = data.Supplier;
        _keterangan.Text = data.Keterangan;

        Isi(
            Ui.Baris("No", Ui.Lbl(data.No)),
            Ui.Baris("Barang", Ui.Lbl($"{data.Kode} - {data.Nama}")),
            Ui.Baris("Jumlah masuk", _jumlah),
            Ui.Baris("Harga dari supplier (Rp)", _harga),
            Ui.Baris("Tanggal masuk", _tanggal),
            Ui.Baris("Supplier", _supplier),
            Ui.Baris("Keterangan", _keterangan));
    }

    protected override async Task<bool> Simpan()
    {
        var jumlah = Ui.Angka(_jumlah.Text);
        var harga = Ui.Angka(_harga.Text);
        if (jumlah == null || harga == null)
        {
            await Ui.Peringatan("Jumlah dan harga harus berupa angka bulat.");
            return false;
        }

        bool ok = await Ui.Coba(() => _store.UbahMasuk(_data.No, jumlah.Value, Fmt.Tanggal(Ui.Tgl(_tanggal)),
            _supplier.Text ?? "", _keterangan.Text ?? "", harga.Value));
        if (ok) AdaPerubahan = true;
        return ok;
    }
}
