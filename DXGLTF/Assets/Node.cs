﻿using D3DPanel;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DXGLTF.Assets
{
    public class Node : IDisposable
    {
        public bool IsValid => true;

        public Node Parent
        {
            get;
            private set;
        }
        List<Node> _children = new List<Node>();
        public IEnumerable<Node> Children
        {
            get
            {
                return _children;
            }
        }
        public void AddChild(Node child)
        {
            _children.Add(child);
            child.Parent = this;
        }

        public Mesh Mesh
        {
            get;
            set;
        }

        public void Dispose()
        {
            if (Mesh != null)
            {
                Mesh.Dispose();
                Mesh = null;
            }

            foreach (var x in _children)
            {
                x.Dispose();
            }
            _children.Clear();
        }

        Matrix _matrix = Matrix.Identity;
        public Matrix LocalMatrix
        {
            get { return _matrix; }
            set
            {
                _matrix = value;
            }
        }

        public Matrix WorldMatrix
        {
            get;
            set;
        }

        public int Index
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return base.ToString();
            }
            else
            {
                return $"{Name}";
            }
        }

        public Node(string name, int index = -1)
        {
            Name = name;
        }

        public Node(D3D11Shader shader, D3D11Mesh mesh):this(new D3D11Material(shader), mesh)
        {
        }

        public Node(D3D11Material material, D3D11Mesh mesh)
        {
            Mesh = new Mesh(new Submesh(material, mesh));
        }

        public IEnumerable<SubmeshIntersection> Intersect(Ray ray)
        {
            if (Mesh == null)
            {
                return Enumerable.Empty<SubmeshIntersection>();
            }

            return Mesh.Intersect(WorldMatrix, ray);
        }
    }
}