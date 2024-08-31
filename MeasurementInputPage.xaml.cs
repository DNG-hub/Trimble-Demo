using Trimble.Models; // Add this line at the top of the file

namespace Trimble
{
    public partial class MeasurementInputPage : ContentPage
    {
        private readonly Scene _currentScene; // Assuming _currentScene is of type Scene

        // Constructor that accepts a Scene parameter
        public MeasurementInputPage(Scene currentScene)
        {
            InitializeComponent();
            _currentScene = currentScene;

            // Use _currentScene as needed within your page
        }
    }
}
