using Mapsui.Projections;

namespace MapyGPSNP
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();

            mojaMapa.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

            var wspolrzedne = SphericalMercator.FromLonLat(18.0084, 53.1235); // r.Jagiellonów

        }

        private void btnJedz_Clicked(object sender, EventArgs e)
        {

        }
    }

}
