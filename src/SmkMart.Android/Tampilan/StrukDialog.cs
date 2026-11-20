using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace SmkMart.Mobile;

public class StrukDialog : DialogHalaman
{
    public StrukDialog(string teks, string judul = "Struk") : base(judul, "Tutup", null)
    {
        var struk = new Label
        {
            Text = teks,
            FontFamily = "monospace",
            FontSize = 12,
            TextColor = Tema.Teks,
            LineBreakMode = LineBreakMode.NoWrap
        };

        var kartu = Ui.Kartu(new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = struk });

        var salin = Ui.TombolAsync("Salin ke Clipboard", async () =>
        {
            await Clipboard.Default.SetTextAsync(teks);
            await Ui.Info("Struk disalin.");
        });

        Isi(kartu, salin);
    }

    protected override Task<bool> Simpan() => Task.FromResult(true);
}
