using Microsoft.WindowsAzure.Storage;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ImageUploader
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

        private async void SelectImageButton_Clicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Error", "This is not supported on your device", "Ok");
                return;
            }

            var mediaOptions = new PickMediaOptions()
            {
                PhotoSize = PhotoSize.Medium
            };
            var selectedImageFile = await CrossMedia.Current.PickPhotoAsync(mediaOptions);

            if(selectedImageFile==null)
            {
                await DisplayAlert("Error", "There was an error when trying to get your image", "Ok");
                return;
            }

            selectedImage.Source = ImageSource.FromStream(() => selectedImageFile.GetStream());

            UploadImage(selectedImageFile.GetStream());
        }

        private async void UploadImage(Stream stream)
        {
            //get your storage account by giving Connection string(you can access this in Access keys part in your storage service) 
            var account = CloudStorageAccount.Parse(Keys.CONNECTION_STRING);
            var client = account.CreateCloudBlobClient(); //get blob container
            var container = client.GetContainerReference("imagecontainer"); //pass container name
            await container.CreateIfNotExistsAsync(); // creates the container with given name if it doesn't exists

            var name = Guid.NewGuid().ToString(); // a unique string
            var blockBlob = container.GetBlockBlobReference($"{name}.jpg"); // pass image file name

            await blockBlob.UploadFromStreamAsync(stream);

            string url = blockBlob.Uri.OriginalString; //this will return the url where this image will be stored
        }
    }
}
