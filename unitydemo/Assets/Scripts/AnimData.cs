using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace opdemo
{
    [Serializable]
    public class AnimData
    {
        public AnimData()
        {
            isValid = false;
            jointAngles = new List<Vector3>();
            facialParams = new List<float>();
        }

        public static AnimData FromJsonData(string data)
        {
            if (!data.StartsWith("AnimData:"))
            {
                return new AnimData();
            } else
            {
                string text = data.Substring(9); // get rid of "AnimData:"
                try
                {
                    AnimData animData = JsonUtility.FromJson<AnimData>(text);
                    Validate(animData);
                    return animData;
                }
                catch (Exception err)
                {
                    Debug.Log(err.ToString());
                    Debug.Log(text);
                    return new AnimData();
                }
            }
        }
        public bool isValid;
        public Vector3 totalPosition;
        public List<Vector3> jointAngles;
        public List<float> facialParams;
        public float rootHeight;

        public void ResetJointAngles(int size = 62)
        {
            jointAngles = new List<Vector3>(size);
            for (int i = 0; i < size; i++)
            {
                jointAngles.Add(new Vector3());
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

        public static void Validate(AnimData data)
        {
            if (data.jointAngles.Count >= 62)
            {
                data.jointAngles[0] = AxisAngleToUnityEuler(data.jointAngles[0]); // transit angle axis to euler
                for (int i = 1; i < data.jointAngles.Count; i++)
                {
                    data.jointAngles[i] = AdamToUnityEuler(data.jointAngles[i]);
                }
                data.isValid = true;
            } else
            {
                Debug.Log("too short data length");
            }
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
        public AnimDataSet()
        {
            isValid = false;
            frameTime = 0.033333f;
            dataList = new List<AnimData>();
            //default_skeleton = new List<Vector3>();
        }

        public bool isValid;
        public float frameTime;
        public List<AnimData> dataList;
        //public List<Vector3> default_skeleton;
        
        public static AnimDataSet FromJsonData(string text)
        {
            AnimDataSet dataSet = new AnimDataSet();
            try
            {
                dataSet = JsonUtility.FromJson<AnimDataSet>(text);
                foreach (AnimData data in dataSet.dataList)
                {
                    AnimData.Validate(data);
                }
                dataSet.isValid = true;
            }
            catch (Exception err)
            {
                Debug.Log(err);
            }
            return dataSet;
        }

        public static AnimDataSet FromBvhData(string text)
        {
            return BvhPaser.BvhToDataSet(text);
        }
    }
}
