using ZXing.Net.Maui;
using ParkIsrael_Octavo.Services;

namespace ParkIsrael_Octavo.Views;

public partial class vEscanearQR : ContentPage
{
    private string Sede;
    public vEscanearQR(string sede)
    {
        InitializeComponent();
        Sede = sede;
        btnEntrar.IsEnabled = false;
        btnSalir.IsEnabled = false;
        btnEntrar.Opacity = 0.4;
        btnSalir.Opacity = 0.4;
        cameraQR.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.TwoDimensional,
            AutoRotate = true,
            Multiple = false
        };        
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var permiso = await Permissions.RequestAsync<Permissions.Camera>();
        if (permiso != PermissionStatus.Granted)
        {
            await DisplayAlert("Permiso requerido", "Debes permitir el uso de la cámara.", "OK");
            return;
        }
        cameraQR.IsDetecting = true;
    }

    private void cameraQR_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        var resultado = e.Results?.FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(resultado))
            return;
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            cameraQR.IsDetecting = false;
            var service = new FirestoreService();
            var usuario = await service.ObtenerUsuarioPorDocumentIdAsync(resultado);
            if (usuario == null)
            {
                imgPerfil.IsVisible = false;
                lblResultado.Text = "Usuario no encontrado";
                return;
            }

            if (!string.IsNullOrWhiteSpace(usuario.Imagen))
            {
                try
                {
                    byte[] bytesImagen = Convert.FromBase64String(usuario.Imagen);
                    imgPerfil.Source = ImageSource.FromStream(() => new MemoryStream(bytesImagen));
                    imgPerfil.IsVisible = true;
                }
                catch
                {
                    imgPerfil.IsVisible = false;
                }
            }
            else
            {
                imgPerfil.IsVisible = false;
            }
            lblResultado.Text =
                $"Nombres: {usuario.Nombres}\n" +
                $"Apellidos: {usuario.Apellidos}\n" +
                $"Status: {usuario.Status}\n" +
                $"Activo: {usuario.Activo}\n" +
                $"Placa: {usuario.PlacaVehicular}";

            string activo = usuario.Activo?.Trim().ToLower() ?? "";
            bool usuarioActivo = activo == "si" ||
                                 activo == "sí" ||
                                 activo == "SI" ||
                                 activo == "activo" ||
                                 activo == "true";

            btnEntrar.IsEnabled = usuarioActivo;
            btnSalir.IsEnabled = usuarioActivo;
            btnEntrar.Opacity = usuarioActivo ? 1 : 0.4;
            btnSalir.Opacity = usuarioActivo ? 1 : 0.4;
            btnEntrar.BackgroundColor = usuarioActivo ? Colors.Green : Color.FromArgb("#A9DFBF");
            btnSalir.BackgroundColor = usuarioActivo ? Colors.Red : Color.FromArgb("#E6B0AA");
            btnEntrar.TextColor = Colors.White;
            btnSalir.TextColor = Colors.White;
        });
    }

    private async void btnSalir_Clicked(object sender, EventArgs e)
    {
        bool ok = await FirebaseServiceCupos.AumentarAsync(Sede);
        if (!ok)
        {
            await DisplayAlert("Aviso", "Todos los espacios ya están libres.", "OK");
            return;
        }
        await DisplayAlert("OK", "Salida registrada correctamente.", "OK");
        cameraQR.IsDetecting = true;
    }

    private async void btnEntrar_Clicked(object sender, EventArgs e)
    {
        bool ok = await FirebaseServiceCupos.ReducirAsync(Sede);
        if (!ok)
        {
            await DisplayAlert("Lleno", "No hay espacios disponibles.", "OK");
            return;
        }
        await DisplayAlert("OK", "Ingreso registrado correctamente.", "OK");
        cameraQR.IsDetecting = true;
    }
}