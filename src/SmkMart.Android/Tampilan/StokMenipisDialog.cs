namespace SmkMart.Mobile;

public class StokMenipisDialog : DialogHalaman
{
    public StokMenipisDialog(List<Barang> daftar) : base("Stok Barang Hampir Habis", "Tutup", null)
    {
        var tumpukan = new VerticalStackLayout { Spacing = 8 };
        foreach (var b in daftar) tumpukan.Add(Ui.KartuBarang(b, false, null));
        Isi(tumpukan);
    }

    protected override Task<bool> Simpan() => Task.FromResult(true);
}
