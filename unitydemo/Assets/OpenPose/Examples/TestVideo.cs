using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestVideo : MonoBehaviour {
    [SerializeField] Material mat;
    Texture2D texture;

	// Use this for initialization
	void Start () {
        texture = new Texture2D(400, 400);
        mat.mainTexture = texture;
	}
	
	// Update is called once per frame
	void Update () {
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                Color color = ((x & y) != 0 ? Color.white : Color.gray);
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
    }
}
