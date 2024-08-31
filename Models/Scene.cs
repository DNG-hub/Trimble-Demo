using System.Collections.ObjectModel;

namespace Trimble.Models
{
    public class Scene
    {
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public ObservableCollection<Measurement> Measurements { get; set; }

        public Scene()
        {
            Measurements = new ObservableCollection<Measurement>();
        }
    }
}