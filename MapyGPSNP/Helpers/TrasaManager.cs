using MapyGPSNP.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MapyGPSNP.Helpers
{
    public static class TrasaManager
    {

        public static async Task<DaneTrasy> PobierzTrase(double startLat, double startLon, double metaLat, double metaLon)
        {
            string start = $"{startLon.ToString(CultureInfo.InvariantCulture)},{startLat.ToString(CultureInfo.InvariantCulture)}";

            string meta = $"{metaLon.ToString(CultureInfo.InvariantCulture)},{metaLat.ToString(CultureInfo.InvariantCulture)}";


            string url = $"http://router.project-osrm.org/route/v1/driving/{start};{meta}?overview=full&geometries=geojson";

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "ProjektStudencki");

            var json = await client.GetStringAsync(url);

            var odp = JsonSerializer.Deserialize<OdpowiedzOSRM>(json);

            if(odp != null && odp.ListaTras.Count > 0)
            {
                return odp.ListaTras[0];
            }


            return null;
        }


        public static List<Punkt> PobierzMocka()
        {

            // pastebin:
            // pastebin.com/RwQzd477
            string fakeDane = @"{
            ""routes"": [
                {
                    ""geometry"": {
                        ""coordinates"": [
                            [18.0084, 53.1235],
                            [18.0100, 53.1242],
                            [18.0125, 53.1255],
                            [18.0150, 53.1268],
                            [18.0185, 53.1280]
                        ]
                    }
                }
            ]
        }";

            var odpowiedz = JsonSerializer.Deserialize<OdpowiedzOSRM>(fakeDane);

            var listaPunktow = new List<Punkt>();

            // mapowanie danych z tablicy dwuelementowej na pomocniczą klasę Punkt
            if(odpowiedz != null && odpowiedz.ListaTras.Count > 0)
            {
                var geometria = odpowiedz.ListaTras[0].Geometria;

                foreach (var koordynaty in geometria.PunktyWspolrzednych)
                {
                    listaPunktow.Add(new Punkt(koordynaty[0], koordynaty[1]));
                }
            }

            return listaPunktow;
        }
    }
}
