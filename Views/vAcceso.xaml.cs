using QRCoder;
namespace ParkIsrael_Octavo.Views;

public partial class vAcceso : ContentPage
{
    public vAcceso()
    {
        InitializeComponent();
    }

    // Coordenadas fijas de las sedes
    readonly Location sede1 = new Location(-0.197869, -78.492637);
    readonly Location sede2 = new Location(-0.19541657395283885, -78.495295430271);
    readonly Location sede3 = new Location(-0.144174, -78.508148);

    private async Task AbrirGoogleMaps(Location destino)
    {
        try
        {
            var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));
            if (location == null)
            {
                await DisplayAlert("Error", "No se pudo obtener tu ubicación.", "OK");
                return;
            }

            string url = $"https://www.google.com/maps/dir/?api=1" + 
                $"&origin={location.Latitude},{location.Longitude}" +
                $"&destination={destino.Latitude},{destino.Longitude}" +
                $"&travelmode=driving";
            await Launcher.Default.OpenAsync(url);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void btnGenerarQR_Clicked(object sender, EventArgs e)
    {
        try
        {
            /*string contenidoQR = "ACCESO-PARK-ISRAEL";
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(contenidoQR, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrBytes = qrCode.GetGraphic(10);
            ImageQR.Source = ImageSource.FromStream(() => new MemoryStream(qrBytes));*/
            Navigation.PushAsync(new vQR());
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void btnSede_1_Clicked(object sender, EventArgs e)
    {
        await AbrirGoogleMaps(sede1);
    }

    private async void btnSede_2_Clicked(object sender, EventArgs e)
    {
        await AbrirGoogleMaps(sede2);
    }

    private async void btnSede_3_Clicked(object sender, EventArgs e)
    {
        await AbrirGoogleMaps(sede3);
    }
}