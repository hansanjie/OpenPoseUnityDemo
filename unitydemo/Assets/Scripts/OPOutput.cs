using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace opdemo.dll
{
    [Serializable]
    public class OPFrame
    {
        public List<OPUnit> units = new List<OPUnit>();

        public static OPFrame FromJson(string json)
        {
            try
            {
                return JsonUtility.FromJson<OPFrame>(json);
            } catch(Exception err)
            {
                Debug.LogError(err);
                Debug.Log(json);
                return null;
            }
        }
    }

    [Serializable]
    public class OPUnit
    {
        public List<Vector3> poseKeypoints = new List<Vector3>();
        public List<Vector3> handKeypoints_L = new List<Vector3>();
        public List<Vector3> handKeypoints_R = new List<Vector3>();
        public List<Vector3> faceKeypoints = new List<Vector3>();
    }

    [Serializable]
    public class OPImage
    {
        [SerializeField] Vector2Int size;
        [SerializeField] List<Vector3Int> pixels = new List<Vector3Int>();

        public int w { get { return size.x; } }
        public int h { get { return size.y; } }

        public Color GetColorAt(int x, int y)
        {
            if (x >= w || y >= h) return Color.black;

            Debug.Assert(pixels.Count == w * h);
            Vector3Int res = pixels[x * w + y];
            return new Color(res.x / 255f, res.y / 255f, res.z / 255f);
        }

        public static OPImage FromJson(string json)
        {
            try
            {
                return JsonUtility.FromJson<OPImage>(json);
            }
            catch (Exception err)
            {
                Debug.LogError(err);
                Debug.Log(json);
                return null;
            }
        }
    }
}
