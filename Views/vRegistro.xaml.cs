using ParkIsrael_Octavo.Services;
using ParkIsrael_Octavo.Models;
using ParkIsrael_Octavo.Updates;

#if ANDROID
using Android.Graphics;
using Android.Media;
#endif

namespace ParkIsrael_Octavo.Views;

public partial class vRegistro : ContentPage
{
    FirestoreService firestore = new FirestoreService();
    private string _imagenBase64 = "";

    public vRegistro()
    {
        InitializeComponent();
    }


    //permisos para galeria y camara
    private async Task<bool> SolicitarPermisosAsync()
    {
#if ANDROID
        var cam = await Permissions.CheckStatusAsync<Permissions.Camera>();
        var photos = await Permissions.CheckStatusAsync<Permissions.Photos>();
        if (cam != PermissionStatus.Granted)
            cam = await Permissions.RequestAsync<Permissions.Camera>();
        if (photos != PermissionStatus.Granted)
            photos = await Permissions.RequestAsync<Permissions.Photos>();
        return cam == PermissionStatus.Granted && photos == PermissionStatus.Granted;
#else
        return true;
#endif
    }

#if ANDROID

    //orientacion de la imagen para que no salga rotada
    private int ObtenerRotacionDesdeExif(string path)
    {
        try
        {
            var exif = new ExifInterface(path);
            int orientation = exif.GetAttributeInt(
                ExifInterface.TagOrientation,
                (int)Orientation.Normal
            );

            return orientation switch
            {
                (int)Orientation.Rotate90 => 90,
                (int)Orientation.Rotate180 => 180,
                (int)Orientation.Rotate270 => 270,
                _ => 0
            };
        }
        catch
        {
            return 0;
        }
    }
#endif


    //trata de mejorar la calidad de la imagen para que no se suba muy pixelada
    private byte[] RedimensionarAndroidDesdePath(string path, int ancho, int alto)
    {
#if ANDROID
        BitmapFactory.Options options = new BitmapFactory.Options
        {
            InPreferredConfig = Bitmap.Config.Argb8888
        };

        // 1?? Cargar original
        Bitmap original = BitmapFactory.DecodeFile(path, options);

        // 2?? Obtener rotación EXIF
        int rotation = ObtenerRotacionDesdeExif(path);

        // 3?? Rotar primero (evita pixelado)
        if (rotation != 0)
        {
            Matrix matrix = new Matrix();
            matrix.PostRotate(rotation);
            original = Bitmap.CreateBitmap(
                original,
                0, 0,
                original.Width, original.Height,
                matrix,
                true
            );
        }

        // 4?? Redimensionar SOLO una vez (calidad alta)
        Bitmap resized = Bitmap.CreateScaledBitmap(original, ancho, alto, true);

        // 5?? Exportar JPG calidad 100 (no pérdida visible)
        using MemoryStream ms = new MemoryStream();
        resized.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);

        return ms.ToArray();
#else
        return File.ReadAllBytes(path);
#endif
    }


    //para tomar imagen desde la galeria
    private async Task TomarDesdeGaleriaAsync()
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync();
            if (result == null)
                return;
            byte[] finalBytes;
#if ANDROID
            if (!string.IsNullOrEmpty(result.FullPath))
            {
                finalBytes = RedimensionarAndroidDesdePath(result.FullPath, 400, 400);
            }
            else
            {
                using var stream = await result.OpenReadAsync();
                using MemoryStream ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                finalBytes = ms.ToArray();
            }
#else
            using var stream = await result.OpenReadAsync();
            using MemoryStream ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            finalBytes = ms.ToArray();
#endif

            imgPerfil.Source = ImageSource.FromStream(() => new MemoryStream(finalBytes));
            _imagenBase64 = Convert.ToBase64String(finalBytes);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error Galería", ex.Message, "OK");
        }
    }

    //para tomar imagen desde la camara
    private async Task TomarDesdeCamaraAsync()
    {
        try
        {
            var result = await MediaPicker.CapturePhotoAsync();
            if (result == null)
                return;
            byte[] finalBytes;

#if ANDROID
            if (!string.IsNullOrEmpty(result.FullPath))
            {
                finalBytes = RedimensionarAndroidDesdePath(result.FullPath, 400, 400);
            }
            else
            {
                using var stream = await result.OpenReadAsync();
                using MemoryStream ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                finalBytes = ms.ToArray();
            }
#else
            using var stream = await result.OpenReadAsync();
            using MemoryStream ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            finalBytes = ms.ToArray();
#endif
            imgPerfil.Source = ImageSource.FromStream(() => new MemoryStream(finalBytes));
            _imagenBase64 = Convert.ToBase64String(finalBytes);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error Cámara", ex.Message, "OK");
        }
    }

    private async void btnFoto_Clicked(object sender, EventArgs e)
    {
        if (!await SolicitarPermisosAsync())
        {
            await DisplayAlert("Permisos", "Debe otorgar permisos de cámara y fotos.", "OK");
            return;
        }
        string opcion = await DisplayActionSheet(
            "Seleccionar imagen",
            "Cancelar",
            null,
            "Tomar foto",
            "Elegir de galería"
        );
        if (opcion == "Tomar foto")
            await TomarDesdeCamaraAsync();
        else if (opcion == "Elegir de galería")
            await TomarDesdeGaleriaAsync();
    }

    private async void btnGuardar_Clicked(object sender, EventArgs e)
    {
        var model = new UsuarioModel(
            Id: 0,
            Apellidos: txtApellidos.Text,
            Nombres: txtNombres.Text,
            Cedula: txtCedula.Text,
            Correo: txtCorreo.Text,
            Telefono: txtTelefono.Text,
            Status: pkStatus.SelectedItem?.ToString() ?? "",
            TipoVehiculo: pkTipoVehiculo.SelectedItem?.ToString() ?? "",
            PlacaVehicular: txtPlaca.Text,
            Usuario: txtUsuario.Text,
            Contrasena: txtContrasena.Text,
            Activo: "Si",
            Imagen: _imagenBase64,
            Mensaje: ""
        );
        var actualizado = await UsuarioUpdate.GuardarUsuario(model, firestore);
        await DisplayAlert("Info", actualizado.Mensaje, "OK");
        await Navigation.PushAsync(new vLogin());
    }
}
