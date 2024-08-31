using Microsoft.Maui.Graphics;

namespace Trimble.Graphics
{
    public class GraphicsDrawable : IDrawable
    {
        private readonly Action<ICanvas, RectF> _draw;

        public GraphicsDrawable(Action<ICanvas, RectF> draw)
        {
            _draw = draw;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            _draw(canvas, dirtyRect);
        }
    }
}