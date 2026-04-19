
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
        private double? _startLat;
        private double? _startLon;
        private double? _metaLat;
        private double? _metaLon;

        public MainPage()
        {
            InitializeComponent();

            mojaMapa.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

            var start = SphericalMercator.FromLonLat(18.0192, 53.1230);
            mojaMapa.Map?.Navigator.CenterOn(new MPoint(start.x, start.y));
            mojaMapa.Map?.Navigator.ZoomTo(10);


        }

        private async Task btnJedz_Clicked(object sender, EventArgs e)
        {
            
        }

        private void WyczyscWarstwe(string nazwa)
        {
            var warstwa = mojaMapa.Map?.Layers.FirstOrDefault(l => l.Name == nazwa);
            if (warstwa != null)
                mojaMapa.Map?.Layers.Remove(warstwa);
        }

        private List<MPoint> RysujTrase(List<Punkt> punktyTrasy)
        {
            WyczyscWarstwe("warstwaTrasy");
            WyczyscWarstwe("warstwaMarkerow");

            var projekcje = punktyTrasy
                .Select(p => { var k = SphericalMercator.FromLonLat(p.Dlugosc, p.Szerokosc); return new MPoint(k.x, k.y); })
                .ToList();

            var linia = new GeometryFeature(new LineString(projekcje.Select(p => new Coordinate(p.X, p.Y)).ToArray()));
            linia.Styles.Add(new VectorStyle { Line = new Pen(Color.Blue, 7) });
            mojaMapa.Map?.Layers.Add(new MemoryLayer { Features = [linia], Name = "warstwaTrasy" });

            var markerStart = new PointFeature(projekcje.First());
            markerStart.Styles.Add(new SymbolStyle { Fill = new Mapsui.Styles.Brush(Color.Green), Outline = new Pen(Color.White, 2), SymbolScale = 0.6 });

            var markerKoniec = new PointFeature(projekcje.Last());
            markerKoniec.Styles.Add(new SymbolStyle { Fill = new Mapsui.Styles.Brush(Color.Red), Outline = new Pen(Color.White, 2), SymbolScale = 0.6 });

            mojaMapa.Map?.Layers.Add(new MemoryLayer { Features = [markerStart, markerKoniec], Name = "warstwaMarkerow" });

            mojaMapa.Refresh();
            lblOpisTrasy.Text = "Trasa została narysowana!";

            return projekcje;
        }

        private async void btnSzukaj_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new WyznaczanieTrasyPage((sLat, sLon, mLat, mLon) =>
            {
                _startLat = sLat;
                _startLon = sLon;
                _metaLat = mLat;
                _metaLon = mLon;
                _ = WyznaczIRysujTrase();
            }));
        }

        private async void btnJedz_Clicked_1(object sender, EventArgs e)
        {
            if (_startLat == null || _startLon == null || _metaLat == null || _metaLon == null)
            {
                lblOpisTrasy.Text = "Najpierw wyszukaj trasę przyciskiem Szukaj.";
                return;
            }
            await WyznaczIRysujTrase();
        }

        private static int ZoomDladystansu(double dystansKm) => dystansKm switch
        {
            < 1    => 16,
            < 5    => 14,
            < 20   => 13,
            < 50   => 12,
            < 150  => 11,
            < 400  => 10,
            _      => 8
        };

        private async Task WyznaczIRysujTrase()
        {
            var trasa = await TrasaManager.PobierzTrase(_startLat!.Value, _startLon!.Value, _metaLat!.Value, _metaLon!.Value);

            if (trasa != null)
            {
                var punktyTrasy = trasa.Geometria.PunktyWspolrzednych.Select(p => new Punkt(p[0], p[1])).ToList();

                var projekcje = RysujTrase(punktyTrasy);

                double dystansKm = trasa.Dystans / 1000;
                int czasMinuty = (int)Math.Round(trasa.CzasSekundy / 60);

                var koniec = projekcje.Last();
                mojaMapa.Map?.Navigator.CenterOn(koniec);
                mojaMapa.Map?.Navigator.ZoomTo(ZoomDladystansu(dystansKm));

                lblOpisTrasy.Text += $"\tDystans: {dystansKm:F2} km, Czas: {czasMinuty} min";
            }
        }
    }

}
