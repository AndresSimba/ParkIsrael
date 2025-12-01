using ParkIsrael_Octavo.Services;

namespace ParkIsrael_Octavo.Views
{
    public partial class vGestion : ContentPage
    {
        public vGestion()
        {
            InitializeComponent();
            CargarMaximos();
        }

        // ---------------------------------------------------------
        //            CARGAR CUPOS MÁXIMOS DESDE FIREBASE
        // ---------------------------------------------------------
        private async void CargarMaximos()
        {
            var (matriz, maxM) = await FirebaseServiceCupos.ObtenerCuposAsync("matriz");
            var (idiomas, maxI) = await FirebaseServiceCupos.ObtenerCuposAsync("idiomas");
            var (posgrados, maxP) = await FirebaseServiceCupos.ObtenerCuposAsync("posgrados");

            txtMaxMatriz.Text = maxM.ToString();
            txtMaxIdiomas.Text = maxI.ToString();
            txtMaxPosgrados.Text = maxP.ToString();
        }

        // ---------------------------------------------------------
        //         GUARDAR NUEVOS CUPOS MÁXIMOS EN FIREBASE
        // ---------------------------------------------------------
        private async void btnGuardarCupo_Clicked(object sender, EventArgs e)
        {
            if (!int.TryParse(txtMaxMatriz.Text, out int maxMatriz) ||
                !int.TryParse(txtMaxIdiomas.Text, out int maxIdiomas) ||
                !int.TryParse(txtMaxPosgrados.Text, out int maxPosgrados))
            {
                await DisplayAlert("Error", "Ingrese solo números válidos.", "OK");
                return;
            }

            bool ok = await FirebaseServiceCupos.GuardarMaximosAsync(
                maxMatriz, maxIdiomas, maxPosgrados);

            if (!ok)
            {
                await DisplayAlert("Error", "No se pudieron guardar los cupos máximos.", "OK");
                return;
            }

            await DisplayAlert("OK", "Cupos máximos actualizados correctamente.", "Aceptar");
        }

        // ---------------------------------------------------------
        //   REINICIAR CUPOS ACTUALES AL MÁXIMO EN FIREBASE
        // ---------------------------------------------------------
        private async void btnReinCupo_Clicked(object sender, EventArgs e)
        {
            bool confirmar = await DisplayAlert(
                "Reiniciar cupos",
                "¿Desea reiniciar los cupos actuales al máximo en todas las sedes?",
                "Sí", "No");

            if (!confirmar) return;

            bool ok = await FirebaseServiceCupos.ReiniciarCuposAsync();

            if (!ok)
            {
                await DisplayAlert("Error", "No se pudieron reiniciar los cupos.", "OK");
                return;
            }

            await DisplayAlert("OK", "Cupos reiniciados correctamente.", "Aceptar");
        }

        // ---------------------------------------------------------
        //               CERRAR VISTA
        // ---------------------------------------------------------
        private async void btnCerrar_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}