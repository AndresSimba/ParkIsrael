using QRCoder;
using ParkIsrael_Octavo.Models;
namespace ParkIsrael_Octavo.Views;

public partial class vQR : ContentPage
{
    private UsuarioModel Usuario; // Recibimos datos
    public vQR(UsuarioModel usuario)
	{
		InitializeComponent();
        Usuario = usuario;
        GenerarQR();
    }

    private void GenerarQR()
    {
        try
        {
            // Construimos un texto con todos los datos del usuario
            string contenidoQR =
                $"Nombres: {Usuario.Nombres}\n" +
                $"Apellidos: {Usuario.Apellidos}\n" +
                $"Cédula: {Usuario.Cedula}\n" +
                $"Teléfono: {Usuario.Telefono}\n" +
                $"Correo: {Usuario.Correo}\n" +
                $"Status: {Usuario.Status}\n" +
                $"Estado: {Usuario.Activo}\n" +
                $"Tipo Vehículo: {Usuario.TipoVehiculo}\n" +
                $"Placa: {Usuario.PlacaVehicular}";

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