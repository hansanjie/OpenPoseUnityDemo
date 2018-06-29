using UnityEngine;
using System.IO;
using System;
using System.Threading;
using JetBrains.Annotations;
using Object = UnityEngine.Object;
#if (TRILIB_USE_DEVIL || USE_DEVIL) && !UNITY_WEBGL && !UNITY_IOS
using DevIL;
#endif

namespace TriLib
{
    /// <summary>
    /// Represents a texture compression parameter.
    /// </summary>
    public enum TextureCompression
    {
        /// <summary>
        /// No texture compression will be applied.
        /// </summary>
        None,

        /// <summary>
        /// Normal-quality texture compression will be applied.
        /// </summary>
        NormalQuality,

        /// <summary>
        /// High-quality texture compression will be applied.
        /// </summary>
        HighQuality
    }

    /// <summary>
    /// Represents a <see cref="UnityEngine.Texture2D"/> post-loading event handle.
    /// </summary>
    public delegate void TextureLoadHandle(string sourcePath,Material material,string propertyName,Texture2D texture);

    /// <summary>
    /// Represents a  <see cref="UnityEngine.Texture2D"/> pre-loading event handle.
    /// </summary>
    public delegate void TexturePreLoadHandle(IntPtr scene,string path,string name,Material material,string propertyName,ref bool checkAlphaChannel,TextureWrapMode textureWrapMode = TextureWrapMode.Repeat,string basePath = null,TextureLoadHandle onTextureLoaded = null,TextureCompression textureCompression = TextureCompression.None,bool isNormalMap = false);

    /// <summary>
    /// Represents a class to load external textures.
    /// </summary>
    public static class Texture2DUtils
    {
        public static Texture2D LoadTextureFromFile(
            string path,
            int width,
            int height,
            string name,
            ref bool checkAlphaChannel,
            [CanBeNull] byte[] data,
            bool isRawData = false,
            bool isNormalMap = false,
            string basePath = null,
            TextureWrapMode textureWrapMode = TextureWrapMode.Repeat,
            TextureCompression textureCompression = TextureCompression.None
        )
        {
            var finalPath = path;
            if (data == null)
            {
                string filename = null;
                data = FileUtils.LoadFileData(finalPath);
                if (data.Length == 0 && basePath != null)
                {
                    finalPath = Path.Combine(basePath, path);
                    data = FileUtils.LoadFileData(finalPath);
                }
                if (data.Length == 0)
                {
                    filename = FileUtils.GetFilename(path);
                    finalPath = filename;
                    data = FileUtils.LoadFileData(finalPath);
                }
                if (data.Length == 0 && basePath != null && filename != null)
                {
                    finalPath = Path.Combine(basePath, filename);
                    data = FileUtils.LoadFileData(finalPath);
                }
                if (data.Length == 0)
                {
#if TRILIB_OUTPUT_MESSAGES || ASSIMP_OUTPUT_MESSAGES
                    Debug.LogWarningFormat("Texture '{0}' not found", path);
#endif
                    return null;
                }
            }
            Texture2D tempTexture2D;
            if (ApplyTextureData(data, isRawData, out tempTexture2D, width, height))
            {
                return ProccessTextureData(tempTexture2D, name, ref checkAlphaChannel, textureWrapMode, finalPath, textureCompression, isNormalMap);
            }
#if TRILIB_OUTPUT_MESSAGES || ASSIMP_OUTPUT_MESSAGES
            Debug.LogErrorFormat("Unable to load texture '{0}'", path);
#endif
            return null;
        }

        public static Texture2D LoadTextureFromMemory(byte[] data, string path, ref bool checkAlphaChannel, TextureWrapMode textureWrapMode = TextureWrapMode.Repeat, TextureCompression textureCompression = TextureCompression.None, bool isNormalMap = false, bool isRawData = false, int width = 0, int height = 0)
        {
            if (data.Length == 0 || string.IsNullOrEmpty(path))
            {
                return null;
            }
            Texture2D tempTexture2D;
            if (ApplyTextureData(data, isRawData, out tempTexture2D, width, height))
            {
                return ProccessTextureData(tempTexture2D, StringUtils.GenerateUniqueName(path.GetHashCode()), ref checkAlphaChannel, textureWrapMode, null, textureCompression, isNormalMap);
            }
#if TRILIB_OUTPUT_MESSAGES || ASSIMP_OUTPUT_MESSAGES
            Debug.LogErrorFormat("Unable to load texture '{0}'", path);
#endif
            return null;
        }

        private static bool ApplyTextureData(byte[] data, bool isRawData, out Texture2D tempTexture2D, int width, int height)
        {
            if (data.Length == 0)
            {
                tempTexture2D = null;
                return false;
            }
            if (isRawData)
            {
                try
                {
                    tempTexture2D = new Texture2D(width, height, TextureFormat.ARGB32, true);
                    tempTexture2D.LoadRawTextureData(data);
                    tempTexture2D.Apply();
                    return true;
                }
                catch
                {
#if TRILIB_OUTPUT_MESSAGES || ASSIMP_OUTPUT_MESSAGES
                    Debug.LogError("Invalid embedded texture data");
#endif
                }
            }
#if (TRILIB_USE_DEVIL || USE_DEVIL) && !UNITY_WEBGL && !UNITY_IOS
            return IlLoader.LoadTexture2DFromByteArray(data, data.Length, out tempTexture2D);
#else
            tempTexture2D = new Texture2D(2, 2, TextureFormat.RGBA32, true);
            return tempTexture2D.LoadImage(data);
#endif
        }

        private static Texture2D ProccessTextureData(Texture2D tempTexture2D, string name, ref bool checkAlphaChannel, TextureWrapMode textureWrapMode, string finalPath, TextureCompression textureCompression, bool isNormalMap)
        {
            if (tempTexture2D == null)
            {
                return null;
            }
            tempTexture2D.name = name;
            tempTexture2D.wrapMode = textureWrapMode;
            var colors = tempTexture2D.GetPixels32();
            Texture2D finalTexture2D;
            if (isNormalMap)
            {
#if UNITY_5
                finalTexture2D = new Texture2D(tempTexture2D.width, tempTexture2D.height, TextureFormat.ARGB32, true);
                for (var i = 0; i < colors.Length; i++)
                {
                    var color = colors[i];
                    color.a = color.r;
                    color.r = 0;
                    color.b = 0;
                    colors[i] = color;
                }
                finalTexture2D.SetPixels32(colors);
                finalTexture2D.Apply();
#else
                finalTexture2D = Object.Instantiate(AssetLoaderBase.NormalBaseTexture);
                finalTexture2D.filterMode = tempTexture2D.filterMode;
                finalTexture2D.wrapMode = tempTexture2D.wrapMode;
                finalTexture2D.Resize(tempTexture2D.width, tempTexture2D.height);
                finalTexture2D.SetPixels32(colors);
                finalTexture2D.Apply();
#endif
            }
            else
            {
                finalTexture2D = new Texture2D(tempTexture2D.width, tempTexture2D.height, TextureFormat.ARGB32, true);
                finalTexture2D.SetPixels32(colors);
                finalTexture2D.Apply();
                if (textureCompression != TextureCompression.None)
                {
                    tempTexture2D.Compress(textureCompression == TextureCompression.HighQuality);
                }
            }
            if (checkAlphaChannel)
            {
                checkAlphaChannel = false;
                foreach (var color in colors)
                {
                    if (color.a == 255) continue;
                    checkAlphaChannel = true;
                    break;
                }
            }
            return finalTexture2D;
        }
    }
}

