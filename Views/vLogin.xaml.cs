using ParkIsrael_Octavo.Services;
using ParkIsrael_Octavo.Models;

namespace ParkIsrael_Octavo.Views;

public partial class vLogin : ContentPage
{
	public vLogin()
	{
        InitializeComponent();
	}

    private async void btnIngresar_Clicked(object sender, EventArgs e)
    {
        string usuario = txtUsuario.Text;
        string contra = txtContrasena.Text;

        var service = new FirestoreService();
        var login = await service.LoginAsync(usuario, contra);

        if (!login.ok)
        {
            await DisplayAlert("Error", login.mensaje, "OK");
            return;
        }

        //si el usuario es administrador va a paras a la View vAdmin
        if (login.status == "administrador")
        {
            await Navigation.PushAsync(new vAdmin());
            return;
        }

        //si el usuario es estudiante pasa a la View vAcceso y carga los datos del usuario
        if (login.status == "estudiante")
        {
            UsuarioModel? datosUsuario = await service.ObtenerUsuarioPorNombreAsync(usuario);
            if (datosUsuario is null)
            {
                await DisplayAlert("Error", "No se pudo cargar la información del usuario", "OK");
                return;
            }
            await Navigation.PushAsync(new vAcceso(datosUsuario));
            return;
        }
        await DisplayAlert("Error", "Status desconocido en la base", "OK");
    }

    private async void btnRegistro_Clicked(object sender, EventArgs e)
    {
		try
		{
            await Navigation.PushAsync(new vRegistro());
        }
		catch (Exception ex)
		{
            Console.WriteLine($"Error al Regsitrar: {ex.Message}");
        }
    }
}