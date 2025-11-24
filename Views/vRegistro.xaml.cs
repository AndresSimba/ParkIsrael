using ParkIsrael_Octavo.Services;
using ParkIsrael_Octavo.Models;
using ParkIsrael_Octavo.Updates;
using Microsoft.Maui.Graphics;

namespace ParkIsrael_Octavo.Views;

public partial class vRegistro : ContentPage
{
    FirestoreService firestore = new FirestoreService();
    string imagenBase64 = ""; // Para guardar la foto
    public vRegistro()
	{
		InitializeComponent();
	}

    private string _imagenBase64 = "";
    private async void btnGuardar_Clicked(object sender, EventArgs e)
    {
        var model = new UsuarioModel(
        Id: 0, // Lo genera Firestore con autoincremento
        Apellidos: txtApellidos.Text,
        Nombres: txtNombres.Text,
        Cedula: txtCedula.Text,
        Correo: txtCorreo.Text,
        Telefono: txtTelefono.Text,
        Status: pkStatus.SelectedItem?.ToString() ?? "", // o administrador
        TipoVehiculo: pkTipoVehiculo.SelectedItem?.ToString() ?? "", // cambia según tu UI
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

    private async void btnFoto_Clicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Seleccione una imagen"
            });

            if (result == null)
                return;

            using var stream = await result.OpenReadAsync();
            using MemoryStream ms = new MemoryStream();
            await stream.CopyToAsync(ms);

            // Convertir a imagen para poder redimensionar
            var originalImage = Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(ms);
            var resized = originalImage.Resize(250, 250, ResizeMode.Fit);
            imgPerfil.Source = ImageSource.FromStream(() => resized.AsStream());
            using var outStream = resized.AsStream();
            byte[] bytes = new byte[outStream.Length];
            outStream.Read(bytes, 0, bytes.Length);
            _imagenBase64 = Convert.ToBase64String(bytes);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}