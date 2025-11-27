using ParkIsrael_Octavo.Models;
using ParkIsrael_Octavo.Services;
using System.IO;

namespace ParkIsrael_Octavo.Views;

public partial class vEditarUsuario : ContentPage
{
    public UsuarioModel Usuario { get; private set; }
    private string NuevaImagenBase64;
    private string documentId;

    public vEditarUsuario(UsuarioModel usuario, string docId)
    {
        InitializeComponent();
        Usuario = usuario;
        documentId = docId;
        BindingContext = Usuario;
        pkActivo.SelectedItem = Usuario.Activo;
        pkStatus.SelectedItem = Usuario.Status;
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
                pkStatus.SelectedItem?.ToString() ?? Usuario.Status,
                pkTipoVehiculo.SelectedItem?.ToString() ?? Usuario.TipoVehiculo,
                txtPlaca.Text,
                txtUsuario.Text,
                txtContrasena.Text,
                pkActivo.SelectedItem?.ToString() ?? Usuario.Activo,
                imagenFinal,
                Usuario.Mensaje
            );

            var firestore = new FirestoreService();
            // Convertimos el Id a string
            bool ok = await firestore.ActualizarUsuario(documentId, usuarioActualizado);
            if (ok)
            {
                await DisplayAlert("OK", "Usuario actualizado correctamente", "Aceptar");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error", "No se pudo actualizar el usuario", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    private async void btnEliminar_Clicked(object sender, EventArgs e)
    {
        bool confirmar = await DisplayAlert("Confirmar", "¿Seguro que desea eliminar este usuario?", "Sí", "No");
        if (!confirmar)
            return;
        try
        {
            var firestore = new FirestoreService();
            bool ok = await firestore.EliminarUsuario(documentId);
            if (ok)
            {
                await DisplayAlert("Éxito", "Usuario eliminado correctamente.", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error", "No se pudo eliminar el usuario.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
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