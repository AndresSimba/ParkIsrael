using ParkIsrael_Octavo.Models;

namespace ParkIsrael_Octavo.Views;

public partial class vAcceso : ContentPage
{
    private UsuarioModel Usuario; // Guardamos los datos
    public vAcceso(UsuarioModel usuario)
    {
        InitializeComponent();
        Usuario = usuario;
        CargarDatos();
    }
        private void CargarDatos()
    {
        lblNombre.Text = $"{Usuario.Nombres} {Usuario.Apellidos}";
        lblCedula.Text = Usuario.Cedula;
        lblTelefono.Text = Usuario.Telefono;
        lblCorreo.Text = Usuario.Correo;
        lblStatus.Text = Usuario.Status;
        lblTipoVehiculo.Text = Usuario.TipoVehiculo;
        lblPlaca.Text = Usuario.PlacaVehicular;
        lblEstado.Text= Usuario.Activo;

        if (Usuario.Activo != null && Usuario.Activo.Trim().ToUpper() == "SI")
        {
            lblEstado.Text = "ACTIVO";
            lblEstado.TextColor = Colors.Green;
        }
        else
        {
            lblEstado.Text = "INACTIVO";
            lblEstado.TextColor = Colors.Red;
        }

        // Cargar imagen Base64
        if (!string.IsNullOrEmpty(Usuario.Imagen))
        {
            byte[] bytes = Convert.FromBase64String(Usuario.Imagen);
            imgPerfil.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
        }
    }

    // Coordenadas fijas de las sedes
    readonly Location sede1 = new Location(-0.197869, -78.492637);
    readonly Location sede2 = new Location(-0.19541657395283885, -78.495295430271);
    readonly Location sede3 = new Location(-0.144174, -78.508148);

    private async Task AbrirGoogleMaps(Location destino)
    {
        try
        {
            var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));
            if (location == null)
            {
                await DisplayAlert("Error", "No se pudo obtener tu ubicación.", "OK");
                return;
            }

            string url = $"https://www.google.com/maps/dir/?api=1" + 
                $"&origin={location.Latitude},{location.Longitude}" +
                $"&destination={destino.Latitude},{destino.Longitude}" +
                $"&travelmode=driving";
            await Launcher.Default.OpenAsync(url);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void btnGenerarQR_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new vQR(Usuario));
    }

    private async void btnSede_1_Clicked(object sender, EventArgs e)
    {
        await AbrirGoogleMaps(sede1);
    }

    private async void btnSede_2_Clicked(object sender, EventArgs e)
    {
        await AbrirGoogleMaps(sede2);
    }

    private async void btnSede_3_Clicked(object sender, EventArgs e)
    {
        await AbrirGoogleMaps(sede3);
    }
}