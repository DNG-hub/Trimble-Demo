using Microsoft.Maui.Graphics;
using System.Globalization;

namespace Trimble
{
    public partial class PointCloudPage : ContentPage
    {
        private List<Point3D> points = new List<Point3D>();

        public PointCloudPage()
        {
            InitializeComponent();
            pointCloudView.Drawable = new PointCloudDrawable(points);
        }

        private async void OnOpenFileClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.PickAsync();
                if (result != null)
                {
                    points = await ReadPLYFile(result.FullPath);
                    pointCloudView.Invalidate();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async Task<List<Point3D>> ReadPLYFile(string filePath)
        {
            var points = new List<Point3D>();
            using (var stream = await FileSystem.OpenAppPackageFileAsync(filePath))
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