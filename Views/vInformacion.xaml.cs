using ParkIsrael_Octavo.Models;
using ParkIsrael_Octavo.Services;

namespace ParkIsrael_Octavo.Views;

public partial class vInformacion : ContentPage
{
    // Servicio para consultar la información de usuarios en Firestore
    private FirestoreService service = new FirestoreService();

    // Lista original de usuarios obtenida desde la base de datos
    private List<UsuarioModel> listaOriginal = new();

    // Variable para controlar el filtro de estado: todos, activos o inactivos
    private string filtroEstado = "todos";

    public vInformacion()
    {
        InitializeComponent();
        // Selecciona por defecto la búsqueda por apellido
        pkTipoBusqueda.SelectedIndex = 0;
        // Carga inicial de usuarios desde la base de datos
        CargarUsuarios();
    }

    private async void CargarUsuarios()
    {
        // Llamada a la base de datos para obtener todos los usuarios
        listaOriginal = await service.ObtenerUsuariosAsync();

        // Ordena la lista: primero administradores y luego por apellidos
        listaOriginal = listaOriginal
            .OrderBy(u => u.Status != "administrador")
            .ThenBy(u => u.Apellidos)
            .ToList();
        // Aplica los filtros actuales y actualiza la tabla
        AplicarFiltros();
    }

    private void AplicarFiltros()
    {
        // Obtiene el texto ingresado en el buscador
        string texto = txtBuscar.Text?.Trim().ToLower() ?? "";
        // Obtiene el tipo de búsqueda seleccionado: Apellido, Cédula o Placa
        string tipoBusqueda = pkTipoBusqueda.SelectedItem?.ToString() ?? "Apellido";
        // Parte desde la lista original para no perder información
        IEnumerable<UsuarioModel> lista = listaOriginal;

        // Filtra solo usuarios activos
        if (filtroEstado == "activos")
        {
            lista = lista.Where(u => (u.Activo ?? "").Trim().ToLower() == "si");
        }
        else if (filtroEstado == "inactivos")
        {
            lista = lista.Where(u => (u.Activo ?? "").Trim().ToLower() != "si");
        }
        // Aplica búsqueda solo si el usuario escribió texto
        if (!string.IsNullOrWhiteSpace(texto))
        {
            // Buscar por apellido
            if (tipoBusqueda == "Apellido")
            {
                lista = lista.Where(u => (u.Apellidos ?? "").ToLower().Contains(texto));
            }
            // Buscar por cédula
            else if (tipoBusqueda == "Cédula")
            {
                lista = lista.Where(u => (u.Cedula ?? "").ToLower().Contains(texto));
            }
            // Buscar por placa vehicular
            else if (tipoBusqueda == "Placa")
            {
                lista = lista.Where(u => (u.PlacaVehicular ?? "").ToLower().Contains(texto));
            }
        }
        // Convierte el resultado filtrado en lista
        var resultado = lista.ToList();
        // Muestra los usuarios filtrados en la tabla
        lvUsuarios.ItemsSource = resultado;
        // Actualiza el total real de usuarios visibles
        lblTotal.Text = $"Total: {resultado.Count} usuarios";
    }

    private async void lvUsuarios_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        // Evita errores si no hay ningún usuario seleccionado
        if (e.SelectedItem == null)
            return;
        // Convierte el elemento seleccionado al modelo UsuarioModel
        var usuarioSeleccionado = e.SelectedItem as UsuarioModel;
        if (usuarioSeleccionado == null)
            return;
        // Obtiene nuevamente el usuario completo y su documentId desde Firestor
        var (usuarioCompleto, docId) = await service.ObtenerUsuarioPorNombreAsync(usuarioSeleccionado.Usuario);
        // Valida que se haya encontrado el documento del usuario
        if (usuarioCompleto == null)
        {
            await DisplayAlert("Error", "No se pudo cargar el documento del usuario", "OK");
            return;
        }
        // Navega a la pantalla de edición enviando el usuario y el documentId
        await Navigation.PushAsync(new vEditarUsuario(usuarioCompleto, docId));
        // Limpia la selección del ListView para evitar que quede marcada
        ((ListView)sender).SelectedItem = null;
    }
    
    // Navega a la pantalla de registro de usuario
    private async void btnRegistro_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new vRegistro());
    }

    // Aplica filtros cuando cambia el tipo de búsqueda
    private void pkTipoBusqueda_SelectedIndexChanged(object sender, EventArgs e)
    {
        AplicarFiltros();
    }

    // Filtra automáticamente mientras el usuario escribe
    private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
    {
        AplicarFiltros();
    }

    // Limpia el texto de búsqueda
    private void btnLimpiar_Clicked(object sender, EventArgs e)
    {
        txtBuscar.Text = "";
        // Regresa el tipo de búsqueda a Apellido
        pkTipoBusqueda.SelectedIndex = 0;
        // Regresa el filtro de estado a Todos
        filtroEstado = "todos";
        // Pinta el botón Todos como seleccionado
        PintarBotonEstado("todos");
        // Recarga la lista sin filtros
        AplicarFiltros();
    }

    // Muestra todos los usuarios
    private void btnTodos_Clicked(object sender, EventArgs e)
    {
        filtroEstado = "todos";
        PintarBotonEstado("todos");
        AplicarFiltros();
    }

    // Muestra solo usuarios activos
    private void btnActivos_Clicked(object sender, EventArgs e)
    {
        filtroEstado = "activos";
        PintarBotonEstado("activos");
        AplicarFiltros();
    }

    // Muestra solo usuarios inactivos
    private void btnInactivos_Clicked(object sender, EventArgs e)
    {
        filtroEstado = "inactivos";
        PintarBotonEstado("inactivos");
        AplicarFiltros();
    }

    private void PintarBotonEstado(string estado)
    {
        // Estilo visual del botón Todos
        btnTodos.BackgroundColor = estado == "todos" ? Color.FromArgb("#4B2EDB") : Colors.White;
        btnTodos.TextColor = estado == "todos" ? Colors.White : Color.FromArgb("#4B2EDB");
        btnTodos.BorderColor = Color.FromArgb("#4B2EDB");
        // Estilo visual del botón Activos
        btnActivos.BackgroundColor = estado == "activos" ? Color.FromArgb("#27AE60") : Colors.White;
        btnActivos.TextColor = estado == "activos" ? Colors.White : Color.FromArgb("#27AE60");
        btnActivos.BorderColor = Color.FromArgb("#27AE60");
        // Estilo visual del botón Inactivos
        btnInactivos.BackgroundColor = estado == "inactivos" ? Color.FromArgb("#E74C3C") : Colors.White;
        btnInactivos.TextColor = estado == "inactivos" ? Colors.White : Color.FromArgb("#E74C3C");
        btnInactivos.BorderColor = Color.FromArgb("#E74C3C");
    }

    // Vuelve a consultar la base de datos y refresca la tabla
    private void btnRecargar_Clicked(object sender, EventArgs e)
    {
        CargarUsuarios();
    }
}