namespace ParkIsrael_Octavo.Models
{
    public record UsuarioModel(
        int Id,// autoincremental
        string Apellidos,
        string Nombres,
        string Cedula,
        string Telefono,
        string Correo,
        string Status,// administrador / estudiante
        string TipoVehiculo,
        string PlacaVehicular,
        string Usuario,
        string Contrasena,
        string Activo,// "Si"
        string Imagen,// Base64
        string Mensaje// Respuesta del servidor
    );
}
