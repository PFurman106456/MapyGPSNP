using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapyGPSNP.Model
{
    public class Punkt
    {
        public double Dlugosc { get; set; }

        public double Szerokosc { get; set; }


        public Punkt(double dlugosc, double szerokosc)
        {
            Dlugosc = dlugosc;
            Szerokosc = szerokosc;
        }
    }
}
