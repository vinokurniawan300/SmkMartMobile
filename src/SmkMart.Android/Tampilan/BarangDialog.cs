namespace SmkMart.Mobile;

public class BarangDialog : DialogHalaman
{
    private readonly DataStore _store;
    private readonly Barang? _ubah;
    private readonly Entry _kode = Ui.Isian();
    private readonly Entry _nama = Ui.Isian("Nama barang");
    private readonly Entry _kategori = Ui.Isian("Kategori");
    private readonly Entry _beli = Ui.Isian("0", angka: true);
    private readonly Entry _jual = Ui.Isian("0", angka: true);
    private readonly Entry _stok = Ui.Isian("0", angka: true);
    private readonly Entry _stokMin = Ui.Isian("0", angka: true);

    public Barang? Hasil { get; private set; }

    public BarangDialog(DataStore store, Barang? ubah = null)
        : base(ubah == null ? "Tambah Barang" : "Ubah Barang")
    {
        _store = store;
        _ubah = ubah;

        _kode.IsReadOnly = true;
        if (ubah == null)
        {
            _kode.Text = store.BuatKode();
        }
        else
        {
            _kode.Text = ubah.Kode;
            _nama.Text = ubah.Nama;
            _kategori.Text = ubah.Kategori;
            _beli.Text = ubah.HargaBeli.ToString();
            _jual.Text = ubah.HargaJual.ToString();
            _stok.Text = ubah.Stok.ToString();
            _stokMin.Text = ubah.StokMin.ToString();
            _stok.IsEnabled = false;
        }

        Isi(
            Ui.Baris("Kode", _kode),
            Ui.Baris("Nama barang", _nama),
            Ui.Baris("Kategori", _kategori),
            Ui.Baris("Harga beli (Rp)", _beli),
            Ui.Baris("Harga jual (Rp)", _jual),
            Ui.Baris(ubah == null ? "Stok awal" : "Stok saat ini", _stok),
            Ui.Baris("Stok minimum", _stokMin));
    }

    protected override async Task<bool> Simpan()
    {
        var beli = Ui.Angka(_beli.Text);
        var jual = Ui.Angka(_jual.Text);
        var stok = Ui.Angka(_stok.Text);
        var min = Ui.Angka(_stokMin.Text);
        if (beli == null || jual == null || stok == null || min == null)
        {
            await Ui.Peringatan("Harga dan stok harus berupa angka bulat (0 atau lebih).");
            return false;
        }

        return await Ui.Coba(() =>
        {
            if (_ubah == null)
                Hasil = _store.TambahBarang(_nama.Text ?? "", _kategori.Text ?? "", beli.Value, jual.Value, stok.Value, min.Value);
            else
                _store.UbahBarang(_ubah.Kode, _nama.Text ?? "", _kategori.Text ?? "", beli.Value, jual.Value, min.Value);
        });
    }
}
