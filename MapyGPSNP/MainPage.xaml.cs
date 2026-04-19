
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

            var start = SphericalMercator.FromLonLat(18.0320, 53.1244);
            mojaMapa.Map?.Navigator.CenterOn(new MPoint(start.x, start.y));
            mojaMapa.Map?.Navigator.ZoomTo(10);

            // Wczytaj zapisane koordynaty przy starcie
            WczytajKoordynatyZPreferences();
        }

        private void WczytajKoordynatyZPreferences()
        {
            var inv   = System.Globalization.CultureInfo.InvariantCulture;
            var style = System.Globalization.NumberStyles.Float;

            if (double.TryParse(Preferences.Get("startLat", ""), style, inv, out double sLat) &&
                double.TryParse(Preferences.Get("startLon", ""), style, inv, out double sLon) &&
                double.TryParse(Preferences.Get("metaLat",  ""), style, inv, out double mLat) &&
                double.TryParse(Preferences.Get("metaLon",  ""), style, inv, out double mLon))
            {
                _startLat = sLat;
                _startLon = sLon;
                _metaLat  = mLat;
                _metaLon  = mLon;
            }
        }

        private async Task btnJedz_Clicked(object sender, EventArgs e) { }

        // ── Czyszczenie warstw ─────────────────────────────────────────────

        private void WyczyscWarstwe(string nazwa)
        {
            var warstwa = mojaMapa.Map?.Layers.FirstOrDefault(l => l.Name == nazwa);
            if (warstwa != null)
                mojaMapa.Map?.Layers.Remove(warstwa);
        }

        // ── Rysowanie trasy z markerami ────────────────────────────────────

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
            markerStart.Styles.Add(new SymbolStyle
            {
                Fill = new Mapsui.Styles.Brush(Color.Green),
                Outline = new Pen(Color.White, 2),
                SymbolScale = 0.6
            });

            var markerKoniec = new PointFeature(projekcje.Last());
            markerKoniec.Styles.Add(new SymbolStyle
            {
                Fill = new Mapsui.Styles.Brush(Color.Red),
                Outline = new Pen(Color.White, 2),
                SymbolScale = 0.6
            });

            mojaMapa.Map?.Layers.Add(new MemoryLayer { Features = [markerStart, markerKoniec], Name = "warstwaMarkerow" });

            mojaMapa.Refresh();
            lblOpisTrasy.Text = "Trasa została narysowana!";

            return projekcje;
        }

        // ── Zoom z bounding boxa (Spherical Mercator, metry) ──────────────
        // Progi dobrane tak by cała trasa była widoczna z marginesem

        // ZoomTo() przyjmuje rozdzielczość w m/px — wyższy = bardziej oddalony
        // Zakładamy ~400px szerokości viewportu, padding 50%
        private static (double resolution, double bboxMax) ZoomZBoundingBoxa(double minX, double maxX, double minY, double maxY)
        {
            var bboxMax = Math.Max(maxX - minX, maxY - minY);
            var resolution = (bboxMax * 0.75) / 400.0;
            return (resolution, bboxMax);
        }

        // ── Nawigacja ──────────────────────────────────────────────────────

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

        private void btnZoomIn_Clicked(object sender, EventArgs e)
        {
            var current = mojaMapa.Map?.Navigator.Viewport.Resolution ?? 100;
            mojaMapa.Map?.Navigator.ZoomTo(current / 2.0);
        }

        private void btnZoomOut_Clicked(object sender, EventArgs e)
        {
            var current = mojaMapa.Map?.Navigator.Viewport.Resolution ?? 100;
            mojaMapa.Map?.Navigator.ZoomTo(current * 2.0);
        }

        private void btnCentrujStart_Clicked(object sender, EventArgs e)
        {
            if (_startLat == null || _startLon == null)
            {
                lblOpisTrasy.Text = "Brak punktu startowego.";
                return;
            }
            var s = SphericalMercator.FromLonLat(_startLon.Value, _startLat.Value);
            mojaMapa.Map?.Navigator.CenterOn(new MPoint(s.x, s.y));
            mojaMapa.Map?.Navigator.ZoomTo(3);
        }

        // ── Wyznaczanie trasy ──────────────────────────────────────────────

        private async Task WyznaczIRysujTrase()
        {
            overlayLadowania.IsVisible = true;
            spinner.IsRunning = true;
            btnJedz.IsEnabled = false;

            try
            {
                await WyznaczIRysujTraseWewnetrzna();
            }
            finally
            {
                overlayLadowania.IsVisible = false;
                spinner.IsRunning = false;
                btnJedz.IsEnabled = true;
            }
        }

        private async Task WyznaczIRysujTraseWewnetrzna()
        {
            // Zadanie 7 — obsługa błędów sieci i uprawnień
            try
            {
                var trasa = await TrasaManager.PobierzTrase(
                    _startLat!.Value, _startLon!.Value,
                    _metaLat!.Value, _metaLon!.Value);

                if (trasa == null)
                {
                    lblOpisTrasy.Text = "Nie znaleziono trasy dla podanych punktów.";
                    return;
                }

                var punktyTrasy = trasa.Geometria.PunktyWspolrzednych
                    .Select(p => new Punkt(p[0], p[1])).ToList();

                var projekcje = RysujTrase(punktyTrasy);

                double dystansKm = trasa.Dystans / 1000;
                int czasMinuty = (int)Math.Round(trasa.CzasSekundy / 60);
                var godzinaPrzyjazdu = DateTime.Now.AddSeconds(trasa.CzasSekundy);

                // Zadanie 4 — auto-fit: środek + zoom z bounding boxa
                var minX = projekcje.Min(p => p.X);
                var maxX = projekcje.Max(p => p.X);
                var minY = projekcje.Min(p => p.Y);
                var maxY = projekcje.Max(p => p.Y);
                var srodek = new MPoint((minX + maxX) / 2, (minY + maxY) / 2);
                var (resolution, bboxM) = ZoomZBoundingBoxa(minX, maxX, minY, maxY);

                mojaMapa.Map?.Navigator.CenterOn(srodek);
                mojaMapa.Map?.Navigator.ZoomTo(resolution);

                lblOpisTrasy.Text += $"\tDystans: {dystansKm:F2} km, Czas: {czasMinuty} min, Przyjazd: {godzinaPrzyjazdu:HH:mm}";
            }
            catch (HttpRequestException)
            {
                lblOpisTrasy.Text = "Brak połączenia z internetem. Sprawdź sieć i spróbuj ponownie.";
            }
            catch (TaskCanceledException)
            {
                lblOpisTrasy.Text = "Przekroczono czas oczekiwania. Sprawdź połączenie internetowe.";
            }
            catch (Exception ex)
            {
                lblOpisTrasy.Text = $"Nie udało się wyznaczyć trasy: {ex.Message}";
            }
        }
    }
}
