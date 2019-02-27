using System.Collections.Generic;


namespace DXGLTF.Nodes
{
    interface IVisualizer
    {
        bool BuildNode(GltfScene.Source source, UniJSON.JsonPointer p, ShaderLoader shaderLoader, List<Assets.Node> drawables);
    }
}
