using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Linq;
using System.Runtime.InteropServices;


namespace D3DPanel
{
    public class D3D11Material : IDisposable
    {
        D3D11Shader m_shader;

        // texture0
        ImageBytes m_textureBytes;
        ShaderResourceView m_srv;
        SamplerState m_ss;

        RasterizerState m_rs;

        // material color
        public Color4 Color
        {
            get;
            private set;
        }

        public D3D11Material(D3D11Shader shader, ImageBytes texture, Color4 color)
        {
            m_shader = shader;
            m_textureBytes = texture;
            Color = color;
        }

        public void Dispose()
        {
            if (m_rs != null)
            {
                m_rs.Dispose();
                m_rs = null;
            }

            if (m_ss != null)
            {
                m_ss.Dispose();
                m_ss = null;
            }

            if (m_srv != null)
            {
                m_srv.Dispose();
                m_srv = null;
            }

            if (m_shader != null)
            {
                m_shader.Dispose();
                m_shader = null;
            }
        }

        public void Draw(D3D11Renderer renderer, D3D11Mesh mesh)
        {
            var device = renderer.Device;
            var context = renderer.Context;

            // shader
            m_shader.SetupContext(device, context);

            // material
            if (m_srv == null)
            {
                if (m_textureBytes.Bytes.Count > 0)
                {
                    ImageLoader.LoadImage(m_textureBytes, (buffer, format, w, h) =>
                    {
                        var desc = new Texture2DDescription
                        {
                            Width = w,
                            Height = h,
                            MipLevels = 1,
                            ArraySize = 1,
                            Format = format,
                            SampleDescription = new SharpDX.DXGI.SampleDescription
                            {
                                Count = 1,
                                Quality = 0
                            },
                            Usage = ResourceUsage.Default,
                            BindFlags = BindFlags.ShaderResource
                        };
                        var rect = new DataRectangle(buffer.DataPointer, w * 4);
                        using (var texture = new Texture2D(device, desc, rect))
                        {
                            m_srv = new ShaderResourceView(device, texture);
                        }
                    });
                }
                else
                {
                    // default white2x2
                    var desc = new Texture2DDescription
                    {
                        Width = 2,
                        Height = 2,
                        MipLevels = 1,
                        ArraySize = 1,
                        Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                        SampleDescription = new SharpDX.DXGI.SampleDescription
                        {
                            Count = 1,
                            Quality = 0
                        },
                        Usage = ResourceUsage.Default,
                        BindFlags = BindFlags.ShaderResource
                    };

                    var bytes = Enumerable.Repeat((byte)255, 2 * 2 * 4).ToArray();
                    {
                        var pinnedArray = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                        var ptr = pinnedArray.AddrOfPinnedObject();
                        var rect = new DataRectangle(ptr, 2 * 4);
                        using (var texture = new Texture2D(device, desc, rect))
                        {
                            m_srv = new ShaderResourceView(device, texture);
                        }
                        pinnedArray.Free();
                    }
                }
            }
            context.PixelShader.SetShaderResource(0, m_srv);

            if (m_ss == null)
            {
                m_ss = new SamplerState(device, new SamplerStateDescription
                {
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                    AddressW = TextureAddressMode.Wrap,
                });
            }
            context.PixelShader.SetSampler(0, m_ss);

            if (m_rs == null)
            {
                m_rs = new RasterizerState(device, new RasterizerStateDescription
                {
                    CullMode = CullMode.None,
                    FillMode = FillMode.Solid,
                    IsDepthClipEnabled = true,
                });
            }
            context.Rasterizer.State = m_rs;

            mesh.Draw(device, context, m_shader.InputElements.Value);
        }
    }
}
