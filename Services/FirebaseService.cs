using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ParkIsrael_Octavo.Models;


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
                        from = new[]
                        {
                            new { collectionId = "usuarios" }
                        },
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

                // revisar si existe
                if (data == null || data[0].document == null)
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

        public async Task<UsuarioModel?> ObtenerUsuarioPorNombreAsync(string usuario)
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
                    return null;
                dynamic data = JsonConvert.DeserializeObject(responseText);
                if (data == null || data[0].document == null)
                    return null;
                var doc = data[0].document.fields;
                return new UsuarioModel(
                    Id: int.Parse((string)doc.id.integerValue),
                    Apellidos: (string)doc.apellidos.stringValue,
                    Nombres: (string)doc.nombres.stringValue,
                    Cedula: (string)doc.cedula.stringValue,
                    Telefono: (string)doc.telefono.stringValue,
                    Correo: (string)doc.correo.stringValue,
                    Status: (string)doc.status.stringValue,
                    TipoVehiculo: (string)doc.tipoVehiculo.stringValue,
                    PlacaVehicular: (string)doc.placaVehicular.stringValue,
                    Usuario: (string)doc.usuario.stringValue,
                    Contrasena: (string)doc.contrasena.stringValue,
                    Activo: (string)doc.activo.stringValue,
                    Imagen: (string)doc.imagen.stringValue,
                    Mensaje: ""
                );
            }
            catch
            {
                return null;
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
    }
}
