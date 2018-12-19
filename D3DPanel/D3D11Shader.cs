using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System.Linq;


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

        const RegisterComponentMaskFlags MASK_XYZ = 
            RegisterComponentMaskFlags.ComponentX 
            | RegisterComponentMaskFlags.ComponentY 
            | RegisterComponentMaskFlags.ComponentZ
            ;
        const RegisterComponentMaskFlags MASK_XY =
            RegisterComponentMaskFlags.ComponentX
            | RegisterComponentMaskFlags.ComponentY
            ;

        static SharpDX.DXGI.Format GetFormat(RegisterComponentType type, RegisterComponentMaskFlags mask)
        {
            if (mask.HasFlag(RegisterComponentMaskFlags.All))
            {
                switch (type)
                {
                    case RegisterComponentType.Float32:
                        return SharpDX.DXGI.Format.R32G32B32A32_Float;
                }
                throw new System.NotImplementedException();
            }
            else if (mask.HasFlag(MASK_XYZ))
            {
                switch (type)
                {
                    case RegisterComponentType.Float32:
                        return SharpDX.DXGI.Format.R32G32B32_Float;
                }
                throw new System.NotImplementedException();
            }
            else if (mask.HasFlag(MASK_XY))
            {
                switch (type)
                {
                    case RegisterComponentType.Float32:
                        return SharpDX.DXGI.Format.R32G32_Float;
                }
                throw new System.NotImplementedException();
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        static InputElement ToInputElement(ShaderParameterDescription desc)
        {
            return new InputElement(desc.SemanticName, desc.SemanticIndex,
                GetFormat(desc.ComponentType, desc.UsageMask), 0);
        }

        public void SetShader(string vs, string ps)
        {
            Dispose();
            if(string.IsNullOrEmpty(vs) || string.IsNullOrEmpty(ps))
            {
                return;
            }

            m_vsCompiled = ShaderBytecode.Compile(vs, "VS", "vs_4_0", ShaderFlags.None, EffectFlags.None);
            m_psCompiled = ShaderBytecode.Compile(ps, "PS", "ps_4_0", ShaderFlags.None, EffectFlags.None);

            var reflection = new ShaderReflection(m_vsCompiled.Bytecode);
            InputElements = Enumerable.Range(0, reflection.Description.InputParameters)
                .Select(x => reflection.GetInputParameterDescription(x))
                .Select(x => ToInputElement(x))
                .ToArray()
                ;
        }

        public void SetupContext(Device device, DeviceContext context)
        {
            if (m_vsCompiled == null)
            {
                return;
            }

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
