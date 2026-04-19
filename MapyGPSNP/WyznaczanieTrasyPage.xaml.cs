namespace MapyGPSNP;

public partial class WyznaczanieTrasyPage : ContentPage
{
    private static string _lastStartLat = "53.1230";
    private static string _lastStartLon = "18.0192";
    private static string _lastMetaLat  = "53.1289";
    private static string _lastMetaLon  = "18.0125";

    private readonly Action<double, double, double, double> _onJedz;

    public WyznaczanieTrasyPage(Action<double, double, double, double> onJedz)
    {
        InitializeComponent();
        _onJedz = onJedz;

        entryStartLat.Text = _lastStartLat;
        entryStartLon.Text = _lastStartLon;
        entryMetaLat.Text  = _lastMetaLat;
        entryMetaLon.Text  = _lastMetaLon;
    }

    private async void btnLokalizacja_Clicked(object sender, EventArgs e)
    {
        btnLokalizacja.IsEnabled = false;
        btnLokalizacja.Text = "Pobieranie...";
        lblBlad.IsVisible = false;

        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                lblBlad.Text = "Brak uprawnień do lokalizacji.";
                lblBlad.IsVisible = true;
                return;
            }

            var lokalizacja = await Geolocation.Default.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(10)
            });

            if (lokalizacja != null)
            {
                var inv = System.Globalization.CultureInfo.InvariantCulture;
                entryStartLat.Text = lokalizacja.Latitude.ToString("F6", inv);
                entryStartLon.Text = lokalizacja.Longitude.ToString("F6", inv);
            }
            else
            {
                lblBlad.Text = "Nie udało się pobrać lokalizacji.";
                lblBlad.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            lblBlad.Text = $"Błąd lokalizacji: {ex.Message}";
            lblBlad.IsVisible = true;
        }
        finally
        {
            btnLokalizacja.IsEnabled = true;
            btnLokalizacja.Text = "Użyj mojej lokalizacji";
        }
    }

    private async void btnRozpocznij_Clicked(object sender, EventArgs e)
    {
        var inv   = System.Globalization.CultureInfo.InvariantCulture;
        var style = System.Globalization.NumberStyles.Float;

        double sLat = 0, sLon = 0, mLat = 0, mLon = 0;

        bool ok =
            double.TryParse(entryStartLat.Text?.Replace(',', '.'), style, inv, out sLat) &
            double.TryParse(entryStartLon.Text?.Replace(',', '.'), style, inv, out sLon) &
            double.TryParse(entryMetaLat.Text?.Replace(',', '.'),  style, inv, out mLat) &
            double.TryParse(entryMetaLon.Text?.Replace(',', '.'),  style, inv, out mLon);

        if (!ok)
        {
            lblBlad.Text = "Wprowadź prawidłowe koordynaty we wszystkich polach.";
            lblBlad.IsVisible = true;
            return;
        }

        _lastStartLat = entryStartLat.Text;
        _lastStartLon = entryStartLon.Text;
        _lastMetaLat  = entryMetaLat.Text;
        _lastMetaLon  = entryMetaLon.Text;

        lblBlad.IsVisible = false;
        _onJedz(sLat, sLon, mLat, mLon);
        await Navigation.PopAsync();
    }
}
