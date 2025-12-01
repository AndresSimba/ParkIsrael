using ParkIsrael_Octavo.Services;

namespace ParkIsrael_Octavo.Views
{
    public partial class vControl : ContentPage
    {
        public vControl()
        {
            InitializeComponent();

            // Estado inicial
            btnEntrada.IsEnabled = false;
            btnSalida.IsEnabled = false;
            btnEntrada.Opacity = 0.4;
            btnSalida.Opacity = 0.4;

            pkSede.SelectedIndexChanged += pkSede_SelectedIndexChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ActualizarUIFirebase();
        }

        // -----------------------------
        //          ENTRADA
        // -----------------------------
        private async void btnEntrada_Clicked(object sender, EventArgs e)
        {
            if (pkSede.SelectedIndex == -1) return;

            string sede = pkSede.SelectedItem.ToString().ToLower();

            bool ok = await FirebaseServiceCupos.ReducirAsync(sede);

            if (!ok)
            {
                await DisplayAlert("Lleno", "No hay más espacios disponibles.", "OK");
                return;
            }

            await ActualizarUIFirebase();
        }

        // -----------------------------
        //          SALIDA
        // -----------------------------
        private async void btnSalida_Clicked(object sender, EventArgs e)
        {
            if (pkSede.SelectedIndex == -1) return;

            string sede = pkSede.SelectedItem.ToString().ToLower();

            bool ok = await FirebaseServiceCupos.AumentarAsync(sede);

            if (!ok)
            {
                await DisplayAlert("Vacío", "Todos los espacios ya están libres.", "OK");
                return;
            }

            await ActualizarUIFirebase();
        }

        // -----------------------------
        //  HABILITAR / DESHABILITAR BOTONES
        // -----------------------------
        private void pkSede_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool haySede = pkSede.SelectedIndex != -1;

            btnEntrada.IsEnabled = haySede;
            btnSalida.IsEnabled = haySede;

            btnEntrada.Opacity = haySede ? 1 : 0.4;
            btnSalida.Opacity = haySede ? 1 : 0.4;
        }

        // -----------------------------
        //    REINICIAR CUPOS EN FIREBASE
        // -----------------------------
        private async void btnReiniciar_Clicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert(
                "Reiniciar",
                "¿Desea reiniciar los cupos al máximo en todas las sedes?",
                "Sí", "No");

            if (!confirmar) return;

            await FirebaseServiceCupos.ReiniciarCuposAsync();
            await ActualizarUIFirebase();

            await DisplayAlert("OK", "Cupos reiniciados correctamente", "Aceptar");
        }

        // -----------------------------
        //       ANIMACIÓN (opcional)
        // -----------------------------
        private async Task AnimarLabel(Label label, bool aumento)
        {
            var colorOriginal = label.TextColor;

            label.TextColor = aumento ? Colors.LawnGreen : Colors.IndianRed;

            await label.ScaleTo(1.3, 150, Easing.CubicIn);
            await label.ScaleTo(1.0, 150, Easing.CubicOut);

            label.TextColor = colorOriginal;
        }

        // -----------------------------
        //     ACTUALIZAR PANTALLA
        // -----------------------------
        private async Task ActualizarUIFirebase()
        {
            var (m, maxM) = await FirebaseServiceCupos.ObtenerCuposAsync("matriz");
            var (i, maxI) = await FirebaseServiceCupos.ObtenerCuposAsync("idiomas");
            var (p, maxP) = await FirebaseServiceCupos.ObtenerCuposAsync("posgrados");

            lblMatriz.Text = m.ToString();
            lblIdiomas.Text = i.ToString();
            lblPosgrado.Text = p.ToString();

            // Semáforo
            ActualizarColor(lblMatriz, new CupoItem(m, maxM));
            ActualizarColor(lblIdiomas, new CupoItem(i, maxI));
            ActualizarColor(lblPosgrado, new CupoItem(p, maxP));
        }

        // -----------------------------
        //   NAVEGAR A PANTALLA PUBLICA
        // -----------------------------
        private async void btnPantalla_Clicked(object sender, EventArgs e)
        {
            if (pkSede.SelectedIndex == -1)
            {
                await DisplayAlert("Aviso", "Seleccione una sede primero.", "OK");
                return;
            }

            string sede = pkSede.SelectedItem.ToString().ToLower();
            await Navigation.PushAsync(new vPantalla(sede));
        }

        // -----------------------------
        //        SEMÁFORO
        // -----------------------------
        private void ActualizarColor(Label label, CupoItem item)
        {
            double porcentaje = (double)item.cupos / item.max;

            if (porcentaje > 0.5)
                label.TextColor = Colors.LawnGreen;    // Verde
            else if (porcentaje > 0.2)
                label.TextColor = Colors.Gold;        // Amarillo
            else
                label.TextColor = Colors.IndianRed;   // Rojo
        }
    }

    // Modelo simple para semáforo
    public class CupoItem
    {
        public int cupos { get; set; }
        public int max { get; set; }

        public CupoItem(int cupos, int max)
        {
            this.cupos = cupos;
            this.max = max;
        }
    }
}
