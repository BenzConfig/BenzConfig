using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace benzconfig
{
    public partial class MainWindow : Window
    {
        private const string SettingsFile = "settings.json";
        private double SummerPropCity;
        private double SummerPropHighway;
        private double SummerRatesCity;
        private double SummerRatesHighway;
        private double WinterPropCity;
        private double WinterPropHighway;
        private double WinterRatesCity;
        private double WinterRatesHighway;

        private CancellationTokenSource? _typingCtsSummer;
        private CancellationTokenSource? _typingCtsWinter;

        private enum ActiveSection
        {
            Summer,
            Winter
        }

        private ActiveSection _lastActiveSection = ActiveSection.Summer;

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow { Owner = this };
            aboutWindow.ShowDialog();
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new Settings(
                SummerPropCity, SummerPropHighway, SummerRatesCity, SummerRatesHighway,
                WinterPropCity, WinterPropHighway, WinterRatesCity, WinterRatesHighway)
            { Owner = this };

            if (win.ShowDialog() == true)
            {
                SummerPropCity = win.SummerPropCity;
                SummerPropHighway = win.SummerPropHighway;
                SummerRatesCity = win.SummerRatesCity;
                SummerRatesHighway = win.SummerRatesHighway;

                WinterPropCity = win.WinterPropCity;
                WinterPropHighway = win.WinterPropHighway;
                WinterRatesCity = win.WinterRatesCity;
                WinterRatesHighway = win.WinterRatesHighway;

                RecalculateSummer();
                RecalculateWinter();
            }
        }

        private void RecalculateSummer()
        {
            BtnSummer_Click(this, new RoutedEventArgs());
        }

        private void RecalculateWinter()
        {
            BtnWinter_Click(this, new RoutedEventArgs());
        }

        private void NumberOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[\d.,]+$");
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Summer_GotFocus(object sender, RoutedEventArgs e)
        {
            _lastActiveSection = ActiveSection.Summer;
        }

        private void Winter_GotFocus(object sender, RoutedEventArgs e)
        {
            _lastActiveSection = ActiveSection.Winter;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            switch (_lastActiveSection)
            {
                case ActiveSection.Summer:
                    BtnSummer_Click(BtnSummer, new RoutedEventArgs());
                    break;

                case ActiveSection.Winter:
                    BtnWinter_Click(BtnWinter, new RoutedEventArgs());
                    break;
            }

            e.Handled = true;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async Task TypeTextAsync(TextBox textBox, string text, int delayMs, CancellationToken token)
        {
            textBox.Clear();

            try
            {
                foreach (char c in text)
                {
                    token.ThrowIfCancellationRequested();
                    textBox.AppendText(c.ToString());
                    textBox.ScrollToEnd();
                    await Task.Delay(delayMs, token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private bool TryGetDistance(TextBox input, TextBox output, out double distance)
        {
            distance = 0;

            string text = input.Text.Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                output.Text = "Введите корректные данные";
                input.Focus();
                return false;
            }

            if (!double.TryParse(
                text.Replace(',', '.'),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out distance))
            {
                output.Text = "Некорректные данные\nВведите корректные данные";
                input.Focus();
                return false;
            }

            if (distance <= 0)
            {
                output.Text = "Данные должены быть больше нуля";
                input.Focus();
                return false;
            }

            return true;
        }

        private async void BtnSummer_Click(object? sender, RoutedEventArgs? e)
        {
            if (!TryGetDistance(InputSummer, OutSummer, out double distance))
                return;

            double roadCity = Math.Round(SummerPropCity * distance, 2);
            double roadHighway = Math.Round(SummerPropHighway * distance, 2);

            double resultCity = Math.Round(roadCity / 100 * SummerRatesCity, 2);
            double resultHighway = Math.Round(roadHighway / 100 * SummerRatesHighway, 2);

            double total = Math.Round(resultCity + resultHighway, 2);

            string resultText =
                $"Общий расход: {total} л\n\n" +
                $"Детализация\n" +
                $"Пробег по городу: {roadCity} км\n" +
                $"Пробег по трассе: {roadHighway} км\n\n" +
                $"Нормы расхода\n" +
                $"Город: {SummerRatesCity:F2} л на 100 км\n" +
                $"Трасса: {SummerRatesHighway:F2} л на 100 км\n\n" +
                $"Пропорции\n" +
                $"Городской режим: {SummerPropCity * 100:F0}%\n" +
                $"Трассовый режим: {SummerPropHighway * 100:F0}%";

            _typingCtsSummer?.Cancel();
            _typingCtsSummer = new CancellationTokenSource();

            await TypeTextAsync(
                OutSummer,
                resultText,
                1,
                _typingCtsSummer.Token);
        }

        private async void BtnWinter_Click(object? sender, RoutedEventArgs? e)
        {
            if (!TryGetDistance(InputWinter, OutWinter, out double distance))
                return;

            double roadCity = Math.Round(WinterPropCity * distance, 2);
            double roadHighway = Math.Round(WinterPropHighway * distance, 2);

            double resultCity = Math.Round(roadCity / 100 * WinterRatesCity, 2);
            double resultHighway = Math.Round(roadHighway / 100 * WinterRatesHighway, 2);

            double total = Math.Round(resultCity + resultHighway, 2);

            string resultText =
                $"Общий расход: {total} л\n\n" +
                $"Детализация\n" +
                $"Пробег по городу: {roadCity} км\n" +
                $"Пробег по трассе: {roadHighway} км\n\n" +
                $"Нормы расхода\n" +
                $"Город: {WinterRatesCity:F2} л на 100 км\n" +
                $"Трасса: {WinterRatesHighway:F2} л на 100 км\n\n" +
                $"Пропорции\n" +
                $"Городской режим: {WinterPropCity * 100:F0}%\n" +
                $"Трассовый режим: {WinterPropHighway * 100:F0}%";

            _typingCtsWinter?.Cancel();
            _typingCtsWinter = new CancellationTokenSource();

            await TypeTextAsync(
                OutWinter,
                resultText,
                1,
                _typingCtsWinter.Token);
        }

        private void LoadSettings()
        {
            if (!File.Exists(SettingsFile))
            {
                SummerPropCity = 0.3;
                SummerPropHighway = 0.7;
                SummerRatesCity = 11.5;
                SummerRatesHighway = 8.5;

                WinterPropCity = 0.3;
                WinterPropHighway = 0.7;
                WinterRatesCity = 13.8;
                WinterRatesHighway = 10.2;
                return;
            }

            try
            {
                var json = File.ReadAllText(SettingsFile);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);

                if (settings != null)
                {
                    SummerPropCity = settings.Summer.PropCity;
                    SummerPropHighway = settings.Summer.PropHighway;
                    SummerRatesCity = settings.Summer.RateCity;
                    SummerRatesHighway = settings.Summer.RateHighway;

                    WinterPropCity = settings.Winter.PropCity;
                    WinterPropHighway = settings.Winter.PropHighway;
                    WinterRatesCity = settings.Winter.RateCity;
                    WinterRatesHighway = settings.Winter.RateHighway;
                }
            }
            catch
            {
                MessageBox.Show("Ошибка загрузки настроек", "Ошибка");
            }
        }

        private void SaveSettings()
        {
            var settings = new AppSettings
            {
                Summer = new SeasonSettings
                {
                    PropCity = SummerPropCity,
                    PropHighway = SummerPropHighway,
                    RateCity = SummerRatesCity,
                    RateHighway = SummerRatesHighway
                },

                Winter = new SeasonSettings
                {
                    PropCity = WinterPropCity,
                    PropHighway = WinterPropHighway,
                    RateCity = WinterRatesCity,
                    RateHighway = WinterRatesHighway
                }
            };

            File.WriteAllText(SettingsFile,
                JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
            base.OnClosing(e);
        }

    }
}