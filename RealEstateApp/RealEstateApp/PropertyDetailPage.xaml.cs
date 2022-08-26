using System.Text.Json;
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
using System.IO;
using System.Globalization;
using PropertyChanged;
using System.ComponentModel;
using Newtonsoft.Json.Linq;

namespace RealEstateApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PropertyDetailPage : ContentPage, INotifyPropertyChanged
    {
        public PropertyDetailPage(PropertyListItem propertyListItem)
        {
            InitializeComponent();
            GetSettings();
            Property = propertyListItem.Property;

            IRepository Repository = TinyIoCContainer.Current.Resolve<IRepository>();
            Agent = Repository.GetAgents().FirstOrDefault(x => x.Id == Property.AgentId);

            BindingContext = this;

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) => {
                // handle the tap
            };
        }
        private float _volume;
        private float _pitch;
        public float volume
        {
            get => _volume;
            set
            {
                _volume = value;
                RaisePropertyChanged();
            }
        }
        public float pitch
        {
            get => _pitch;
            set
            {
                _pitch = value;
                RaisePropertyChanged();
            }
        }
        public bool showPlayButton = true;
        public bool showStopButton = false;
        CancellationTokenSource cts;
        public event PropertyChangedEventHandler PropertyChanged;
        public Agent Agent { get; set; }

        public Property Property { get; set; }

        private async void GetSettings()
        {
            try
            {
                var volumeSetting = await SecureStorage.GetAsync("Volume_Setting");
                volume = float.Parse(volumeSetting, CultureInfo.InvariantCulture.NumberFormat);
            }
            catch (Exception)
            {
                volume = 0;
            }
            try
            {
                var pitchSettings = await SecureStorage.GetAsync("Pitch_Setting"); 
                pitch = float.Parse(pitchSettings, CultureInfo.InvariantCulture.NumberFormat);
            }
            catch (Exception)
            {
                pitch = 0;
            }
        }
        public async void SaveSettings()
        {
            try
            {
                await SecureStorage.SetAsync("Volume_Setting", $"{volume}");
            }
            catch (Exception)
            {
                await DisplayAlert("Warning", "No Volume setting have been made", "Ok");
            }
            try
            {
                await SecureStorage.SetAsync("Pitch_Setting", $"{pitch}");
            }
            catch (Exception)
            {
                await DisplayAlert("Warning", "No Volume setting have been made", "Ok");
            }
        }
        private async void EditProperty_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddEditPropertyPage(Property));
        }
        private async void TextToSpeech_Clicked(object sender, EventArgs e)
        {
            SaveSettings();
            showPlayButton = !showPlayButton;
            showStopButton = !showStopButton;
             cts = new CancellationTokenSource();
            var settings = new SpeechOptions
            {
                Volume = volume,
                Pitch = volume,
            };
        
            await TextToSpeech.SpeakAsync(Property.Description,settings, cts.Token);
            showPlayButton = !showPlayButton;
            showStopButton = !showStopButton;

        }


        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void EndTextToSpeech_Clicked(object sender, EventArgs e)
        {
            if (cts?.IsCancellationRequested ?? true)
                return;

            cts.Cancel();

            showPlayButton = !showPlayButton;
            showStopButton = !showStopButton;
        }

        private async void Email_Tapped(object sender, EventArgs e)
        {
            try
            {
                var message = new EmailMessage
                {
                    Subject = "Feedback",
                    Body = "Thanks for the excellent service!",
                    To = new List<string> {Property.Vendor.Email}
                };

                var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var attachmentFilePath = Path.Combine(folder, "property.txt");
                File.WriteAllText(attachmentFilePath, $"{Property.Address}");

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
                switch (chooser)
                {
                    case "Call":
                        PhoneDialer.Open(Property.Vendor.Phone);
                        break;
                    case "SmS":
                        var message = new SmsMessage
                        {
                            Recipients = new List<string> {Property.Vendor.Phone},
                            Body = "Hello!"
                        };
                        await Sms.ComposeAsync(message);
                        break;
                }
            }
            catch (Exception)
            {
                await DisplayAlert("Warning", "No Phone Dialer", "OK");
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

        private async void OpenBrowser_Clicked(object sender, EventArgs e)
        {
            try
            {
                var options = new BrowserLaunchOptions
                {
                    LaunchMode = BrowserLaunchMode.SystemPreferred,
                    TitleMode = BrowserTitleMode.Show,
                    PreferredToolbarColor = Color.Pink,
                    PreferredControlColor = Color.Red
                };
                await Browser.OpenAsync("http://pluralsight.com", options);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Warning", ex.Message, "ok");
            }
        }

        private async void OpenFile_Clicked(object sender, EventArgs e)
        {
            try
            {
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(Property.ContractFilePath)
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Warning", ex.Message, "ok");
            }
        }
        private async void Share_Clicked(object sender, EventArgs e)
        {
            var text = new ShareTextRequest
            {
                Title = "hare Property",
                Uri = $"{Property.NeighbourhoodUrl}",
                Subject = "A propertyyou may be interested in",
                Text = $"Address: {Property.Address} - Price: {Property.Price} - Beds: {Property.Beds}"
            };
            await Share.RequestAsync(text);
        }
        private async void ShareFile_Clicked(object sender, EventArgs e)
        {
            var file = new ShareFileRequest
            {
                Title = "Share Property Contract",
                File = new ShareFile(Property.ContractFilePath)
            };
            await Share.RequestAsync(file);
        }
        private async void ShareClipbord_Clicked(object sender, EventArgs e)
        {
            try
            {
                string json = JsonSerializer.Serialize(Property);
                await Clipboard.SetTextAsync(json);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}