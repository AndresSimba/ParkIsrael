namespace ParkIsrael_Octavo.Views;

public partial class vLogin : ContentPage
{
	public vLogin()
	{
        InitializeComponent();
	}

    private async void btnIngresar_Clicked(object sender, EventArgs e)
    {
		try
        {
            
            if (string.IsNullOrWhiteSpace(txtUsuario.Text) ||
                string.IsNullOrWhiteSpace(txtContrasena.Text))
            {
                await DisplayAlert("Campos incompletos",
                    "Por favor ingrese usuario y contraseña.",
                    "Aceptar");
                return; 
            }

            
            await Navigation.PushAsync(new vAcceso());
            await DisplayAlert("Bienvenido", "", "Cerrar");
        }
        catch (Exception ex)
		{

            Console.WriteLine($"Error al Ingresar: {ex.Message}");
        }

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