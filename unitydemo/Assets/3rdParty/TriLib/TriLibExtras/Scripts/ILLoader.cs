#if (TRILIB_USE_DEVIL || USE_DEVIL) && !UNITY_WEBGL && !UNITY_IOS
using System.Runtime.InteropServices;
using UnityEngine;
namespace DevIL
    {
        /// <summary>
        /// Class used to Parse Image data with DevIL Interop.
        /// </summary>
        public static class IlLoader
        {
            /// <summary>
            /// Loads the image data from Byte input into Texture2D.
            /// </summary>
            /// <param name="bytes">Image data.</param>
            /// <param name="length">Image data length.</param>
            /// <param name="texture2D">The <see cref="UnityEngine.Texture2D"/> to draw the image into.</param>
            /// <returns>True if succeed. Otherwise, false.</returns>
            public static bool LoadTexture2DFromByteArray(byte[] bytes, int length, out Texture2D texture2D)
            {
                texture2D = null;
                var loaded = false;
                try
                {
                    IlInterop.ilInit();
                    IlInterop.ilEnable(IlInterop.IL_ORIGIN_SET);
                    IlInterop.ilOriginFunc(IlInterop.IL_ORIGIN_LOWER_LEFT);
                    var image = IlInterop.ilGenImage();
                    IlInterop.ilBindImage(image);
                    if (IlInterop.ilLoadL(IlInterop.IL_TYPE_UNKNOWN, bytes, length))
                    {
                        IlInterop.ilConvertImage(IlInterop.IL_RGBA, IlInterop.IL_UNSIGNED_BYTE);
                        var imageWidth = IlInterop.ilGetInteger(IlInterop.IL_IMAGE_WIDTH);
                        var imageHeight = IlInterop.ilGetInteger(IlInterop.IL_IMAGE_HEIGHT);
                        var dataLength = imageWidth*imageHeight*4;
                        var intPtr = IlInterop.ilGetData();
                        var data = new byte[dataLength];
                        Marshal.Copy(intPtr, data, 0, dataLength);
                        texture2D = new Texture2D(imageWidth, imageHeight, TextureFormat.RGBA32, false);
                        texture2D.LoadRawTextureData(data);
                        texture2D.Apply();
                        loaded = true;
                    }
                    IlInterop.ilDeleteImages(1, ref image);
                }
                catch
                {
                }                          
                return loaded;
            }
        }
    }
#endif