using System.Collections.Generic;


namespace DXGLTF.nodes
{
    interface IVisualizer
    {
        bool BuildNode(GltfScene.Source source, UniJSON.JsonPointer p, ShaderLoader shaderLoader, List<Node> drawables);
    }
}
