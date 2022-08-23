using RealEstateApp.Models;
using RealEstateApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using TinyIoC;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RealEstateApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddEditPropertyPage : ContentPage
    {
        private IRepository Repository;

        #region PROPERTIES
        public ObservableCollection<Agent> Agents { get; }

        private Property _property;
        private Agent _selectedAgent;
        public Property Property
        {
            get => _property;
            set
            {
                _property = value;
                if (_property.AgentId != null)
                {
                    SelectedAgent = Agents.FirstOrDefault(x => x.Id == _property?.AgentId);
                }

            }
        }
        public Agent SelectedAgent
        {
            get => _selectedAgent;
            set
            {
                if (Property != null)
                {
                    _selectedAgent = value;
                    Property.AgentId = _selectedAgent?.Id;
                }
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (Connectivity.NetworkAccess == NetworkAccess.Internet
                && Connectivity.ConnectionProfiles.Contains(ConnectionProfile.WiFi))
            {
                Geocoding_FromAddress.IsVisible = true;
                Geocoding_ToAddress.IsVisible = true;
            }
            else
            {
                Geocoding_FromAddress.IsVisible = false;
                Geocoding_ToAddress.IsVisible = false;
            }
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Vibration.Cancel();
        }

        public string StatusMessage { get; set; }

        public Color StatusColor { get; set; } = Color.White;
        #endregion

        public AddEditPropertyPage(Property property = null)
        {
            InitializeComponent();

            Repository = TinyIoCContainer.Current.Resolve<IRepository>();
            Agents = new ObservableCollection<Agent>(Repository.GetAgents());

            if (property == null)
            {
                Title = "Add Property";
                Property = new Property();
            }
            else
            {
                Title = "Edit Property";
                Property = property;
            }

            BindingContext = this;
        }

        private async void SaveProperty_Clicked(object sender, System.EventArgs e)
        {
            if (IsValid() == false)
            {
                StatusMessage = "Please fill in all required fields";
                StatusColor = Color.Red;
            }
            else
            {
                Repository.SaveProperty(Property);
                await Navigation.PopToRootAsync();
            }
        }

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(Property.Address)
                || Property.Beds == null
                || Property.Price == null
                || Property.AgentId == null)
                return false;

            return true;
        }

        private async void CancelSave_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }

        private async void SetAddressFromLocation_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                var location = await Geolocation.GetLocationAsync();

                if (location != null)
                {
                    Property.Latitude = location.Latitude;

                    Property.Longitude = location.Longitude;

                    Location location1 = new Location(Property.Latitude.Value, Property.Longitude.Value);

                    var address = (await Geocoding.GetPlacemarksAsync(location1))
                        .FirstOrDefault();
                    Property.Address = $"{address.Thoroughfare} {address.SubThoroughfare}, {address.PostalCode}, {address.CountryName}";
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
            }
            catch (Exception ex)
            {
                // Unable to get location
            }
        }
        private async void SetLocationFromAddress(object sender, System.EventArgs e)
        {
            string _address = Address.Text;
            if (string.IsNullOrWhiteSpace(_address))
            {
                await DisplayAlert("ALERT", "Address Must be filled out, with something like this: address 123, 4700, Denmark", "OK");
                RunVibration();
            }
            else
            {
                Property.Address = _address;

                var _Locations = await Geocoding.GetLocationsAsync(_address);

                Location _location = _Locations?.FirstOrDefault();

                Property.Latitude = _location.Latitude;
                Property.Longitude = _location.Longitude;

            }
        }
        private void RunVibration()
        {
            Vibration.Vibrate(TimeSpan.FromSeconds(5));
        }
    }
}