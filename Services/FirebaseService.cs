using Newtonsoft.Json;
using ParkIsrael_Octavo.Models;
using System;
using System.Buffers.Text;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace ParkIsrael_Octavo.Services
{
    public class FirestoreService
    {
        private readonly string projectId = "parkisrael-aaa09";
        private readonly string collection = "usuarios";
        private readonly HttpClient client = new();

        private async Task<int> ObtenerNuevoIdAsync()
        {
            try
            {
                string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/counters/usuarios";
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    throw new Exception("No se pudo obtener el contador en Firestore.");
                dynamic data = JsonConvert.DeserializeObject(content);
                int lastId = int.Parse((string)data.fields.lastId.integerValue);
                int newId = lastId + 1;

                // Actualizar el contador en Firestore
                var updateData = new
                {
                    fields = new
                    {
                        lastId = new { integerValue = newId.ToString() }
                    }
                };
                var json = JsonConvert.SerializeObject(updateData);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                var updateResponse = await client.PatchAsync(url, httpContent);
                if (!updateResponse.IsSuccessStatusCode)
                    throw new Exception("Error actualizando el contador.");
                return newId;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ObtenerNuevoIdAsync(): {ex.Message}");
                return -1; // señal de error
            }
        }

        public async Task<bool> GuardarUsuarioAsync(string apellidos, string nombres, string cedula, string telefono, string correo, string status, string tipoVehiculo,
                                                    string placaVehicular, string usuario, string contrasena, string imagenBase64)
        {
            try
            {
                // 1. Obtener ID autoincremental
                int nuevoId = await ObtenerNuevoIdAsync();
                if (nuevoId == -1)
                    return false;

                // 2. Crear el documento con TODOS los campos
                var document = new
                {
                    fields = new
                    {
                        id = new { integerValue = nuevoId.ToString() },
                        apellidos = new { stringValue = apellidos },
                        nombres = new { stringValue = nombres },
                        cedula = new { stringValue = cedula },
                        telefono = new { stringValue = telefono },
                        correo = new { stringValue = correo },
                        status = new { stringValue = status }, // administrador o estudiante
                        tipoVehiculo = new { stringValue = tipoVehiculo },
                        placaVehicular = new { stringValue = placaVehicular },
                        usuario = new { stringValue = usuario },
                        contrasena = new { stringValue = contrasena },
                        activo = new { stringValue = "Si" }, // Por defecto "Si"
                        imagen = new { stringValue = imagenBase64 } // foto base64
                    }
                };
                var json = JsonConvert.SerializeObject(document);
                string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/{collection}";
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var responseText = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Error Firestore: {responseText}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GuardarUsuarioAsync(): {ex.Message}");
                return false;
            }
        }

        public async Task<(bool ok, string status, string mensaje)> LoginAsync(string usuario, string contrasena)
        {
            try
            {
                string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents:runQuery";

                var query = new
                {
                    structuredQuery = new
                    {
                        from = new[] { new { collectionId = "usuarios" } },
                        where = new
                        {
                            fieldFilter = new
                            {
                                field = new { fieldPath = "usuario" },
                                op = "EQUAL",
                                value = new { stringValue = usuario }
                            }
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(query);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return (false, "", "Error consultando Firestore");

                dynamic data = JsonConvert.DeserializeObject(responseText);

                // 🔥 Validación correcta: Firestore muchas veces devuelve [{}] cuando NO encuentra nada
                if (data == null || data.Count == 0 || data[0].document == null)
                    return (false, "", "Usuario no encontrado");

                string passFirestore = data[0].document.fields.contrasena.stringValue;
                string status = data[0].document.fields.status.stringValue;

                if (passFirestore != contrasena)
                    return (false, "", "Contraseña incorrecta");

                return (true, status, "Login correcto");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error LoginAsync(): {ex.Message}");
                return (false, "", "Error interno en el login");
            }
        }

        public async Task<(UsuarioModel? usuario, string documentId)> ObtenerUsuarioPorNombreAsync(string usuario)
        {
            try
            {
                string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents:runQuery";

                var query = new
                {
                    structuredQuery = new
                    {
                        from = new[] { new { collectionId = "usuarios" } },
                        where = new
                        {
                            fieldFilter = new
                            {
                                field = new { fieldPath = "usuario" },
                                op = "EQUAL",
                                value = new { stringValue = usuario }
                            }
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(query);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return (null, "");

                dynamic data = JsonConvert.DeserializeObject(responseText);

                // 🔥 validación correcta
                if (data == null || data.Count == 0 || data[0].document == null)
                    return (null, "");

                var doc = data[0].document;
                string docId = ((string)doc.name).Split('/').Last();

                var fields = doc.fields;

                var usuarioModel = new UsuarioModel(
                    Id: int.Parse((string)fields.id.integerValue),
                    Apellidos: (string)fields.apellidos.stringValue,
                    Nombres: (string)fields.nombres.stringValue,
                    Cedula: (string)fields.cedula.stringValue,
                    Telefono: (string)fields.telefono.stringValue,
                    Correo: (string)fields.correo.stringValue,
                    Status: (string)fields.status.stringValue,
                    TipoVehiculo: (string)fields.tipoVehiculo.stringValue,
                    PlacaVehicular: (string)fields.placaVehicular.stringValue,
                    Usuario: (string)fields.usuario.stringValue,
                    Contrasena: (string)fields.contrasena.stringValue,
                    Activo: (string)fields.activo.stringValue,
                    Imagen: (string)fields.imagen.stringValue,
                    Mensaje: ""
                );

                return (usuarioModel, docId);
            }
            catch
            {
                return (null, "");
            }
        }

        public async Task<List<UsuarioModel>> ObtenerUsuariosAsync()
        {
            List<UsuarioModel> lista = new();
            try
            {
                string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/{collection}";
                var response = await client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return lista;

                dynamic data = JsonConvert.DeserializeObject(json);

                foreach (var item in data.documents)
                {
                    var f = item.fields;

                    lista.Add(new UsuarioModel(
                        Id: int.Parse((string)f.id.integerValue),
                        Apellidos: (string)f.apellidos.stringValue,
                        Nombres: (string)f.nombres.stringValue,
                        Cedula: (string)f.cedula.stringValue,
                        Telefono: (string)f.telefono.stringValue,
                        Correo: (string)f.correo.stringValue,
                        Status: (string)f.status.stringValue,
                        TipoVehiculo: (string)f.tipoVehiculo.stringValue,
                        PlacaVehicular: (string)f.placaVehicular.stringValue,
                        Usuario: (string)f.usuario.stringValue,
                        Contrasena: (string)f.contrasena.stringValue,
                        Activo: (string)f.activo.stringValue,
                        Imagen: (string)f.imagen.stringValue,
                        Mensaje: ""
                    ));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR ObtenerUsuariosAsync(): {ex.Message}");
            }

            return lista;
        }

        public async Task<bool> ActualizarUsuario(string documentId, UsuarioModel usuario)
        {
            try
            {
                string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/{collection}/{documentId}";
                var data = new
                {
                    fields = new
                    {
                        id = new { integerValue = usuario.Id.ToString() },
                        apellidos = new { stringValue = usuario.Apellidos },
                        nombres = new { stringValue = usuario.Nombres },
                        cedula = new { stringValue = usuario.Cedula },
                        telefono = new { stringValue = usuario.Telefono },
                        correo = new { stringValue = usuario.Correo },
                        status = new { stringValue = usuario.Status },
                        tipoVehiculo = new { stringValue = usuario.TipoVehiculo },
                        placaVehicular = new { stringValue = usuario.PlacaVehicular },
                        usuario = new { stringValue = usuario.Usuario },
                        contrasena = new { stringValue = usuario.Contrasena },
                        activo = new { stringValue = usuario.Activo },
                        imagen = new { stringValue = usuario.Imagen }
                    }
                };
                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var response = await client.PatchAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EliminarUsuario(string documentId)
        {
            try
            {
                string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/{collection}/{documentId}";
                var response = await client.DeleteAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error EliminarUsuario(): {ex.Message}");
                return false;
            }
        }
    }
}
