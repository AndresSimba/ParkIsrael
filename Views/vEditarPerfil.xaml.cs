using ParkIsrael_Octavo.Models;
using ParkIsrael_Octavo.Services;

namespace ParkIsrael_Octavo.Views;

public partial class vEditarPerfil : ContentPage
{
    public UsuarioModel Usuario { get; private set; }

    private string NuevaImagenBase64;
    private string documentId;

    public vEditarPerfil(UsuarioModel usuario, string docId)
    {
        InitializeComponent();

        Usuario = usuario;
        documentId = docId;
        BindingContext = Usuario;
        pkTipoVehiculo.SelectedItem = Usuario.TipoVehiculo;
        CargarImagen();
    }

    private void CargarImagen()
    {
        try
        {
            if (!string.IsNullOrEmpty(Usuario.Imagen))
            {
                byte[] bytes = Convert.FromBase64String(Usuario.Imagen);
                imgPerfil.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            else
            {
                imgPerfil.Source = "usuario_default.png";
            }
        }
        catch
        {
            imgPerfil.Source = "usuario_default.png";
        }
    }

    private async void btnActualizar_Clicked(object sender, EventArgs e)
    {
        try
        {
            string imagenFinal = NuevaImagenBase64 ?? Usuario.Imagen;
            var usuarioActualizado = new UsuarioModel(
                Usuario.Id,
                txtApellidos.Text,
                txtNombres.Text,
                txtCedula.Text,
                txtTelefono.Text,
                txtCorreo.Text,
                // Mantener status original
                Usuario.Status,
                pkTipoVehiculo.SelectedItem?.ToString() ?? Usuario.TipoVehiculo,
                txtPlaca.Text,
                txtUsuario.Text,
                txtContrasena.Text,
                // Mantener activo original
                Usuario.Activo,
                imagenFinal,
                // Mantener mensaje original
                Usuario.Mensaje
            );

            var firestore = new FirestoreService();
            bool ok = await firestore.ActualizarUsuario(documentId, usuarioActualizado);
            if (ok)
            {
                await DisplayAlert("OK", "Tus datos fueron actualizados correctamente", "Aceptar");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error", "No se pudieron actualizar tus datos", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    private async void btnCambImg_Clicked(object sender, EventArgs e)
    {
        try
        {
            var resultado = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images
            });
            if (resultado == null)
                return;
            using var stream = await resultado.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();
            imgPerfil.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
            NuevaImagenBase64 = Convert.ToBase64String(bytes);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}