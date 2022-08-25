using RealEstateApp.Models;
using RealEstateApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using TinyIoC;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;

namespace RealEstateApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PropertyDetailPage : ContentPage
    {
        public PropertyDetailPage(PropertyListItem propertyListItem)
        {
            InitializeComponent();

            Property = propertyListItem.Property;

            IRepository Repository = TinyIoCContainer.Current.Resolve<IRepository>();
            Agent = Repository.GetAgents().FirstOrDefault(x => x.Id == Property.AgentId);

            BindingContext = this;

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) => {
                // handle the tap
            };
        }

        CancellationTokenSource cts;
        public Agent Agent { get; set; }

        public Property Property { get; set; }

        private async void EditProperty_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddEditPropertyPage(Property));
        }
        private async void TextToSpeech_Clicked(object sender, EventArgs e)
        {
            StartTextToSpeech.IsVisible = false;
            EndTextToSpeech.IsVisible = true;
            cts = new CancellationTokenSource();
            await TextToSpeech.SpeakAsync(Property.Description, cts.Token);
        }

        private void EndTextToSpeech_Clicked(object sender, EventArgs e)
        {
            if (cts?.IsCancellationRequested ?? true)
                return;

            cts.Cancel();
            StartTextToSpeech.IsVisible = true;
            EndTextToSpeech.IsVisible = false;
        }

        private async void Email_Tapped(object sender, EventArgs e)
        {
            try
            {
                var message = new EmailMessage
                {
                    Subject = "Feedback",
                    Body = "Thanks for the excellent service!",
                    To = new List<string> { "jensneergaard@hotmail.com" }
                };

                await Email.ComposeAsync(message);
            }
            catch (Exception ex)
            {

                await DisplayAlert("Warning",ex.Message,"ok");
            }
        }
        private async void Phone_Tapped(object sender, EventArgs e)
        {
            string[] buttons = {"Call", "SmS"};
            string chooser = "";
            try
            {
                chooser = await DisplayActionSheet(Property.Vendor.Phone, "Cancel", null,buttons);
            }
            catch (Exception)
            {
                await DisplayAlert("Warning", "No Phone Dialer", "OK");
            }
            switch (chooser)
            {
                case "Call":
                    PhoneDialer.Open("22392361");
                    break;
                case "SmS":
                    var message = new SmsMessage
                    {
                        Recipients = new List<string> { "22392361" },
                        Body = "Hello!"
                    };
                    await Sms.ComposeAsync(message);
                    break;
            }
        }

        private async void OpenMaps_Clicked(object sender, EventArgs e)
        {
            var placement = await Geocoding.GetPlacemarksAsync((double)Property.Latitude, (double)Property.Longitude);
            
            await Map.OpenAsync(placement.FirstOrDefault());
        }

        private async void OpenNavigation_Clicked(object sender, EventArgs e)
        {

            var location = new Location((double)Property.Latitude, (double)Property.Longitude);

            var options = new MapLaunchOptions
            {
                Name = "MyMaps",
                NavigationMode = NavigationMode.Driving
            };
            await Map.OpenAsync(location, options);
        }
    }
}