using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ParkIsrael_Octavo.Services
{
    public class FirestoreService
    {
        private readonly string projectId = "parkisrael-aaa09";
        private readonly string collection = "usuarios";
        private readonly HttpClient client = new();

        public async Task<bool> GuardarUsuarioAsync(string nombre, string correo, string salario)
        {
            var document = new
            {
                fields = new
                {
                    nombre = new { stringValue = nombre },
                    correo = new { stringValue = correo },
                    salario = new { stringValue = salario }
                }
            };

            var json = JsonConvert.SerializeObject(document);
            var url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/{collection}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            return response.IsSuccessStatusCode;
        }
    }
}
