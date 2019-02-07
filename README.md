# DXGLTF

GLTF viewer using SharpDX

## Support format

* GLTF
* GLB
* VRM
* GLTF in zip(experimental not all zip supported)
* GLB in zip(experimental not all zip supported)
* VRM in zip(experimental not all zip supported)

## View

### buffer
`byte[]`

### bufferView
`ArraySegments<byte>`

### accessor
`T[]`

### image
Quad

### sampler

### texture
Repeated

### material
* [ ] pbrMetallicRoughness.baseColorFactor

### mesh
3D View without transform
* [ ] BlendShape

### skin
* [ ] Joint gizmo

### node
* [x] ModelMatrix
* [ ] Translation Gizmo
* [ ] Rotation Gizmo
* [ ] Scale Gizmo

### scene
[ ] ToDo

## ToDo

* [ ] CameraDock
* [ ] Primitive as submesh
* [ ] Axis, Grid
* [ ] BoundingBox
* [ ] Embedded nvim
* [ ] LightDock
* [ ] Lambert Shader
* [x] Logger
* [x] UnlitShader
* [x] TextureLoad

