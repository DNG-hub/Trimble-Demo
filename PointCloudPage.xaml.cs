using Microsoft.Maui.Graphics;
using System.Globalization;

namespace Trimble
{
    public partial class PointCloudPage : ContentPage
    {
        private List<Point3D> points = new List<Point3D>();
        private string plyFolderPath;
        private Picker filePicker;

        public PointCloudPage()
        {
            InitializeComponent();
            InitializePLYFolder();
            InitializePointCloudView();
        }

        private void InitializePLYFolder()
        {
            // Use the correct path where the PLY files are located
            plyFolderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "SOURCE", "REPOS", "TRIMBLE", "DOCUMENTS", "PLY");

            if (!Directory.Exists(plyFolderPath))
            {
                throw new DirectoryNotFoundException($"PLY folder not found: {plyFolderPath}");
            }
            
            // Debug: Print all files in the directory
            var allFiles = Directory.GetFiles(plyFolderPath);
            Console.WriteLine($"All files in {plyFolderPath}:");
            foreach (var file in allFiles)
            {
                Console.WriteLine(Path.GetFileName(file));
            }
        }

        private void InitializePointCloudView()
        {
            pointCloudView.Drawable = new PointCloudDrawable(points);
            pointCloudView.HeightRequest = 300;
            pointCloudView.WidthRequest = 300;
            pointCloudView.BackgroundColor = Colors.LightGray;

            filePicker = new Picker
            {
                Title = "Select a PLY file"
            };
            filePicker.SelectedIndexChanged += OnFileSelected;

            var openFileButton = new Button
            {
                Text = "Load Selected PLY File",
                Command = new Command(OnOpenFileClicked)
            };

            if (Content is Grid grid)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.Children.Add(filePicker);
                Grid.SetRow(filePicker, 0);
                grid.Children.Add(openFileButton);
                Grid.SetRow(openFileButton, 1);
            }

            LoadFileList();
        }

        private void LoadFileList()
        {
            var files = Directory.GetFiles(plyFolderPath, "*.ply");
            var fileNames = files.Select(Path.GetFileName).ToArray();
            filePicker.ItemsSource = fileNames;
        }

        private void OnFileSelected(object? sender, EventArgs e)
        {
            // This method is called when a file is selected in the Picker
            // You can add any additional logic here if needed
        }

        private void OnOpenFileClicked()
        {
            if (filePicker.SelectedItem is string selectedFile)
            {
                var fullPath = Path.Combine(plyFolderPath, selectedFile);
                try
                {
                    points = ReadPLYFile(fullPath);
                    pointCloudView.Drawable = new PointCloudDrawable(points);
                    pointCloudView.Invalidate();
                }
                catch (Exception ex)
                {
                    DisplayAlert("Error", $"An error occurred while reading the file: {ex.Message}", "OK");
                }
            }
            else
            {
                DisplayAlert("No File Selected", "Please select a PLY file first.", "OK");
            }
        }

        private List<Point3D> ReadPLYFile(string filePath)
        {
            var points = new List<Point3D>();
            using (var stream = File.OpenRead(filePath))
            using (var reader = new StreamReader(stream))
            {
                string line;
                bool headerEnd = false;
                int vertexCount = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    if (!headerEnd)
                    {
                        if (line.StartsWith("element vertex"))
                        {
                            vertexCount = int.Parse(line.Split()[2]);
                        }
                        else if (line == "end_header")
                        {
                            headerEnd = true;
                        }
                        continue;
                    }

                    var values = line.Split();
                    if (values.Length >= 3)
                    {
                        points.Add(new Point3D
                        {
                            X = float.Parse(values[0], CultureInfo.InvariantCulture),
                            Y = float.Parse(values[1], CultureInfo.InvariantCulture),
                            Z = float.Parse(values[2], CultureInfo.InvariantCulture)
                        });
                    }

                    if (points.Count >= vertexCount)
                        break;
                }
            }
            return points;
        }
    }

    public class Point3D
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    public class PointCloudDrawable : IDrawable
    {
        private readonly List<Point3D> _points;

        public PointCloudDrawable(List<Point3D> points)
        {
            _points = points;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (_points.Count == 0) return;

            var minX = _points.Min(p => p.X);
            var maxX = _points.Max(p => p.X);
            var minY = _points.Min(p => p.Y);
            var maxY = _points.Max(p => p.Y);

            var scaleX = dirtyRect.Width / (maxX - minX);
            var scaleY = dirtyRect.Height / (maxY - minY);
            var scale = Math.Min(scaleX, scaleY);

            canvas.StrokeColor = Colors.Blue;
            canvas.StrokeSize = 1;

            foreach (var point in _points)
            {
                var x = (point.X - minX) * scale;
                var y = dirtyRect.Height - (point.Y - minY) * scale;
                canvas.DrawCircle((float)x, (float)y, 1);
            }
        }
    }
}