using ParkIsrael_Octavo.Models;
using ParkIsrael_Octavo.Services;
using ParkIsrael_Octavo.Updates;

namespace ParkIsrael_Octavo.Views;

public partial class vDatos : ContentPage
{
    private UsuarioModel model = new("", "", "", "");
    private readonly FirestoreService firestore = new();

    public vDatos()
    {
        InitializeComponent();
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        model = model with
        {
            Nombre = NombreEntry.Text ?? "",
            Correo = CorreoEntry.Text ?? "",
            Salario = SalarioEntry.Text ?? ""
        };

        if (string.IsNullOrWhiteSpace(model.Nombre) ||
            string.IsNullOrWhiteSpace(model.Correo) ||
            string.IsNullOrWhiteSpace(model.Salario))
        {
            await DisplayAlert("Error", "Completa todos los campos.", "OK");
            return;
        }

        model = await UsuarioUpdate.GuardarUsuario(model, firestore);
        MensajeLabel.Text = model.Mensaje;
    }
}
