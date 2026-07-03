using QRCoder;
using ParkIsrael_Octavo.Models;

namespace ParkIsrael_Octavo.Views;

public partial class vQR : ContentPage
{
    private UsuarioModel Usuario; // Recibimos datos
    private string DocumentId;
    public vQR(UsuarioModel usuario, string documentId)
	{
		InitializeComponent();
        Usuario = usuario;
        DocumentId = documentId;
        GenerarQR();
    }

    private void GenerarQR()
    {
        try
        {
            // Construimos un texto con todos los datos del usuario
            string contenidoQR = DocumentId;

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(contenidoQR, QRCodeGenerator.ECCLevel.H);
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