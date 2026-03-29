
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using MapyGPSNP.Helpers;
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

        private void btnJedz_Clicked(object sender, EventArgs e)
        {
            var punktyTrasy = TrasaManager.PobierzMocka();

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
    }

}
