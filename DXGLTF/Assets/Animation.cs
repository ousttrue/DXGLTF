using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DXGLTF.Assets
{
    public enum AnimationTarget
    {
        Translation,
        Rotation,
        Scale,
    }

    public enum AnimationInterpolation
    {
        Linear,
    }

    public struct KeyFrame<T> where T: struct
    {
        public readonly float Seconds;
        public readonly T Value;

        public KeyFrame(float seconds, T value)
        {
            Seconds = seconds;
            Value = value;
        }
    }

    public class NodeAnimation
    {
        static AnimationInterpolation ParseInterpolation(string src)
        {
            return (AnimationInterpolation)Enum.Parse(typeof(AnimationInterpolation), src, true);
        }

        List<KeyFrame<Vector3>> Translation;
        List<KeyFrame<Quaternion>> Rotation;
        List<KeyFrame<Vector3>> Scale;

        public void Animate(TimeSpan time, Node node)
        {
            var seconds = (float)time.TotalSeconds;
            var scale = Vector3.One;
            var rotation = Quaternion.Identity;
            var translation = Vector3.Zero;

            if (Translation != null && Translation.Any())
            {
                if(Translation.Count==1 || seconds <= Translation[0].Seconds)
                {
                    translation = Translation[0].Value;
                }
                else if (seconds >= Translation.Last().Seconds)
                {
                    translation = Translation.Last().Value;
                }
                else
                {
                    int i = 1;
                    for (; i < Translation.Count; ++i)
                    {
                        if (Translation[i].Seconds >= seconds)
                        {
                            break;
                        }
                    }
                    // ToDo: Linear
                    translation = Translation[i].Value;
                }
            }

            if (Rotation != null && Rotation.Any())
            {
                if (Rotation.Count == 1 || seconds <= Rotation[0].Seconds)
                {
                    rotation = Rotation[0].Value;
                }
                else if (seconds >= Rotation.Last().Seconds)
                {
                    rotation = Rotation.Last().Value;
                }
                else
                {
                    int i = 1;
                    for (; i < Rotation.Count; ++i)
                    {
                        if (Rotation[i].Seconds >= seconds)
                        {
                            break;
                        }
                    }
                    // ToDo: Linear
                    rotation = Rotation[i].Value;
                }
            }

            if (Scale != null && Scale.Any())
            {
                if (Scale.Count == 1 || seconds <= Scale[0].Seconds)
                {
                    scale = Scale[0].Value;
                }
                else if (seconds >= Scale.Last().Seconds)
                {
                    scale = Scale.Last().Value;
                }
                else
                {
                    int i = 1;
                    for (; i < Scale.Count; ++i)
                    {
                        if (Scale[i].Seconds >= seconds)
                        {
                            break;
                        }
                    }
                    // ToDo: Linear
                    scale = Scale[i].Value;
                }
            }

            node.LocalMatrix = Matrix.Transformation(Vector3.Zero, Quaternion.Identity, scale,
                Vector3.Zero, rotation,
                translation);
        }

        public float LastSeconds
        {
            get;
            private set;
        }

        void Add<T>(ref List<KeyFrame<T>> target,  AssetSource source, UniGLTF.glTFAnimationSampler sampler)where T: struct
        {
            var interpolation = ParseInterpolation(sampler.interpolation);
            var input = source.GLTF.GetArrayFromAccessor<float>(source.IO, sampler.input);
            var output = source.GLTF.GetArrayFromAccessor<T>(source.IO, sampler.output);
            target = new List<KeyFrame<T>>(input.Length);
            for (int i = 0; i < input.Length; ++i)
            {
                LastSeconds = Math.Max(LastSeconds, input[i]);
                target.Add(new KeyFrame<T>(input[i], output[i]));
            }
        }

        public void Add(AnimationTarget target, AssetSource source, UniGLTF.glTFAnimationSampler sampler)
        {
            switch (target)
            {
                case AnimationTarget.Translation:
                    Add(ref Translation, source, sampler);
                    break;

                case AnimationTarget.Rotation:
                    Add(ref Rotation, source, sampler);
                    break;

                case AnimationTarget.Scale:
                    Add(ref Scale, source, sampler);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class Animation
    {
        Dictionary<int, NodeAnimation> _nodeMap = new Dictionary<int, NodeAnimation>();

        public float Length
        {
            get;
            private set;
        }

        static AnimationTarget ParseChannelTargetPath(string path)
        {
            return (AnimationTarget)Enum.Parse(typeof(AnimationTarget), path, true);
        }

        public static Animation FromGLTF(AssetSource source, UniGLTF.glTFAnimation src)
        {
            var animation = new Animation();

            foreach(var channel in src.channels)
            {
                NodeAnimation nodeAnimation;
                if (!animation._nodeMap.TryGetValue(channel.target.node, out nodeAnimation))
                {
                    nodeAnimation = new NodeAnimation();
                    animation._nodeMap.Add(channel.target.node, nodeAnimation);
                }

                var target = ParseChannelTargetPath(channel.target.path);
                var sampler = src.samplers[channel.sampler];
                nodeAnimation.Add(target, source, sampler);

                animation.Length = Math.Max(animation.Length, nodeAnimation.LastSeconds);
            }

            return animation;
        }

        public void Animate(TimeSpan time, Node[] nodes)
        {
            foreach(var kv in _nodeMap)
            {
                var target = nodes[kv.Key];
                kv.Value.Animate(time, target);
            }
        }
    }
}
