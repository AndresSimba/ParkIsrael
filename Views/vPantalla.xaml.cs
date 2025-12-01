using ParkIsrael_Octavo.Services;

namespace ParkIsrael_Octavo.Views
{
    public partial class vPantalla : ContentPage
    {
        private readonly string Sede;
        private bool ejecutando = true;

        public vPantalla(string sede)
        {
            InitializeComponent();
            Sede = sede.ToLower(); // matriz / idiomas / posgrados
            lblTitulo.Text = $"SEDE {sede.ToUpper()}";
            frmContenedor.BackgroundColor = Colors.Gray; // color inicial
        }

        protected override void OnAppearing()
        {
            ejecutando = true;
            _ = IniciarActualizacion();
        }

        protected override void OnDisappearing()
        {
            ejecutando = false;
        }

        private async Task IniciarActualizacion()
        {
            while (ejecutando)
            {
                await ActualizarDatos();
                await Task.Delay(2000); // cada 2 segundos
            }
        }

        private async Task ActualizarDatos()
        {
            //Cupos actuales desde Firebase
            var (cuposActuales, maximo) = await FirebaseServiceCupos.ObtenerCuposAsync(Sede);
            if (cuposActuales < 0)
                return;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                lblCupos.Text = cuposActuales.ToString();
                ActualizarSemaforo(cuposActuales, maximo);
            });
        }

        private void ActualizarSemaforo(int valor, int maximo)
        {
            double porcentaje = (double)valor / maximo;

            Color color;

            if (porcentaje > 0.5)
                color = Color.FromArgb("#27AE60");   // Verde
            else if (porcentaje >= 0.3)
                color = Color.FromArgb("#F1C40F");   // Amarillo
            else
                color = Color.FromArgb("#C0392B");   // Rojo

            // Pintar el frame
            frmContenedor.BackgroundColor = color;

            //Pintar TODA la página con el mismo color
            this.BackgroundColor = color.MultiplyAlpha(0.25f);
        }
    }
}