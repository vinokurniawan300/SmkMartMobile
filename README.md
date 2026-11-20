# SMK MART - Aplikasi Kasir & Gudang

Aplikasi desktop (C# / .NET 11 / Windows Forms) untuk kasir dan administrasi gudang toko.
Ini adalah versi C# dengan tampilan grafis dari program konsol Python `mbuh.py`.
Seluruh fitur dan aturan bisnis aslinya dipertahankan.

## Fitur

| Peran  | Yang bisa dilakukan |
|--------|---------------------|
| **Admin**  | Penjualan, kelola barang (tambah/ubah/hapus/urutkan/cari), administrasi gudang, laporan penjualan & stok, kelola pengguna |
| **Kasir**  | Transaksi penjualan (keranjang, kembalian otomatis, struk), lihat daftar barang & harga |
| **Gudang** | Catat barang masuk (termasuk mendaftarkan barang baru), catat barang keluar (rusak / retur / lainnya), riwayat, laporan stok |

Tambahan di versi GUI:

- Pencarian barang langsung saat mengetik, urutkan lewat pilihan di atas tabel
- Baris barang dengan stok di bawah/sama dengan stok minimum diberi warna merah
- Banner stok menipis di bagian bawah jendela (klik untuk melihat daftarnya)
- Pemilih tanggal (tidak perlu mengetik `YYYY-MM-DD`)
- Laporan penjualan dengan filter tanggal; klik dua kali sebuah transaksi untuk melihat struknya
- Struk bisa disalin ke clipboard

## Akun bawaan (saat pertama kali dijalankan)

| Username | Password   | Peran  |
|----------|------------|--------|
| admin    | admin123   | Admin  |
| kasir1   | kasir123   | Kasir  |
| gudang1  | gudang123  | Gudang |

Segera ganti password ini lewat menu **Pengguna** (login sebagai Admin).

## Menjalankan

Syarat: Windows 10/11 dan [.NET 11 SDK](https://dotnet.microsoft.com/download/dotnet/11.0).

```bash
git clone <url-repo-anda>
cd SmkMarts
dotnet run --project src/SmkMart
```

Atau buka `SmkMart.sln` di Visual Studio 2022 lalu tekan F5.

Cara cepat di Windows: klik dua kali `BuatInstaller.bat` (otomatis publish + membuat Setup.exe di folder `installer-output`; Inno Setup dipasang lewat winget bila belum ada).

Di GitHub: push tag, mis. `git tag v1.0.1 && git push origin v1.0.1`. GitHub Actions membuat `SmkMart-Setup-1.0.1.exe` di halaman Releases.

Membuat versi rilis manual:

```bash
dotnet publish src/SmkMart/SmkMart.csproj -c Release -r win-x64 --self-contained false -o publish
```

## Versi Android (.apk)

Folder `src/SmkMart.Android/` berisi port **.NET MAUI** untuk Android. Logika data (`Models/` dan `Services/`)
**dipakai bersama** versi Windows lewat tautan file di csproj, jadi aturan bisnis dan format data tetap sama.
Hanya tampilan yang dibuat ulang untuk layar sentuh (tab bawah, kartu, dialog layar penuh).

Fitur sama dengan versi Windows: login 3 peran, penjualan + struk, kelola barang, gudang (masuk/keluar/ubah),
laporan (penjualan, laba/rugi, riwayat harga, stok), kelola pengguna, banner stok menipis.
Tombol **Folder Data** diganti menu **Cadangkan Data** (bagikan data sebagai .zip lewat menu share Android).

Data tersimpan di penyimpanan privat aplikasi, terpisah dari versi Windows.
Untuk memindahkan data, pakai Cadangkan Data lalu salin isi zip-nya.

### Build lokal

Syarat: .NET 11 SDK, JDK 17, dan workload Android.

```bash
dotnet workload install maui-android
dotnet publish src/SmkMart.Android/SmkMart.Android.csproj -f net11.0-android -c Release -p:AndroidPackageFormat=apk
```

APK ada di `src/SmkMart.Android/bin/Release/net11.0-android/publish/`.

### Build di GitHub

Workflow `.github/workflows/android.yml` membangun APK di setiap push dan melampirkan
`SmkMart-<versi>.apk` ke halaman Releases saat tag `v*` di-push (mis. `v1.0.1`).

APK yang dihasilkan perlu **ditandatangani dengan keystore yang sama** di setiap rilis supaya bisa
diperbarui tanpa uninstall. Tanpa keystore, build memakai kunci debug acak (bisa dipasang, tapi tidak bisa
menimpa versi sebelumnya). Untuk keystore tetap:

```bash
keytool -genkeypair -v -keystore smkmart.keystore -alias smkmart -keyalg RSA -keysize 2048 -validity 10000
base64 -w0 smkmart.keystore   # isi ke secret ANDROID_KEYSTORE_BASE64
```

Lalu buat secret repo `ANDROID_KEYSTORE_BASE64`, `ANDROID_KEYSTORE_PASSWORD`, `ANDROID_KEY_ALIAS`,
`ANDROID_KEY_PASSWORD`. Jangan commit file keystore (sudah ada di `.gitignore`).

## Lokasi data

Data disimpan sebagai file teks berpemisah `|` (format sama persis dengan versi Python) di:

```
%AppData%\SmkMart
```

Folder lain bisa dipilih dengan mengisi variabel lingkungan `SMKMART_DATA`.
Tombol **Folder Data** di jendela utama membuka folder tersebut.

**Migrasi dari versi Python:** salin `pengguna.txt`, `barang.txt`, `transaksi.txt`,
`detail_transaksi.txt`, `barang_masuk.txt`, dan `barang_keluar.txt` ke folder data di atas.

File data masuk `.gitignore` agar password dan data transaksi tidak ikut ter-commit.

## Struktur proyek

```
SmkMart.sln
src/SmkMart.Android/        # port .NET MAUI (Android), memakai Models/ + Services/ di bawah
src/SmkMart/
├── Program.cs              # titik masuk: login -> jendela utama -> logout
├── Models/                 # kelas data (Barang, Pengguna, Transaksi, ...)
├── Services/
│   ├── DataStore.cs        # baca/tulis file + seluruh aturan bisnis
│   ├── StrukBuilder.cs     # teks struk
│   └── Fmt.cs              # format Rupiah, tanggal, pembersih input
└── Tampilan/               # form, dialog, dan panel Windows Forms
```

Logika data (`Services`) dipisah dari tampilan (`Tampilan`) sehingga mudah diuji atau dipindah ke UI lain.

## Catatan keamanan

Seperti versi Python aslinya, password disimpan sebagai **teks biasa** di `pengguna.txt`.
Cukup untuk tugas/latihan, tetapi jangan dipakai untuk toko sungguhan tanpa menambahkan hashing password
(misalnya `PasswordHasher` / BCrypt).

## Lisensi

Belum ditentukan. Tambahkan berkas `LICENSE` sebelum repositori dibuka untuk umum.
