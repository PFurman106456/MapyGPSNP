namespace MapyGPSNP;

public partial class WyznaczanieTrasyPage : ContentPage
{
    private readonly Action<double, double, double, double> _onTrasaWybrana;

    public WyznaczanieTrasyPage(Action<double, double, double, double> onTrasaWybrana)
    {
        InitializeComponent();
        _onTrasaWybrana = onTrasaWybrana;
    }

    private async void btnWyznacz_Clicked(object sender, EventArgs e)
    {
        if (!double.TryParse(entryStartLat.Text?.Replace(',', '.'), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double startLat) ||
            !double.TryParse(entryStartLon.Text?.Replace(',', '.'), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double startLon) ||
            !double.TryParse(entryMetaLat.Text?.Replace(',', '.'), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double metaLat) ||
            !double.TryParse(entryMetaLon.Text?.Replace(',', '.'), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double metaLon))
        {
            lblBlad.Text = "Wprowadź prawidłowe koordynaty we wszystkich polach.";
            lblBlad.IsVisible = true;
            return;
        }

        lblBlad.IsVisible = false;
        _onTrasaWybrana(startLat, startLon, metaLat, metaLon);
        await Navigation.PopAsync();
    }
}
