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

        static Format GetFormat(Guid guid)
        {
            if (guid == PixelFormat.Format32bppBGRA)
            {
                return Format.B8G8R8A8_UNorm;
            }

            if (guid == PixelFormat.Format24bppBGR)
            {
                // require conversion to 32bit
                throw new NotImplementedException();
            }

            if(guid == PixelFormat.Format8bppIndexed)
            {
                // require conversion to 32bit
                throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }

        //
        // https://docs.microsoft.com/en-us/windows/desktop/direct3d11/overviews-direct3d-11-resources-textures-how-to
        //
        public static (DataStream, Texture2DDescription, int) LoadImage(ImageBytes image)
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
                    var desc = new Texture2DDescription
                    {
                        Width = frame.Size.Width,
                        Height = frame.Size.Height,
                        MipLevels = 1,
                        ArraySize = 1,
                        Format = GetFormat(frame.PixelFormat),
                        SampleDescription = new SampleDescription
                        {
                            Count = 1,
                            Quality = 0
                        },
                        Usage = ResourceUsage.Default,
                        BindFlags = BindFlags.ShaderResource
                    };

                    var stride = desc.Width * FormatHelper.SizeOfInBytes(desc.Format);
                    var buffer = new DataStream(desc.Height * stride, true, true);
                    frame.CopyPixels(stride, buffer);
                    return (buffer, desc, stride);
                };
            }
        }
    }
}
