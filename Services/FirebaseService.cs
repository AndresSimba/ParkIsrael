using Newtonsoft.Json;
using ParkIsrael_Octavo.Models;
using System.Text;


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
                return -1;
            }
        }

        public async Task<bool> GuardarUsuarioAsync(string apellidos, string nombres, string cedula, string telefono, string correo, string status, string tipoVehiculo,
                                                    string placaVehicular, string usuario, string contrasena, string imagenBase64)
        {
            try
            {
                int nuevoId = await ObtenerNuevoIdAsync();
                if (nuevoId == -1)
                    return false;

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
                        status = new { stringValue = status },
                        tipoVehiculo = new { stringValue = tipoVehiculo },
                        placaVehicular = new { stringValue = placaVehicular },
                        usuario = new { stringValue = usuario },
                        contrasena = new { stringValue = contrasena },
                        activo = new { stringValue = "Si" },
                        imagen = new { stringValue = imagenBase64 }
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
                if (data == null || data.Count == 0 || data[0].document == null)
                    return (null, "");

                var doc = data[0].document;
                var f = doc.fields;

                string Safe(dynamic obj, string def = "")
                {
                    return obj != null ? (string)obj.stringValue : def;
                }

                string SafeInt(dynamic obj)
                {
                    return obj != null ? (string)obj.integerValue : "0";
                }

                string docId = ((string)doc.name).Split('/').Last();

                var usuarioModel = new UsuarioModel(
                    Id: int.Parse(SafeInt(f.id)),
                    Apellidos: Safe(f.apellidos),
                    Nombres: Safe(f.nombres),
                    Cedula: Safe(f.cedula),
                    Telefono: Safe(f.telefono),
                    Correo: Safe(f.correo),
                    Status: Safe(f.status),
                    TipoVehiculo: Safe(f.tipoVehiculo),
                    PlacaVehicular: Safe(f.placaVehicular),
                    Usuario: Safe(f.usuario),
                    Contrasena: Safe(f.contrasena),
                    Activo: Safe(f.activo),
                    Imagen: Safe(f.imagen),
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
                string? pageToken = null;

                do
                {
                    string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/{collection}?pageSize=100";
                    if (!string.IsNullOrEmpty(pageToken))
                        url += $"&pageToken={pageToken}";
                    var response = await client.GetAsync(url);
                    var json = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                        return lista;
                    dynamic data = JsonConvert.DeserializeObject(json);
                    if (data == null || data.documents == null)
                        return lista;
                    string Safe(dynamic obj, string def = "")
                    {
                        try
                        {
                            return obj != null ? (string)obj.stringValue : def;
                        }
                        catch
                        {
                            return def;
                        }
                    }

                    int SafeInt(dynamic obj)
                    {
                        try
                        {
                            return obj != null ? int.Parse((string)obj.integerValue) : 0;
                        }
                        catch
                        {
                            return 0;
                        }
                    }

                    foreach (var item in data.documents)
                    {
                        try
                        {
                            if (item.fields == null)
                                continue;
                            var f = item.fields;
                            lista.Add(new UsuarioModel(
                                Id: SafeInt(f.id),
                                Apellidos: Safe(f.apellidos),
                                Nombres: Safe(f.nombres),
                                Cedula: Safe(f.cedula),
                                Telefono: Safe(f.telefono),
                                Correo: Safe(f.correo),
                                Status: Safe(f.status),
                                TipoVehiculo: Safe(f.tipoVehiculo),
                                PlacaVehicular: Safe(f.placaVehicular),
                                Usuario: Safe(f.usuario),
                                Contrasena: Safe(f.contrasena),
                                Activo: Safe(f.activo),
                                Imagen: Safe(f.imagen),
                                Mensaje: ""
                            ));
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Documento omitido: {ex.Message}");
                        }
                    }

                    try
                    {
                        pageToken = data.nextPageToken != null ? (string)data.nextPageToken : null;
                    }
                    catch
                    {
                        pageToken = null;
                    }

                } while (!string.IsNullOrEmpty(pageToken));
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

                var content = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PatchAsync(url, content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ActualizarUsuario(): {ex.Message}");
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

        public async Task<UsuarioModel?> ObtenerUsuarioPorDocumentIdAsync(string documentId)
        {
            try
            {
                string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/{collection}/{documentId}";

                var response = await client.GetAsync(url);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return null;

                dynamic doc = JsonConvert.DeserializeObject(responseText);
                var f = doc.fields;

                string Safe(dynamic obj, string def = "")
                {
                    return obj != null ? (string)obj.stringValue : def;
                }

                string SafeInt(dynamic obj)
                {
                    return obj != null ? (string)obj.integerValue : "0";
                }

                var usuarioModel = new UsuarioModel(
                    Id: int.Parse(SafeInt(f.id)),
                    Apellidos: Safe(f.apellidos),
                    Nombres: Safe(f.nombres),
                    Cedula: Safe(f.cedula),
                    Telefono: Safe(f.telefono),
                    Correo: Safe(f.correo),
                    Status: Safe(f.status),
                    TipoVehiculo: Safe(f.tipoVehiculo),
                    PlacaVehicular: Safe(f.placaVehicular),
                    Usuario: Safe(f.usuario),
                    Contrasena: Safe(f.contrasena),
                    Activo: Safe(f.activo),
                    Imagen: Safe(f.imagen),
                    Mensaje: ""
                );

                return usuarioModel;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ObtenerUsuarioPorDocumentIdAsync(): {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ExisteCedulaAsync(string cedula)
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
                                field = new { fieldPath = "cedula" },
                                op = "EQUAL",
                                value = new { stringValue = cedula }
                            }
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(query);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var responseText = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return false;
                dynamic data = JsonConvert.DeserializeObject(responseText);
                return data != null && data.Count > 0 && data[0].document != null;
            }
            catch
            {
                return false;
            }
        }
    }
}