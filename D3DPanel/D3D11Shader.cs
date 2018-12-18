using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;


namespace D3DPanel
{
    public class D3D11Shader : System.IDisposable
    {
        CompilationResult m_vsCompiled;
        CompilationResult m_psCompiled;

        InputLayout m_layout;
        VertexShader m_vs;
        PixelShader m_ps;

        public InputElement[] InputElements
        {
            get;
            private set;
        }

        public void Dispose()
        {
            if (m_layout != null)
            {
                m_layout.Dispose();
                m_layout = null;
            }

            if (m_vs != null)
            {
                m_vs.Dispose();
                m_vs = null;
            }

            if (m_ps != null)
            {
                m_ps.Dispose();
                m_ps = null;
            }
        }

        public D3D11Shader(string vs, string ps)
        {
            m_vsCompiled = ShaderBytecode.Compile(vs, "VS", "vs_4_0", ShaderFlags.None, EffectFlags.None);
            m_psCompiled = ShaderBytecode.Compile(ps, "PS", "ps_4_0", ShaderFlags.None, EffectFlags.None);

            // ToDo: Create from vs
            InputElements = new[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0),
                new InputElement("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 12, 0),
                new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 24, 0),
                //new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 16, 0)
            };
        }

        public void SetupContext(Device device, DeviceContext context)
        {
            // Prepare All the stages
            context.InputAssembler.InputLayout = m_layout;

            if (m_vs == null)
            {
                m_vs = new VertexShader(device, m_vsCompiled);
            }
            context.VertexShader.Set(m_vs);

            if (m_ps == null)
            {
                m_ps = new PixelShader(device, m_psCompiled);
            }
            context.PixelShader.Set(m_ps);

            if (m_layout == null)
            {
                // Layout from VertexShader input signature
                m_layout = new InputLayout(
                    device,
                    ShaderSignature.GetInputSignature(m_vsCompiled),
                    InputElements);
            }
            context.InputAssembler.InputLayout = m_layout;
        }
    }
}
