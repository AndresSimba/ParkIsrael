using Newtonsoft.Json;
using System.Text;

namespace ParkIsrael_Octavo.Services
{
    public static class FirebaseInitService
    {
        private static readonly HttpClient client = new();
        private const string baseUrl = "https://parkisrael-aaa09-default-rtdb.firebaseio.com/";

        public static async Task InicializarBaseAsync()
        {
            try
            {
                // Verificar si ya existe "cupos"
                string urlCupos = $"{baseUrl}cupos.json";
                var response = await client.GetAsync(urlCupos);
                string json = await response.Content.ReadAsStringAsync();

                // Si cupos ya existe → NO hacemos nada
                if (!string.IsNullOrEmpty(json) && json != "null")
                {
                    return;
                }

                // Crear estructura inicial
                var datos = new
                {
                    cupos = new
                    {
                        matriz = 20,
                        idiomas = 20,
                        posgrados = 20
                    },
                    config = new
                    {
                        matriz = 20,
                        idiomas = 20,
                        posgrados = 20
                    }
                };

                string urlRoot = $"{baseUrl}.json";
                string jsonBody = JsonConvert.SerializeObject(datos);

                await client.PatchAsync(urlRoot,
                    new StringContent(jsonBody, Encoding.UTF8, "application/json"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR creando base: " + ex.Message);
            }
        }
    }
}