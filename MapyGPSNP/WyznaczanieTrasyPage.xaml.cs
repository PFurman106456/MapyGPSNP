namespace MapyGPSNP;

public partial class WyznaczanieTrasyPage : ContentPage
{
    private const string KeyStartLat = "startLat";
    private const string KeyStartLon = "startLon";
    private const string KeyMetaLat  = "metaLat";
    private const string KeyMetaLon  = "metaLon";

    private const string DomyslnyStartLat = "53.1244";
    private const string DomyslnyStartLon = "18.0320";
    private const string DomyslnyMetaLat  = "52.4030";
    private const string DomyslnyMetaLon  = "16.9082";

    private readonly Action<double, double, double, double> _onJedz;

    public WyznaczanieTrasyPage(Action<double, double, double, double> onJedz)
    {
        InitializeComponent();
        _onJedz = onJedz;

        entryStartLat.Text = Preferences.Get(KeyStartLat, DomyslnyStartLat);
        entryStartLon.Text = Preferences.Get(KeyStartLon, DomyslnyStartLon);
        entryMetaLat.Text  = Preferences.Get(KeyMetaLat,  DomyslnyMetaLat);
        entryMetaLon.Text  = Preferences.Get(KeyMetaLon,  DomyslnyMetaLon);
    }

    private void ZapiszKoordynaty()
    {
        Preferences.Set(KeyStartLat, entryStartLat.Text);
        Preferences.Set(KeyStartLon, entryStartLon.Text);
        Preferences.Set(KeyMetaLat,  entryMetaLat.Text);
        Preferences.Set(KeyMetaLon,  entryMetaLon.Text);
    }

    private bool SprobujParsuj(out double sLat, out double sLon, out double mLat, out double mLon)
    {
        var inv   = System.Globalization.CultureInfo.InvariantCulture;
        var style = System.Globalization.NumberStyles.Float;

        sLat = sLon = mLat = mLon = 0;

        return
            double.TryParse(entryStartLat.Text?.Replace(',', '.'), style, inv, out sLat) &
            double.TryParse(entryStartLon.Text?.Replace(',', '.'), style, inv, out sLon) &
            double.TryParse(entryMetaLat.Text?.Replace(',', '.'),  style, inv, out mLat) &
            double.TryParse(entryMetaLon.Text?.Replace(',', '.'),  style, inv, out mLon);
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

    private void btnZamien_Clicked(object sender, EventArgs e)
    {
        (entryStartLat.Text, entryMetaLat.Text) = (entryMetaLat.Text, entryStartLat.Text);
        (entryStartLon.Text, entryMetaLon.Text) = (entryMetaLon.Text, entryStartLon.Text);
    }

    private async void btnRozpocznij_Clicked(object sender, EventArgs e)
    {
        if (!SprobujParsuj(out double sLat, out double sLon, out double mLat, out double mLon))
        {
            lblBlad.Text = "Wprowadź prawidłowe koordynaty we wszystkich polach.";
            lblBlad.IsVisible = true;
            return;
        }

        ZapiszKoordynaty();

        lblBlad.IsVisible = false;
        _onJedz(sLat, sLon, mLat, mLon);
        await Navigation.PopAsync();
    }
}
