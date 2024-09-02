using Microsoft.Maui.Graphics;
using System.Globalization;

namespace Trimble
{
    public partial class PointCloudPage : ContentPage
    {
        private List<Point3D> points = new List<Point3D>();
        private string plyFolderPath;

        public PointCloudPage()
        {
            InitializeComponent();
            InitializePLYFolder();
            InitializePointCloudView();
        }

        private void InitializePLYFolder()
        {
            plyFolderPath = Path.Combine(FileSystem.AppDataDirectory, "Documents", "PLY");
            if (!Directory.Exists(plyFolderPath))
            {
                Directory.CreateDirectory(plyFolderPath);
            }
        }

        private void InitializePointCloudView()
        {
            pointCloudView.Drawable = new PointCloudDrawable(points);
            pointCloudView.HeightRequest = 300;
            pointCloudView.WidthRequest = 300;
            pointCloudView.BackgroundColor = Colors.LightGray;

            var openFileButton = new Button
            {
                Text = "Open PLY File from Documents\\PLY",
                Command = new Command(() => OnOpenFileClicked(null, EventArgs.Empty))
            };

            if (Content is Grid grid)
            {
                grid.Children.Insert(0, openFileButton);
            }
        }

        private async void OnOpenFileClicked(object sender, EventArgs e)
        {
            var files = Directory.GetFiles(plyFolderPath, "*.ply");
            if (files.Length == 0)
            {
                await DisplayAlert("No Files", "No .ply files found in the Documents\\PLY folder.", "OK");
                return;
            }

            var fileNames = files.Select(Path.GetFileName).ToArray();
            var selectedFile = await DisplayActionSheet("Select a PLY file", "Cancel", null, fileNames);

            if (selectedFile != "Cancel" && !string.IsNullOrEmpty(selectedFile))
            {
                var fullPath = Path.Combine(plyFolderPath, selectedFile);
                try
                {
                    points = await ReadPLYFile(fullPath);
                    pointCloudView.Drawable = new PointCloudDrawable(points);
                    pointCloudView.Invalidate();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"An error occurred while reading the file: {ex.Message}", "OK");
                }
            }
        }

        private async Task<List<Point3D>> ReadPLYFile(string filePath)
        {
            var points = new List<Point3D>();
            using (var stream = File.OpenRead(filePath))
            using (var reader = new StreamReader(stream))
            {
                string line;
                bool headerEnd = false;
                int vertexCount = 0;

                while ((line = await reader.ReadLineAsync()) != null)
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