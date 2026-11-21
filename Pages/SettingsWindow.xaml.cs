using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace benzconfig
{

public partial class Settings : Window
{
    public double SummerPropCity { get; private set; }
    public double SummerPropHighway { get; private set; }
    public double SummerRatesCity { get; private set; }
    public double SummerRatesHighway { get; private set; }
    public double WinterPropCity { get; private set; }
    public double WinterPropHighway { get; private set; }
    public double WinterRatesCity { get; private set; }
    public double WinterRatesHighway { get; private set; }

    private bool _lockSync;

    public Settings(
        double summerCity, double summerHighway, double summerRateCity, double summerRateHighway,
        double winterCity, double winterHighway, double winterRateCity, double winterRateHighway)
    {
        InitializeComponent();

        PropCitySummer.Text = (summerCity * 100).ToString("F0");
        PropHighwaySummer.Text = (summerHighway * 100).ToString("F0");
        RatesCitySummer.Text = summerRateCity.ToString(CultureInfo.InvariantCulture);
        RatesHighwaySummer.Text = summerRateHighway.ToString(CultureInfo.InvariantCulture);

        PropCityWinter.Text = (winterCity * 100).ToString("F0");
        PropHighwayWinter.Text = (winterHighway * 100).ToString("F0");
        RatesCityWinter.Text = winterRateCity.ToString(CultureInfo.InvariantCulture);
        RatesHighwayWinter.Text = winterRateHighway.ToString(CultureInfo.InvariantCulture);

        HookSync();
    }

    private void HookSync()
    {
        PropCitySummer.TextChanged += (s, e) => Sync(PropCitySummer, PropHighwaySummer);
        PropHighwaySummer.TextChanged += (s, e) => Sync(PropHighwaySummer, PropCitySummer);

        PropCityWinter.TextChanged += (s, e) => Sync(PropCityWinter, PropHighwayWinter);
        PropHighwayWinter.TextChanged += (s, e) => Sync(PropHighwayWinter, PropCityWinter);
    }

    private void Sync(TextBox source, TextBox target)
    {
        if (_lockSync) return;

        _lockSync = true;
        _lockSync = false;

        if (!TryParsePercent(source.Text, out double v))
        return;

        if (v > 100)
        {
        v = 100;
        _lockSync = true;
        source.Text = "100";
        source.CaretIndex = source.Text.Length;
        _lockSync = false;
        }

        if (v < 0)
        {
        v = 0;
        _lockSync = true;
        source.Text = "0";
        source.CaretIndex = source.Text.Length;
        _lockSync = false;
        }

        _lockSync = true;
        target.Text = (100 - v).ToString("F0", CultureInfo.InvariantCulture);
        _lockSync = false;
    }

    private double P(TextBox t)
    {
        double.TryParse(t.Text.Replace(',', '.'), NumberStyles.Any,
        CultureInfo.InvariantCulture, out double v);

        return v / 100.0;
    }

    private double D(TextBox t)
    {
        double.TryParse(t.Text.Replace(',', '.'), NumberStyles.Any,
        CultureInfo.InvariantCulture, out double v);

        return v;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        SummerPropCity = P(PropCitySummer);
        SummerPropHighway = P(PropHighwaySummer);
        SummerRatesCity = D(RatesCitySummer);
        SummerRatesHighway = D(RatesHighwaySummer);

        WinterPropCity = P(PropCityWinter);
        WinterPropHighway = P(PropHighwayWinter);
        WinterRatesCity = D(RatesCityWinter);
        WinterRatesHighway = D(RatesHighwayWinter);

        DialogResult = true;
        Close();
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        DragMove();
    }

    private bool TryParsePercent(string text, out double value)
    {
        return double.TryParse(
        text.Replace(',', '.'),
        NumberStyles.Any,
        CultureInfo.InvariantCulture,
        out value);
    }

    private void NumberOnly(object sender, TextCompositionEventArgs e)
    {
        char c = e.Text[0];
        TextBox tb = (TextBox)sender;

        if (char.IsDigit(c))
        return;

        if (c == '.' || c == ',')
        {
        e.Handled = tb.Text.Contains('.') || tb.Text.Contains(',');
        return;
        }

        e.Handled = true;
    }

    }
}