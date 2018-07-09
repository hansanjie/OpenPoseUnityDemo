using System.Collections;
using System.Collections.Generic;
using System.IO;
using TriLib;
using UnityEngine;

namespace opdemo
{
    public class FbxLoader : MonoBehaviour
    {
        [SerializeField] RuntimeAnimatorController AnimatorController;
        [SerializeField] AnimatorOverrideController OverrideController;

        protected void Start()
        {
            using (var assetLoader = new AssetLoader())
            { //Initializes our Asset Loader.
                var assetLoaderOptions = ScriptableObject.CreateInstance<AssetLoaderOptions>(); //Creates an Asset Loader Options object.
                //assetLoaderOptions.AutoPlayAnimations = true; //Indicates that our model will automatically play its first animation, if any.
                //assetLoaderOptions.UseLegacyAnimations = false;
                //assetLoaderOptions.AnimatorController = AnimatorController;
                var filename = Path.Combine(Path.GetFullPath("."), "test.fbx"); //Combines our current directory with our model filename "turtle1.b3d" and generates the full model path.
                //Debug.Log(filename);
            //    GameObject model = assetLoader.LoadFromFile(filename, assetLoaderOptions); //Loads our model.
                //Debug.Log(model.GetComponent<Animation>().clip);
                //model.GetComponent<Animation>().clip.legacy = false;
                //OverrideController["HumanoidIdle"] = model.GetComponent<Animation>().clip;
            }
        }
    }
}
