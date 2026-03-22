using MapyGPSNP.Model;
using NetTopologySuite.Shape;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MapyGPSNP.Model
{
    public class OdpowiedzOSRM
    {
        [JsonPropertyName("routes")]
        public List<DaneTrasy> ListaTras { get; set; }
    }

    public class DaneTrasy
    {
        [JsonPropertyName("distance")]
        public double Dystans { get; set; }

        [JsonPropertyName("duration")]
        public double CzasSekundy { get; set; }

        [JsonPropertyName("geometry")]
        public GeometriaTrasy Geometria { get; set; }
    }

    public class GeometriaTrasy
    {
        [JsonPropertyName("coordinates")]
        public List<double[]> PunktyWspolrzednych { get; set; }
    }
}
