using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DirectWrite;


namespace DXGLTF.Drawables
{
    public interface ID2DDrawable : IDisposable
    {
        void Draw(SharpDX.Direct2D1.DeviceContext context, RectangleF rect);
    }

    public class D2DWriter : ID2DDrawable
    {
        SharpDX.Direct2D1.SolidColorBrush _brush;
        TextFormat _format;

        public void Dispose()
        {
            if (_brush != null)
            {
                _brush.Dispose();
                _brush = null;
            }

            if (_format != null)
            {
                _format.Dispose();
                _format = null;
            }
        }

        void DrawText(SharpDX.Direct2D1.DeviceContext context, RectangleF rect,
            string text, string font, int size, TextAlignment alignment, ParagraphAlignment pAlignment)
        {
            if (_format == null)
            {
                using (var factory = new Factory())
                {
                    _format = new TextFormat(factory, font, size);
                    _format.TextAlignment = alignment;
                    _format.ParagraphAlignment = pAlignment;
                }
            }
            context.DrawText(text, _format, rect, _brush);
        }

        int count = 0;
        public void Draw(SharpDX.Direct2D1.DeviceContext context, RectangleF rect)
        {
            if (_brush == null)
            {
                _brush = new SharpDX.Direct2D1.SolidColorBrush(context, Color4.White);
            }

            context.Clear(Color4.Black);
            context.Transform = Matrix3x2.Identity;
            DrawText(context, rect, $"Draw: {count++}", "Verdana", 24, TextAlignment.Trailing, ParagraphAlignment.Near);
            //device.D2DDeviceContext.FillEllipse(new Ellipse(new Vector2(_rect.MouseX, _rect.MouseY), 50.0f, 50.0f), _brush);
        }
    }
}
