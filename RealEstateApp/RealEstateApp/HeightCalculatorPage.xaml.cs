using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using RealEstateApp.Models;
using System.Collections.ObjectModel;
using PropertyChanged;

namespace RealEstateApp
{
    [AddINotifyPropertyChangedInterface]
    public partial class HeightCalculatorPage : ContentPage
    {
        public double seaLevelPressure = 1021.6;
        SensorSpeed speed = SensorSpeed.UI;
        public double CurrentPressure { get; set; }
        public double CurrentAltitude { get; set; }
        public string LabelOfMesserument { get; set; }

        public ObservableCollection<BarometerMeasurement> Measurements { get; set; } = new ObservableCollection<BarometerMeasurement>();

        public HeightCalculatorPage()
        {
            InitializeComponent();
            BindingContext = this;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            Barometer.ReadingChanged += Barometer_ReadingChanged;
            Barometer.Start(speed);
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Barometer.ReadingChanged -= Barometer_ReadingChanged;
            if (Barometer.IsMonitoring)
                Barometer.Stop();
        }
        void Barometer_ReadingChanged(object sender, BarometerChangedEventArgs e)
        {
            var data = e.Reading;
            CurrentPressure = data.PressureInHectopascals;

            CalculateAltitude(CurrentPressure);
        }

        public double CalculateAltitude(double _presure)
        {
            CurrentAltitude = 44307.694 * (1 - Math.Pow(CurrentPressure / seaLevelPressure, 0.190284));
            return CurrentAltitude;
        }
        private void SaveMeasurement_Clicked(object sender, EventArgs e)
        {
            BarometerMeasurement barometer = new BarometerMeasurement();
            barometer.Altitude = CurrentAltitude;
            barometer.Pressure = CurrentPressure;
            barometer.Label = LabelOfMesserument;
            barometer.HeightChange = (CurrentAltitude - Measurements.FirstOrDefault()?.Altitude ?? 0);
            Measurements.Add(barometer);
            OnPropertyChanged();

        }
    }
}