using ParkIsrael_Octavo.Models;
using ParkIsrael_Octavo.Services;


namespace ParkIsrael_Octavo.Views;

public partial class vInformacion : ContentPage
{

    private FirestoreService service = new FirestoreService();

    public vInformacion()
    {
        InitializeComponent();
        CargarUsuarios();
    }

    private async void CargarUsuarios()
    {
        var lista = await service.ObtenerUsuariosAsync();
        // Ordenar alfabéticamente por apellido
        lvUsuarios.ItemsSource = lista
            .OrderBy(u => u.Apellidos)
            .ToList();
    }

    private async void lvUsuarios_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem == null)
            return;
        var usuarioSeleccionado = e.SelectedItem as UsuarioModel;
        if (usuarioSeleccionado == null)
            return;
        // Obtener el documentId desde Firestore
        var (usuarioCompleto, docId) = await service.ObtenerUsuarioPorNombreAsync(usuarioSeleccionado.Usuario);
        if (usuarioCompleto == null)
        {
            await DisplayAlert("Error", "No se pudo cargar el documento del usuario", "OK");
            return;
        }
        await Navigation.PushAsync(new vEditarUsuario(usuarioCompleto, docId));
        ((ListView)sender).SelectedItem = null;
    }

    private void btnRegistro_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new vRegistro());
    }
}