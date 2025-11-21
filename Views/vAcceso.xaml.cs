
using QRCoder;

namespace ParkIsrael_Octavo.Views;

public partial class vAcceso : ContentPage
{
    public vAcceso()
    {
        InitializeComponent();
    }



    private async void btnSiguiente_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new vUbiSede());
    }

    private void btnGenerarQR_Clicked(object sender, EventArgs e)
    {
        try
        {
            string contenidoQR = "ACCESO-PARK-ISRAEL";

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(contenidoQR, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);

            byte[] qrBytes = qrCode.GetGraphic(10);

            ImageQR.Source = ImageSource.FromStream(() => new MemoryStream(qrBytes));
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", ex.Message, "OK");
        }
    }
}