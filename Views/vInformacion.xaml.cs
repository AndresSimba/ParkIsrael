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
    }
}