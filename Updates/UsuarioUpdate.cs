using ParkIsrael_Octavo.Models;
using ParkIsrael_Octavo.Services;

namespace ParkIsrael_Octavo.Updates
{
    public static class UsuarioUpdate
    {
        public static async Task<UsuarioModel> GuardarUsuario(UsuarioModel model, FirestoreService service)
        {
            bool ok = await service.GuardarUsuarioAsync(
                model.Apellidos,
                model.Nombres,
                model.Cedula,
                model.Telefono,
                model.Correo,
                model.Status,
                model.TipoVehiculo,
                model.PlacaVehicular,
                model.Usuario,
                model.Contrasena,
                model.Imagen
            );

            string msg = ok ? "Guardado correctamente en Firestore" : "Error al guardar";

            return model with { Mensaje = msg };
        }
    }
}
