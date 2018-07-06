using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace opdemo
{
    [Serializable]
    public class AnimData
    {
        public List<AnimUnitData> units = new List<AnimUnitData>();

        //public bool isValid = false;
        public float receivedTime = -1; // used by StreamFrameController

        public static AnimData FromJsonData(string dataString)
        {
            if (!dataString.StartsWith("AnimData:"))
            {
                return new AnimData();
            }
            else
            {
                string text = dataString.Substring("AnimData:".Length); // get rid of "AnimData:"
                try
                {
                    AnimData data = JsonUtility.FromJson<AnimData>(text);
                    foreach (AnimUnitData unit in data.units)
                    {
                        unit.JsonInputToUnitySystem();
                        unit.isValid = true;
                    }
                    //data.isValid = true;
                    return data;
                }
                catch (Exception err)
                {
                    Debug.LogError(err.ToString());
                    Debug.Log(text);
                    return new AnimData();
                }
            }
        }
    }

    [Serializable]
    public class AnimUnitData
    {
        public bool isValid = false;
        public int id;
        public float size = 1f;
        public Vector3 totalPosition;
        public List<Vector3> jointAngles = new List<Vector3>();
        public List<float> facialParams = new List<float>();

        public void ResetJointAngles(int size = 62)
        {
            jointAngles = new List<Vector3>(size);
            for (int i = 0; i < size; i++)
            {
                jointAngles.Add(new Vector3());
            }
        }

        public void JsonInputToUnitySystem()
        {
            if (jointAngles.Count >= 62)
            {
                jointAngles[0] = AxisAngleToUnityEuler(jointAngles[0]); // jointAngles[0] is in angle axis
                for (int i = 1; i < jointAngles.Count; i++)
                {
                    jointAngles[i] = AdamToUnityEuler(jointAngles[i]);
                }
            } else
            {
                Debug.Log("too short data length");
            }
        }

        public void BvhInputToUnitySystem()
        {
            if (jointAngles.Count >= 62)
            {
                for (int i = 0; i < jointAngles.Count; i++)
                {
                    jointAngles[i] = AdamToUnityEuler(jointAngles[i]);
                }
            }
            else
            {
                Debug.Log("too short data length");
            }
        }

        public Quaternion jointAngleToRotation(int index) // deprecated
        {
            if (jointAngles == null)
            {
                Debug.Log("Data not initialized yet");
                return Quaternion.identity;
            }
            if (index >= jointAngles.Count)
            {
                Debug.Log("Joint index " + index + " larger than expected " + jointAngles.Count);
                return Quaternion.identity;
            }
            return ToRotation(jointAngles[index]);
        }

        public static Quaternion ToRotation(Vector3 angle) // deprecated
        {
            return Quaternion.AngleAxis(angle.x, Vector3.right)
                * Quaternion.AngleAxis(angle.y, Vector3.down)
                * Quaternion.AngleAxis(angle.z, Vector3.back);
        }

        public static Vector3 AdamToUnityEuler(Vector3 angle)
        {
            return (Quaternion.AngleAxis(angle.z, Vector3.back)
                * Quaternion.AngleAxis(angle.y, Vector3.down)
                * Quaternion.AngleAxis(angle.x, Vector3.right)).eulerAngles;
        }

        public static Vector3 AxisAngleToUnityEuler(Vector3 axisAngle)
        {
            Vector3 adamAxisInUnityOrder = new Vector3(axisAngle.y, axisAngle.z, -axisAngle.x);
            Vector3 adamAngleInUnityOrder = Quaternion.AngleAxis(adamAxisInUnityOrder.magnitude * Mathf.Rad2Deg, adamAxisInUnityOrder).eulerAngles;
            Vector3 adamEuler = new Vector3(adamAngleInUnityOrder.z, -adamAngleInUnityOrder.x, -adamAngleInUnityOrder.y);
            Vector3 euler = AdamToUnityEuler(adamEuler);

            return euler;
        }
    }

    [Serializable]
    public class AnimDataSet
    {
        //public bool isValid = false;
        public float frameTime = 0.033333f;
        public List<AnimData> dataList = new List<AnimData>();

        public static AnimDataSet FromJsonData(string text) // Json file mode
        {
            AnimDataSet dataSet = new AnimDataSet();
            try
            {
                dataSet = JsonUtility.FromJson<AnimDataSet>(text);
                foreach (AnimData data in dataSet.dataList)
                {
                    foreach (AnimUnitData unit in data.units)
                    {
                        unit.JsonInputToUnitySystem();
                        unit.isValid = true;
                    }
                    data.isValid = true;
                }
                //dataSet.isValid = true;
            }
            catch (Exception err)
            {
                Debug.Log(err);
            }
            return dataSet;
        }

        public static AnimDataSet FromBvhData(string text) // Bvh file mode
        {
            return BvhPaser.BvhToDataSet(text);
        }
    }
}
