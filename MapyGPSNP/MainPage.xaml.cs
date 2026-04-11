
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using MapyGPSNP.Helpers;
using MapyGPSNP.Model;
using NetTopologySuite.Geometries;
using Color = Mapsui.Styles.Color;

namespace MapyGPSNP
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();

            mojaMapa.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

            var wspolrzedne = SphericalMercator.FromLonLat(18.0084, 53.1235); // r.Jagiellonów
            var srodekBdg = new MPoint(wspolrzedne.x, wspolrzedne.y);

            mojaMapa.Map?.Navigator.CenterOn(srodekBdg);
            mojaMapa.Map?.Navigator.ZoomTo(2);


        }

        private async Task btnJedz_Clicked(object sender, EventArgs e)
        {
            
        }

        private void RysujTrase(List<Punkt> punktyTrasy)
        {
            var listaKoordynatow = new List<Coordinate>();

            foreach (var punkt in punktyTrasy)
            {
                var wynikKonwersji = SphericalMercator.FromLonLat(punkt.Dlugosc, punkt.Szerokosc);

                listaKoordynatow.Add(new Coordinate(wynikKonwersji.x, wynikKonwersji.y));
            }

            var sciezkaKsztalt = new LineString(listaKoordynatow.ToArray());

            var sciezkaNaMapie = new GeometryFeature(sciezkaKsztalt);

            sciezkaNaMapie.Styles.Add(new VectorStyle
            {
                Line = new Pen(Color.Blue, 7)
            });

            var warstwaTrasy = new MemoryLayer()
            {
                Features = [sciezkaNaMapie],
                Name = "warstwaTrasy"
            };

            mojaMapa.Map?.Layers.Add(warstwaTrasy);
            mojaMapa.Refresh();


            lblOpisTrasy.Text = "Trasa została narysowana!";
        }

        private async void btnSzukaj_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new WyznaczanieTrasyPage());
        }

        private async void btnJedz_Clicked_1(object sender, EventArgs e)
        {
            //var punktyTrasy = TrasaManager.PobierzMocka();
            double startLat = 53.1235;
            double startLon = 18.0084;

            double metaLat = 54.609445;
            double metaLon = 18.801177;

            // wyśrodkowanie i zbliżenie mapy na obszarze końca trasy
            var punktMeta = SphericalMercator.FromLonLat(metaLon, metaLat);
            var cel = new MPoint(punktMeta.x, punktMeta.y);
            mojaMapa.Map?.Navigator.CenterOn(cel);
            mojaMapa.Map?.Navigator.ZoomTo(10);


            var trasa = await TrasaManager.PobierzTrase(startLat, startLon, metaLat, metaLon);

            if (trasa != null)
            {
                var punktyTrasy = trasa.Geometria.PunktyWspolrzednych.Select(p => new Punkt(p[0], p[1])).ToList();

                RysujTrase(punktyTrasy);

                double dystansKm = trasa.Dystans / 1000;
                int czasMinuty = (int)Math.Round(trasa.CzasSekundy / 60);

                lblOpisTrasy.Text += $"\tDystans: {dystansKm:F2} km, Czas: {czasMinuty} min";
            }

        }
    }

}
