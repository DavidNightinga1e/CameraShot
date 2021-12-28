using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CameraShot.Editor
{
    [CustomEditor(typeof(Camera))]
    public class CameraShotEditor : CameraEditor
    {
        public string FolderPath => $"{Application.persistentDataPath}\\DeveloperScreenshots".Replace("/", "\\");

        private string NewImagePath => $"{FolderPath}\\{DateTime.Now.Ticks}.png";

        private Camera Camera => (Camera) target;

        private Vector2Int CameraRenderSize
        {
            get
            {
                var camera = Camera;
                var cameraWidth = camera.scaledPixelWidth;
                var cameraHeight = Mathf.RoundToInt(cameraWidth / camera.aspect);
                return new Vector2Int(cameraWidth, cameraHeight);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.LabelField("Capture", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Size: {CameraRenderSize.x}, {CameraRenderSize.y}");

            GUILayout.BeginHorizontal();
            var isMakePng = GUILayout.Button("Make PNG", GUILayout.Height(30));
            var isOpenFolder = GUILayout.Button("Show Folder", GUILayout.Height(30));
            GUILayout.EndHorizontal();

            if (isMakePng)
                MakePng();
            if (isOpenFolder)
                OpenFolder();
        }

        private void OpenFolder()
        {
            CheckFolder();
            Process.Start($"explorer.exe", $"{FolderPath}");
        }

        private void MakePng()
        {
            CheckFolder();

            var camera = Camera;
            var activeRt = RenderTexture.active;

            var cameraSize = CameraRenderSize;
            var oldTexture = camera.targetTexture;
            camera.targetTexture = new RenderTexture(cameraSize.x, cameraSize.y, 32);
            RenderTexture.active = camera.targetTexture;

            camera.Render();

            var targetTextureHeight = camera.targetTexture.height;
            var targetTextureWidth = camera.targetTexture.width;
            var texture2D = new Texture2D(targetTextureWidth, targetTextureHeight);
            texture2D.ReadPixels(new Rect(0, 0, targetTextureWidth, targetTextureHeight), 0, 0);
            texture2D.Apply();
            RenderTexture.active = activeRt;
            camera.targetTexture = oldTexture;

            var pngBytes = texture2D.EncodeToPNG();

            CheckFolder();
            using (var fileStream = File.Create(NewImagePath))
            {
                fileStream
                    .WriteAsync(pngBytes, 0, pngBytes.Length)
                    .Wait();
            }

            DestroyImmediate(texture2D, false);
        }

        private void CheckFolder()
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);
        }
    }
}