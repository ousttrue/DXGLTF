# DXGLTF

GLTF viewer using SharpDX

![ss](ss.jpg)

## Support format

* GLTF
* GLB
* VRM
* GLTF in zip(experimental not all zip supported)
* GLB in zip(experimental not all zip supported)
* VRM in zip(experimental not all zip supported)

## Dock
* [ ] CameraDock
    * [ ] Auto near far
* [ ] LightDock

## Gizmo
* [x] Axis
* [ ] Grid
* [ ] BoundingBox

## GLTF

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
* [x] TextureLoad

### material
* [x] pbrMetallicRoughness.baseColorFactor
* [x] UnlitShader

### mesh
3D View without transform
* [ ] BlendShape
* [ ] Primitive as submesh
* [ ] Lambert Shader

### skin
* [ ] Joint gizmo

### node
* [x] ModelMatrix
* [ ] Translation Gizmo
* [ ] Rotation Gizmo
* [ ] Scale Gizmo

### scene

## ToDo

* [x] Logger

