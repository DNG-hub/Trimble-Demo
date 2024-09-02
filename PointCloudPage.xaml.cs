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
            try
            {
                plyFolderPath = Path.Combine(FileSystem.AppDataDirectory, "Documents", "PLY");
                Console.WriteLine($"Attempting to access or create PLY folder at: {plyFolderPath}");

                if (!Directory.Exists(plyFolderPath))
                {
                    Console.WriteLine("PLY folder does not exist. Creating it now.");
                    Directory.CreateDirectory(plyFolderPath);
                    Console.WriteLine("PLY folder created successfully.");
                }
                else
                {
                    Console.WriteLine("PLY folder already exists.");
                }

                var files = Directory.GetFiles(plyFolderPath, "*.ply");
                Console.WriteLine($"Number of .ply files found: {files.Length}");
                foreach (var file in files)
                {
                    Console.WriteLine($"Found file: {Path.GetFileName(file)}");
                }

                if (files.Length == 0)
                {
                    Console.WriteLine("No .ply files found in the PLY folder.");
                    Console.WriteLine("Please ensure you have copied some .ply files to this location:");
                    Console.WriteLine(plyFolderPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while initializing the PLY folder: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Fallback to a different path if the initial one fails
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
        }

        private void InitializePointCloudView()
        {
            pointCloudView.Drawable = new PointCloudDrawable(points);
            pointCloudView.HeightRequest = 300;
            pointCloudView.WidthRequest = 300;
            pointCloudView.BackgroundColor = Colors.LightGray;

            var openFileButton = new Button
            {
                Text = "Load Selected PLY File",
                Command = new Command(() => OnOpenFileClicked(null, EventArgs.Empty))
            };

            if (Content is Grid grid)
            {
                grid.Children.Add(openFileButton);
                Grid.SetRow(openFileButton, 0);
            }
        }

        private async void OnOpenFileClicked(object sender, EventArgs e)
        {
            try
            {
                var files = Directory.GetFiles(plyFolderPath, "*.ply");
                if (files.Length == 0)
                {
                    await DisplayAlert("No Files", "No .ply files found in the specified folder.", "OK");
                    return;
                }

                var fileNames = files.Select(Path.GetFileName).ToArray();
                var selectedFile = await DisplayActionSheet("Select a PLY file", "Cancel", null, fileNames);

                if (selectedFile != "Cancel" && !string.IsNullOrEmpty(selectedFile))
                {
                    var fullPath = Path.Combine(plyFolderPath, selectedFile);
                    points = await ReadPLYFile(fullPath);
                    pointCloudView.Drawable = new PointCloudDrawable(points);
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
            using (var stream = File.OpenRead(filePath))
            using (var reader = new BinaryReader(stream))
            {
                string header = "";
                bool headerEnd = false;
                int vertexCount = 0;
                bool isBinary = false;
                bool isLittleEndian = true;

                // Read the header
                while (!headerEnd)
                {
                    var line = await ReadLineAsync(reader);
                    header += line + "\n";

                    if (line.StartsWith("format"))
                    {
                        if (line.Contains("binary_little_endian"))
                        {
                            isBinary = true;
                            isLittleEndian = true;
                        }
                        else if (line.Contains("binary_big_endian"))
                        {
                            isBinary = true;
                            isLittleEndian = false;
                        }
                        else if (line.Contains("ascii"))
                        {
                            isBinary = false;
                        }
                        else
                        {
                            throw new FormatException("Unsupported PLY format.");
                        }
                    }
                    else if (line.StartsWith("element vertex"))
                    {
                        vertexCount = int.Parse(line.Split()[2]);
                    }
                    else if (line == "end_header")
                    {
                        headerEnd = true;
                    }
                }

                // Verify that this is a binary file with the expected properties
                if (!isBinary)
                {
                    throw new FormatException("PLY file is not in binary format as expected.");
                }

                // Read vertex data
                for (int i = 0; i < vertexCount; i++)
                {
                    float x = isLittleEndian ? reader.ReadSingle() : BitConverter.ToSingle(reader.ReadBytes(4).Reverse().ToArray(), 0);
                    float y = isLittleEndian ? reader.ReadSingle() : BitConverter.ToSingle(reader.ReadBytes(4).Reverse().ToArray(), 0);
                    float z = isLittleEndian ? reader.ReadSingle() : BitConverter.ToSingle(reader.ReadBytes(4).Reverse().ToArray(), 0);

                    points.Add(new Point3D { X = x, Y = y, Z = z });
                }
            }
            return points;
        }

        private async Task<string> ReadLineAsync(BinaryReader reader)
        {
            var result = new List<byte>();
            byte b;
            while ((b = reader.ReadByte()) != '\n')
            {
                result.Add(b);
            }
            return System.Text.Encoding.ASCII.GetString(result.ToArray()).TrimEnd('\r');
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
}