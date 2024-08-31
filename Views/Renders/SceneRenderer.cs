using Microsoft.Maui.Graphics;
using Trimble.Models;

namespace Trimble.Views.Renderers
{
     public class SceneRenderer
    {
        private Scene _scene;
        private ICanvas? _canvas;
        private RectF _bounds;
        private const float GridSpacing = 50f;
        private const float MeasurementRadius = 5f;

        public SceneRenderer(Scene scene)
        {
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
            _bounds = new RectF();
        }

        public void SetCanvas(ICanvas canvas)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        }

        public void SetBounds(float width, float height)
        {
            _bounds = new RectF(0, 0, width, height);
        }

        public void Draw()
        {
            if (_canvas == null)
            {
                throw new InvalidOperationException("Canvas has not been set. Call SetCanvas before drawing.");
            }

            DrawBackground();
            DrawGrid();
            DrawMeasurements();
        }

        private void DrawBackground()
        {
            _canvas!.FillColor = Colors.White;
            _canvas.FillRectangle(_bounds);
        }

        private void DrawGrid()
        {
            _canvas!.StrokeColor = Colors.LightGray;
            _canvas.StrokeSize = 1;

            for (float x = 0; x < _bounds.Width; x += GridSpacing)
            {
                _canvas.DrawLine(x, 0, x, _bounds.Height);
            }

            for (float y = 0; y < _bounds.Height; y += GridSpacing)
            {
                _canvas.DrawLine(0, y, _bounds.Width, y);
            }
        }

        private void DrawMeasurements()
        {
            if (_scene.Measurements == null || _scene.Measurements.Count == 0)
                return;

            var (scaleX, scaleY, offsetX, offsetY) = CalculateScaleAndOffset();

            _canvas!.StrokeColor = Colors.Blue;
            _canvas.StrokeSize = 2;

            foreach (var measurement in _scene.Measurements)
            {
                var x = (measurement.X - offsetX) * scaleX;
                var y = _bounds.Height - (measurement.Y - offsetY) * scaleY; // Invert Y-axis

                _canvas.DrawCircle((float)x, (float)y, MeasurementRadius);
            }
        }

        private (double scaleX, double scaleY, double offsetX, double offsetY) CalculateScaleAndOffset()
        {
            var minX = _scene.Measurements.Min(m => m.X);
            var maxX = _scene.Measurements.Max(m => m.X);
            var minY = _scene.Measurements.Min(m => m.Y);
            var maxY = _scene.Measurements.Max(m => m.Y);

            var dataWidth = maxX - minX;
            var dataHeight = maxY - minY;

            var scaleX = _bounds.Width / dataWidth;
            var scaleY = _bounds.Height / dataHeight;

            // Use the smaller scale to ensure all points fit
            var scale = Math.Min(scaleX, scaleY);

            // Calculate offsets to center the data
            var offsetX = minX - ((_bounds.Width / scale - dataWidth) / 2);
            var offsetY = minY - ((_bounds.Height / scale - dataHeight) / 2);

            return (scale, scale, offsetX, offsetY);
        }
    }
}