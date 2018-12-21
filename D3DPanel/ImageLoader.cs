using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WIC;
using System;
using System.IO;


namespace D3DPanel
{
    public static class ImageLoader
    {
        static BitmapDecoder GetDecoder(ImagingFactory factory, ImageFormat format)
        {
            switch (format)
            {
                case ImageFormat.Jpeg: return new JpegBitmapDecoder(factory);
                case ImageFormat.Png: return new PngBitmapDecoder(factory);
            }
            throw new NotImplementedException();
        }


        //
        // https://docs.microsoft.com/en-us/windows/desktop/direct3d11/overviews-direct3d-11-resources-textures-how-to
        //
        public static void LoadImage(ImageBytes image, Action<DataStream, Format, int, int> callback)
        {
            var bytes = image.Bytes;
            using (var s = new MemoryStream(bytes.Array, bytes.Offset, bytes.Count, false))
            using (var factory = new ImagingFactory())
            using (var stream = new WICStream(factory, s))
            using (var decoder = GetDecoder(factory, image.Format))
            {
                decoder.Initialize(stream, DecodeOptions.CacheOnDemand);
                using (var frame = decoder.GetFrame(0))
                {
                    var format = default(Format);
                    var stride = frame.Size.Width * 4;

                    using (var buffer = new DataStream(frame.Size.Height * stride, true, true))
                    {
                        if (frame.PixelFormat == PixelFormat.Format32bppBGRA)
                        {
                            // OK
                            format = Format.B8G8R8A8_UNorm;
                            frame.CopyPixels(stride, buffer);
                        }
                        else
                        {
                            // Convert
                            var fc = new FormatConverter(factory);
                            fc.Initialize(frame, PixelFormat.Format32bppBGR);
                            fc.CopyPixels(stride, buffer);
                            format = Format.B8G8R8A8_UNorm;
                        }

                        callback(buffer, format, frame.Size.Width, frame.Size.Height);
                    }
                };
            }
        }
    }
}
