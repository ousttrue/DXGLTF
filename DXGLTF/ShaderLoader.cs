﻿using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace DXGLTF
{
    enum ShaderType
    {
        Gizmo,
        Unlit,
        Standard,
    }

    class SourceWatcher
    {
        FileSystemWatcher m_watcher;
        public SourceWatcher(string dir, string file)
        {
            m_watcher = new FileSystemWatcher();
            m_watcher.Path = dir;
            m_watcher.NotifyFilter = System.IO.NotifyFilters.LastWrite;
            m_watcher.Filter = file;
            m_watcher.Changed += Watcher_Changed;
            m_watcher.EnableRaisingEvents = true;

            var path = Path.Combine(dir, file);
            if (File.Exists(path))
            {
                Source.Value = File.ReadAllText(path, Encoding.UTF8);
            }
        }

        private async void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            await Task.Delay(100);
            Source.Value = File.ReadAllText(e.FullPath, Encoding.UTF8);
        }

        public ReactiveProperty<string> Source = new ReactiveProperty<string>();
    }

    class ShaderLoader
    {
        Dictionary<ShaderType, SourceWatcher> m_map = new Dictionary<ShaderType, SourceWatcher>();
        public ShaderLoader()
        {
            var shaderDir = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "shaders");
            m_map.Add(ShaderType.Unlit, new SourceWatcher(shaderDir, "unlit.hlsl"));
            m_map.Add(ShaderType.Gizmo, new SourceWatcher(shaderDir, "gizmo.hlsl"));
        }

        public IObservable<string> GetShaderSource(ShaderType type)
        {
            return m_map[type].Source;
        }
    }
}