using System;
#if (NET_4_6 || NETFX_CORE)
using System.Threading.Tasks;
#else
using System.Threading;
#endif
using UnityEngine;

namespace TriLib
{
    public class AssetLoaderAsync : AssetLoaderBase
    {
        /// <summary>
        /// Asynchronously loads a <see cref="UnityEngine.GameObject"/> from input filename with defined options.
        /// @warning To ensure your materials will be loaded, don´t remove the material files included in the package.
        /// </summary>
        /// <param name="filename">Filename used to load the <see cref="UnityEngine.GameObject"/>.</param>
        /// <param name="options"><see cref="AssetLoaderOptions"/> used to load the object.</param>
        /// <param name="wrapperGameObject">Use this field to load the new <see cref="UnityEngine.GameObject"/> into referenced <see cref="UnityEngine.GameObject"/>.</param> 
        /// <param name="onAssetLoaded">The action that will be executed when the <see cref="UnityEngine.GameObject"/> be loaded</param>
        /// <returns>The created Thread on NET 2.0, otherwise returns the created Task.</returns>
        /// <example>
        /// @code
        /// protected void Awake() {
        ///     GameObject myGameObject;
        ///     try {
        ///         using (var assetLoader = new AssetLoader()) {
        ///             assetLoader.LoadFromFile("mymodel.fbx", null, null, delegate(GameObject loadedGameObject) {
        ///                 Debug.Log("My object '" + loadedGameObject.name +  "' has been loaded!");
        ///             });
        ///         }
        ///     } catch (Exception e) {
        ///         Debug.LogFormat("Unable to load mymodel.fbx. The loader returned: {0}", e);
        ///     }
        /// }
        /// @endcode
        /// </example>
#if (NET_4_6 || NETFX_CORE)
        public Task LoadFromFile(string filename, AssetLoaderOptions options, GameObject wrapperGameObject,
            Action<GameObject> onAssetLoaded)
#else
        public Thread LoadFromFile(string filename, AssetLoaderOptions options, GameObject wrapperGameObject,
            Action<GameObject> onAssetLoaded)
#endif
        {
            return ThreadUtils.RunThread(delegate
            {
                InternalLoadFromFile(filename, options);
            },
                delegate
                {
                    var basePath = FileUtils.GetFileDirectory(filename);
                    var loadedGameObject = BuildGameObject(options, basePath, wrapperGameObject);
                    if (onAssetLoaded != null)
                    {
                        onAssetLoaded(loadedGameObject);
                    }
                }
            );
        }

        /// <summary>
        /// Asynchronously loads a <see cref="UnityEngine.GameObject"/> from input byte array with defined options.
        /// @warning To ensure your materials will be loaded, don´t remove material files included in the package.
        /// </summary>
        /// <param name="fileBytes">Data used to load the <see cref="UnityEngine.GameObject"/>.</param>
        /// <param name="filename">Original file name, if you know it. Otherwise, use the original file extension instead. (Eg: ".FBX")</param>
        /// <param name="options"><see cref="AssetLoaderOptions"/> used to load the object.</param>
        /// <param name="wrapperGameObject">Use this field to load the new <see cref="UnityEngine.GameObject"/> into referenced <see cref="UnityEngine.GameObject"/>.</param> 
        /// <param name="onAssetLoaded">The action that will be executed when the <see cref="UnityEngine.GameObject"/> be loaded</param>
        /// <returns>The created Thread on NET 2.0, otherwise returns the created Task.</returns>
        /// <example>
        /// @code
        /// protected void Awake() {
        ///     GameObject myGameObject;
        ///     try {
        ///         using (var assetLoader = new AssetLoader()) {
        ///             //In case you don't have a valid filename, set this to the file extension
        ///             //to help TriLib assigining a file loader to this file
        ///             //example value: ".FBX"
        /// 			var filename = "c:/models/mymodel.fbx";
        /// 			var fileData = File.ReadAllBytes(filename);
        ///             assetLoader.LoadFromMemory(fleData, filename, delegate(GameObject loadedGameObject) {
        ///                 Debug.Log("My object '" + loadedGameObject.name +  "' has been loaded!");
        ///             });
        ///         }
        ///     } catch (Exception e) {
        ///         Debug.LogFormat("Unable to load mymodel.fbx. The loader returned: {0}", e);
        ///     }
        /// }
        /// @endcode
        /// </example>
#if (NET_4_6 || NETFX_CORE)
        public Task LoadFromMemory(byte[] fileBytes, string filename, AssetLoaderOptions options,
            GameObject wrapperGameObject, ObjectLoadedHandle onAssetLoaded)
#else
        public Thread LoadFromMemory(byte[] fileBytes, string filename, AssetLoaderOptions options,
            GameObject wrapperGameObject, ObjectLoadedHandle onAssetLoaded)
#endif
        {
            return ThreadUtils.RunThread(delegate
                {
                    InternalLoadFromMemory(fileBytes, filename, options);
                },
                delegate
                {
                    var loadedGameObject = BuildGameObject(options, filename, wrapperGameObject);
                    if (onAssetLoaded != null)
                    {
                        onAssetLoaded(loadedGameObject);
                    }
                }
            );
        }

        /// <summary>
        /// Asynchronously loads the asset from memory processing textures.
        /// </summary>
        /// <param name="fileData">File data.</param>
        /// <param name="assetExtension">Asset extension.</param>
        /// <param name="options"><see cref="AssetLoaderOptions"/> used to load the object.</param>
        /// <param name="wrapperGameObject">Use this field to load the new <see cref="UnityEngine.GameObject"/> into referenced <see cref="UnityEngine.GameObject"/>.</param>
        /// <param name="onAssetLoaded">The action that will be executed when the <see cref="UnityEngine.GameObject"/> be loaded</param>
        /// <returns>The created Thread on NET 2.0, otherwise returns the created Task.</returns>
#if (NET_4_6 || NETFX_CORE)
        public Task LoadFromMemoryWithTextures(byte[] fileData, string assetExtension, AssetLoaderOptions options, GameObject wrapperGameObject, Action<GameObject> onAssetLoaded)
#else
        public Thread LoadFromMemoryWithTextures(byte[] fileData, string assetExtension, AssetLoaderOptions options, GameObject wrapperGameObject, Action<GameObject> onAssetLoaded)
#endif
        {
            return ThreadUtils.RunThread(delegate
                {
                    InternalLoadFromMemoryAndZip(fileData, assetExtension, options);
                },
                delegate
                {
                    var loadedGameObject = BuildGameObject(options, assetExtension, wrapperGameObject);
                    if (onAssetLoaded != null)
                    {
                        onAssetLoaded(loadedGameObject);
                    }
                }
            );
        }
    }
}
