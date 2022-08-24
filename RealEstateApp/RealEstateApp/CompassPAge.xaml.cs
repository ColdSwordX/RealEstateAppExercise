using PropertyChanged;
using RealEstateApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RealEstateApp
{
    [AddINotifyPropertyChangedInterface]
    public partial class CompassPAge : ContentPage
    {
        private Property _Property;
        public CompassPAge(Property _property)
        {
            InitializeComponent();
            _Property = _property;
            BindingContext = this;
        }
        public string CurrentAspect { get; set; }
        public double RotationAngle { get; set; }
        public double CurrentHeading { get; set; }
        SensorSpeed speed = SensorSpeed.UI;

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Compass.ReadingChanged += Compass_ReadingChanged;
            Compass.Start(speed);
        }
        protected override void OnDisappearing()
        {
            if (Compass.IsMonitoring)
                Compass.Stop();
            Compass.ReadingChanged -= Compass_ReadingChanged;
        }
        void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            CurrentHeading = e.Reading.HeadingMagneticNorth;
            RotationAngle = CurrentHeading * -1;

            if (CurrentHeading > 315 || CurrentHeading < 45)
            {
                CurrentAspect = "North";
            }
            else if(CurrentHeading > 45 && CurrentHeading < 135)
            {
                CurrentAspect = "Øst";
            }
            else if (CurrentHeading > 135 &&  CurrentHeading < 225)
            {
                CurrentAspect = "Syd";
            }
            else if (CurrentHeading > 225 && CurrentHeading < 315)
            {
                CurrentAspect = "Vest";
            }
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            _Property.Aspect = CurrentAspect;
            await Navigation.PopModalAsync();
        }
    }
}