using Newtonsoft.Json;
using System.Text;

namespace ParkIsrael_Octavo.Services
{
    public static class ServicioParqueaderoFirebase
    {
        private static readonly string firebaseUrl =
            "https://parkisrael-aaa09-default-rtdb.firebaseio.com";

        // ===================
        // 🔹 OBTENER CUPOS
        // ===================
        public static async Task<CuposModel?> ObtenerCuposAsync()
        {
            using var client = new HttpClient();
            var response = await client.GetAsync($"{firebaseUrl}/cupos.json");

            if (!response.IsSuccessStatusCode) return null;

            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CuposModel>(json);
        }

        // ===================
        // 🔹 ACTUALIZAR CUPOS
        // ===================
        private static async Task<bool> GuardarCuposAsync(CuposModel cupos)
        {
            using var client = new HttpClient();

            string json = JsonConvert.SerializeObject(cupos);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{firebaseUrl}/cupos.json", content);
            return response.IsSuccessStatusCode;
        }

        // ===================
        // 🔹 ENTRADA
        // ===================
        public static async Task<(bool ok, string msg)> RegistrarEntrada(string sede)
        {
            var cupos = await ObtenerCuposAsync();
            if (cupos == null) return (false, "Error leyendo Firebase.");

            int actual = cupos.GetCupo(sede);
            if (actual <= 0)
                return (false, "Parqueadero lleno.");

            cupos.SetCupo(sede, actual - 1);

            bool saved = await GuardarCuposAsync(cupos);
            return (saved, saved ? "OK" : "Error guardando en Firebase");
        }

        // ===================
        // 🔹 SALIDA
        // ===================
        public static async Task<(bool ok, string msg)> RegistrarSalida(string sede)
        {
            var cupos = await ObtenerCuposAsync();
            if (cupos == null) return (false, "Error leyendo Firebase.");

            int actual = cupos.GetCupo(sede);
            int maximo = cupos.GetMax(sede);

            if (actual >= maximo)
                return (false, "Parqueadero vacío.");

            cupos.SetCupo(sede, actual + 1);

            bool saved = await GuardarCuposAsync(cupos);
            return (saved, saved ? "OK" : "Error guardando en Firebase");
        }

        // ===================
        // 🔹 REINICIAR
        // ===================
        public static async Task<bool> ReiniciarAsync()
        {
            var cupos = new CuposModel
            {
                matriz = new CupoItem { cupos = 20, max = 20 },
                idiomas = new CupoItem { cupos = 20, max = 20 },
                posgrados = new CupoItem { cupos = 20, max = 20 },
            };

            return await GuardarCuposAsync(cupos);
        }
    }

    // ============================================================================
    // MODELOS
    // ============================================================================
    public class CuposModel
    {
        public CupoItem matriz { get; set; }
        public CupoItem idiomas { get; set; }
        public CupoItem posgrados { get; set; }

        public int GetCupo(string sede) => sede switch
        {
            "MATRIZ" => matriz.cupos,
            "IDIOMAS" => idiomas.cupos,
            "POSGRADOS" => posgrados.cupos,
            _ => 0
        };

        public int GetMax(string sede) => sede switch
        {
            "MATRIZ" => matriz.max,
            "IDIOMAS" => idiomas.max,
            "POSGRADOS" => posgrados.max,
            _ => 0
        };

        public void SetCupo(string sede, int valor)
        {
            switch (sede)
            {
                case "MATRIZ": matriz.cupos = valor; break;
                case "IDIOMAS": idiomas.cupos = valor; break;
                case "POSGRADOS": posgrados.cupos = valor; break;
            }
        }
    }

    public class CupoItem
    {
        public int cupos { get; set; }
        public int max { get; set; }
    }
}
