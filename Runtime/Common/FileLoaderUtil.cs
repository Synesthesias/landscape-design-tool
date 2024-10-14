using System.IO;
using UnityEngine;

namespace Landscape2.Runtime.Common
{
    public static class FileLoaderUtil
    {
        private static Texture2D tmpTexture;
        public static Texture2D LoadTexture(string path)
        {
            tmpTexture ??= new Texture2D(2, 2);
            var bytes = File.ReadAllBytes(path);
            tmpTexture.LoadImage(bytes);
            tmpTexture.Apply();
            return tmpTexture;
        }
    }
}