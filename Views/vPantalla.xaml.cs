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

            Sede = sede.ToLower();
            lblTitulo.Text = $"SEDE {sede.ToUpper()}";
            frmContenedor.BackgroundColor = Colors.Gray;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ejecutando = true;
            _ = IniciarActualizacion();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            ejecutando = false;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            AjustarPantalla(width, height);
        }

        private void AjustarPantalla(double width, double height)
        {
            if (width <= 0 || height <= 0)
                return;

            double tamanioFrame = Math.Min(width * 0.75, height * 0.55);

            frmContenedor.WidthRequest = tamanioFrame;
            frmContenedor.HeightRequest = tamanioFrame;

            lblTitulo.FontSize = Math.Max(24, Math.Min(width * 0.10, 50));

            lblCupos.FontSize = Math.Max(70, tamanioFrame * 0.48);
        }

        private async Task IniciarActualizacion()
        {
            while (ejecutando)
            {
                await ActualizarDatos();
                await Task.Delay(2000);
            }
        }

        private async Task ActualizarDatos()
        {
            var (cuposActuales, maximo) = await FirebaseServiceCupos.ObtenerCuposAsync(Sede);

            if (cuposActuales < 0 || maximo <= 0)
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
                color = Color.FromArgb("#27AE60");
            else if (porcentaje >= 0.3)
                color = Color.FromArgb("#F1C40F");
            else
                color = Color.FromArgb("#C0392B");

            frmContenedor.BackgroundColor = color;
            this.BackgroundColor = color.MultiplyAlpha(0.25f);
        }
    }
}