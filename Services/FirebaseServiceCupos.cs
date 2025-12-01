using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ParkIsrael_Octavo.Services
{
    public static class FirebaseServiceCupos
    {
        // URL base de tu Realtime Database (SIN .json al final)
        private const string firebaseUrl = "https://parkisrael-aaa09-default-rtdb.firebaseio.com";
        private static readonly HttpClient http = new HttpClient();

        // Modelo para una sede en Firebase: { "cupos": 20, "max": 20 }
        public class CupoItem
        {
            public int cupos { get; set; }
            public int max { get; set; }
        }

        /// <summary>
        /// Obtiene (cupos, max) de una sede: "matriz", "idiomas" o "posgrados"
        /// </summary>
        public static async Task<(int cupos, int max)> ObtenerCuposAsync(string sede)
        {
            var url = $"{firebaseUrl}/cupos/{sede}.json";
            var resp = await http.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
                return (-1, -1);
            var json = await resp.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<CupoItem>(json);
            if (item == null)
                return (-1, -1);
            return (item.cupos, item.max);
        }

        /// <summary>
        /// ENTRAR: reduce en 1 los cupos de la sede (sin bajar de 0).
        /// Devuelve false si ya está lleno (0).
        /// </summary>
        public static async Task<bool> ReducirAsync(string sede)
        {
            var (cupos, max) = await ObtenerCuposAsync(sede);
            if (cupos <= 0 || max <= 0)
                return false; // ya está lleno o datos inválidos
            cupos--;
            var url = $"{firebaseUrl}/cupos/{sede}/cupos.json";
            var json = JsonConvert.SerializeObject(cupos);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await http.PutAsync(url, content);
            return resp.IsSuccessStatusCode;
        }

        /// <summary>
        /// SALIR: aumenta en 1 los cupos de la sede (sin pasar del máximo).
        /// Devuelve false si ya está en el máximo.
        /// </summary>
        public static async Task<bool> AumentarAsync(string sede)
        {
            var (cupos, max) = await ObtenerCuposAsync(sede);
            if (cupos < 0 || max <= 0)
                return false;
            if (cupos >= max)
                return false; // ya está al máximo
            cupos++;
            var url = $"{firebaseUrl}/cupos/{sede}/cupos.json";
            var json = JsonConvert.SerializeObject(cupos);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await http.PutAsync(url, content);
            return resp.IsSuccessStatusCode;
        }

        /// <summary>
        /// Guarda los cupos máximos para cada sede (matriz, idiomas, posgrados).
        /// NO toca los cupos actuales, solo el "max".
        /// </summary>
        public static async Task<bool> GuardarMaximosAsync(int maxMatriz, int maxIdiomas, int maxPosgrados)
        {
            bool ok1 = await PutIntAsync("cupos/matriz/max", maxMatriz);
            bool ok2 = await PutIntAsync("cupos/idiomas/max", maxIdiomas);
            bool ok3 = await PutIntAsync("cupos/posgrados/max", maxPosgrados);
            return ok1 && ok2 && ok3;
        }

        /// <summary>
        /// Reinicia los cupos actuales = max en las tres sedes.
        /// </summary>
        public static async Task<bool> ReiniciarCuposAsync()
        {
            var (cM, maxM) = await ObtenerCuposAsync("matriz");
            var (cI, maxI) = await ObtenerCuposAsync("idiomas");
            var (cP, maxP) = await ObtenerCuposAsync("posgrados");
            bool ok1 = await PutIntAsync("cupos/matriz/cupos", maxM);
            bool ok2 = await PutIntAsync("cupos/idiomas/cupos", maxI);
            bool ok3 = await PutIntAsync("cupos/posgrados/cupos", maxP);
            return ok1 && ok2 && ok3;
        }

        // --- Helpers privados ---------------------------

        private static async Task<bool> PutIntAsync(string path, int value)
        {
            var url = $"{firebaseUrl}/{path}.json";
            var json = JsonConvert.SerializeObject(value);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await http.PutAsync(url, content);
            return resp.IsSuccessStatusCode;
        }
    }
}
