namespace ParkIsrael_Octavo.Views;

public partial class vRegistro : ContentPage
{
	public vRegistro()
	{
		InitializeComponent();
	}

    private async void btnGuardar_Clicked(object sender, EventArgs e)
    {
		try
		{
            if (string.IsNullOrWhiteSpace(txtCedula.Text) ||
                string.IsNullOrWhiteSpace(txtNombres.Text) ||
                string.IsNullOrWhiteSpace(txtApellidos.Text)||
                string.IsNullOrWhiteSpace(txtPlaca.Text)||
                string.IsNullOrWhiteSpace(txtUsuario.Text)||
                string.IsNullOrWhiteSpace(txtContraseña.Text))
            {
                await DisplayAlert("Campos incompletos",
                    "Llenar todos los Campos",
                    "Aceptar");
                return;
            }

            await Navigation.PushAsync(new vLogin());
		}
		catch (Exception ex)
		{
            Console.WriteLine($"Error al Ingresar: {ex.Message}");
        }
    }
}