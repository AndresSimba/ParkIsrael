using ParkIsrael_Octavo.Models;
using ParkIsrael_Octavo.Services;

namespace ParkIsrael_Octavo.Updates
{
    public static class UsuarioUpdate
    {
        public static async Task<UsuarioModel> GuardarUsuario(UsuarioModel model, FirestoreService service)
        {
            bool ok = await service.GuardarUsuarioAsync(model.Nombre, model.Correo, model.Salario);
            string msg = ok ? "Guardado correctamente en Firestore" : "Error al guardar";

            return model with { Mensaje = msg };
        }
    }
}
