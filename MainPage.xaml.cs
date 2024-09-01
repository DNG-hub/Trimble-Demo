using Trimble.Models;

namespace Trimble
{
    public partial class MainPage : ContentPage
    {
        public List<Scene> Scenes { get; set; }

        public MainPage()
        {
            InitializeComponent();
            Scenes = new List<Scene>();
            SceneListView.ItemsSource = Scenes;
        }

        private async void OnNewSceneClicked(object sender, EventArgs e)
        {
            string sceneName = await DisplayPromptAsync("New Scene", "Enter scene name:");
            if (!string.IsNullOrWhiteSpace(sceneName))
            {
                var newScene = new Scene { Name = sceneName };
                Scenes.Add(newScene);
                SceneListView.ItemsSource = null;
                SceneListView.ItemsSource = Scenes;
            }
        }

        private async void OnSceneSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is Scene selectedScene)
            {
                await Navigation.PushAsync(new ScenePage(selectedScene));
            }
        }

        private async void OnOpenPointCloudClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PointCloudPage());
        }
    }
}