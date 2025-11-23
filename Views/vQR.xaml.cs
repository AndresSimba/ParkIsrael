using QRCoder;
namespace ParkIsrael_Octavo.Views;

public partial class vQR : ContentPage
{
	public vQR()
	{
		InitializeComponent();
        GenerarQR();
    }

    private void GenerarQR()
    {
        try
        {
            string contenidoQR = "ACCESO-PARK-ISRAEL"; // QR fijo
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(contenidoQR, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrBytes = qrCode.GetGraphic(20);
            imgQR.Source = ImageSource.FromStream(() => new MemoryStream(qrBytes));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR QR: {ex.Message}");
        }
    }
}