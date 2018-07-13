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
                Debug.Log(err);
                return null;
            }
        }
    }

    [Serializable]
    public class OPUnit
    {
        public List<Vector3> poseKeypoints = new List<Vector3>();
    }
}
