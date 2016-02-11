using System;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using Windows.UI.Xaml.Input;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace CatsApp
{
    public sealed partial class MainPage : Page
    {
        string _imageId = "";
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async Task NextCat()
        {
            Uri randomCatUri = new Uri(string.Format("http://thecatapi.com/api/images/get?format=xml&size=med&cache={0}", Guid.NewGuid()));
            try
            {
                var httpClient = new HttpClient();

                HttpResponseMessage randomeCatResponse = await httpClient.GetAsync(randomCatUri).AsTask();

                CatLove.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                CatHater.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                string responseBodyAsText;
                responseBodyAsText = await SetCatImage(randomeCatResponse);

                _imageId = GetResponseValue("id", responseBodyAsText);

            }
            catch (Exception ex)
            {
                ErrorText.Visibility = Windows.UI.Xaml.Visibility.Visible;
                ErrorText.Content = "Meow!! Issues with the Cattery. Retry";
            }
        }

        private async void Vote(int score)
        {
            if (_imageId != "")
            {
                Uri rateCatUri = new Uri(string.Format("http://thecatapi.com/api/images/vote?api_key=MjMzOTQ&sub_id=12345&image_id={0}&score={1}&cache={2}", _imageId, score, Guid.NewGuid()));
                var httpClient = new HttpClient();
                HttpResponseMessage ratedCatResponse = await httpClient.GetAsync(rateCatUri).AsTask();
            }
        }

        private async Task<string> SetCatImage(HttpResponseMessage response)
        {
            string responseBodyAsText = await response.Content.ReadAsStringAsync().AsTask();
          
            var catUri = new Uri(GetResponseValue("url", responseBodyAsText));
            var imageSource = new BitmapImage(catUri);
            var height = imageSource.PixelHeight;
            CatPlaceHolder.Source = imageSource;
            return responseBodyAsText;
        }

        private string GetResponseValue(string tagName, string response)
        {
            var urlStart = response.IndexOf(string.Format("<{0}>", tagName)) + 5;
            var urlEnd = response.IndexOf(string.Format("</{0}>", tagName));

            return response.Substring(urlStart, urlEnd - urlStart);
        }


        private async void CatPlaceHolder_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingRoutedEventArgs e)
        {
            if (e.Velocities.Linear.X > 0)
            {
                CatLove.Visibility = Windows.UI.Xaml.Visibility.Visible;
                Vote(10);
            }
            else if (e.Velocities.Linear.X < 0)
            {
                CatHater.Visibility = Windows.UI.Xaml.Visibility.Visible;
                Vote(1);
            }
            await NextCat();
        }

        private async void ErrorText_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ErrorText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            await NextCat();
        }
    }
}
