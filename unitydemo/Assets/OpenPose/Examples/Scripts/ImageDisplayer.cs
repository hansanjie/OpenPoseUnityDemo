using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace opdemo.examples
{
    using opdemo.dll;
    public class ImageDisplayer : MonoBehaviour {

        [SerializeField] Material ImageMaterial;

        private Texture2D texture;
        private OPImage imageData;
        private bool newDataFlag = false;

        public void PushNewImageData(OPImage imageData)
        {
            if (imageData != null)
            {
                this.imageData = imageData;
                newDataFlag = true;
            }
        }

        private void Paint()
        {
            Debug.Log("paint");
            if (texture.width != imageData.w || texture.height != imageData.h)
            {
                texture = new Texture2D(imageData.h, imageData.w);
                ImageMaterial.mainTexture = texture;
            }
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    //Color color = ((x & y) != 0 ? Color.white : Color.gray);
                    texture.SetPixel(x, y, imageData.GetColorAt(x, y));
                }
            }
            texture.Apply();
        }

	    // Use this for initialization
	    void Start () {
            texture = new Texture2D(400, 400);
            ImageMaterial.mainTexture = texture;
        }
	
	    // Update is called once per frame
	    void Update () {
            if (newDataFlag)
            {
                Paint();
                newDataFlag = false;
            }
        }
    }
}
