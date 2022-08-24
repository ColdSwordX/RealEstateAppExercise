using RealEstateApp.Models;
using RealEstateApp.Services;
using System;
using System.Linq;
using System.Threading;
using TinyIoC;
using Xamarin.Essentials;
using Xamarin.Forms;
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
    }
}