using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace opdemo
{
    public class CharacterAnimController : MonoBehaviour
    {
        //[SerializeField] GameObject JointObject;
        public static Vector3 Offset = new Vector3(0f, 1f, 0f);
        public static bool AllowInterpolation = true;
        public static int InsertStepNumber = 2;
        private static float DisplayedFrameRate = 0f;
        private static float LastDisplayedFrameTime = 0f;

        [SerializeField] bool AllowFacialAnim = false;
        [SerializeField] List<Transform> Joints;
        [SerializeField] List<Transform> FacialJoints;

        private Vector3 SavedGlobalPosition;
        private Dictionary<int, Quaternion> InitRotations = new Dictionary<int, Quaternion>();
        private Dictionary<int, Quaternion> SavedRotations = new Dictionary<int, Quaternion>();
        private Dictionary<int, Quaternion> LastRotations = new Dictionary<int, Quaternion>();
        private Dictionary<int, Quaternion> UpdatedRotations = new Dictionary<int, Quaternion>();
        private AnimData frameData;
        private float interpolateFrameRest;

        private Vector3 InitRootPosition;

        void Start()
        {
            if (Joints.Count == 0)
            {
                Debug.Log("No Joints attached");
                return;
            }
            for (int i = 0; i < Joints.Count; i++)
            {
                if (Joints[i] == null) continue;
                InitRotations.Add(i, Joints[i].rotation);
                UpdatedRotations.Add(i, Joints[i].localRotation);
                SavedRotations.Add(i, Joints[i].localRotation);
                LastRotations.Add(i, Joints[i].localRotation);
                //Instantiate(JointObject, Joints[i], false);
            }
            InitRootPosition = Joints[0].position;
        }
        
        private void InitSkeleton(List<Vector3> posList)
        {
            Debug.Log(posList.Count);
            for (int i = 0; i < Mathf.Min(Joints.Count, posList.Count); i++)
            {
                if (Joints[i] == null) continue;
                Joints[i].position = new Vector3(-posList[i].x, posList[i].y, posList[i].z) / 100f;
            }
        }

        public Transform GetCenter()
        {
            return Joints[0];
        }

        public void Recenter()
        {
            Offset -= Joints[0].position - InitRootPosition;
        }

        private void UpdateModel(float interpolatePoint = 1f)
        {
            // calculate frame rate
            //float thisFrameLength = Time.time - LastDisplayedFrameTime;
            //DisplayedFrameRate = 0.75f * DisplayedFrameRate + 0.25f * 1f / thisFrameLength;
            //LastDisplayedFrameTime = Time.time;
            //Debug.Log(DisplayedFrameRate);

            // save data
            SavedGlobalPosition = Joints[0].position;
            for (int i = 0; i < Joints.Count; i++)
            {
                if (Joints[i] == null) continue;
                SavedRotations[i] = Joints[i].localRotation;
            }

            // TODO: Unity and OP are using different coordinates, writing a new script for transiting. 
            // global translation
            Joints[0].position = Vector3.Lerp(SavedGlobalPosition, (-frameData.totalPosition / 100f) + Offset, interpolatePoint);
            //else Joints[0].position = (-frameData.totalPosition / 100f) + Offset;
            // global rotation
            Vector3 axisAngle = new Vector3(-frameData.jointAngles[0].x, frameData.jointAngles[0].y, frameData.jointAngles[0].z);
            Joints[0].rotation = InitRotations[0];
            Joints[0].Rotate(axisAngle, axisAngle.magnitude * Mathf.Rad2Deg, Space.World);
            Joints[0].Rotate(Vector3.left, 180f, Space.World);
            UpdatedRotations[0] = Joints[0].localRotation;
            Joints[0].rotation = InitRotations[0];
            // joints rotations
            for (int i = 1; i < Joints.Count; i++)
            {
                if (Joints[i] == null) continue;
                Joints[i].rotation = InitRotations[i];// * frameData.jointAngleToRotation(i);
                Joints[i].Rotate(Vector3.right, frameData.jointAngles[i].x, Space.World);
                Joints[i].Rotate(Vector3.down, frameData.jointAngles[i].y, Space.World);
                Joints[i].Rotate(Vector3.back, frameData.jointAngles[i].z, Space.World);
                UpdatedRotations[i] = Joints[i].localRotation;
                Joints[i].rotation = InitRotations[i];
            }
            // apply global and joints rotations
            for (int i = 0; i < Joints.Count; i++)
            {
                //if (i < 20 || i > 61) continue;
                if (Joints[i] == null) continue;
                Joints[i].localRotation = Quaternion.Lerp(SavedRotations[i], UpdatedRotations[i], interpolatePoint);
                //else Joints[i].localRotation = UpdatedRotations[i]; // no insert
            }
            //if (UDPReceiver.EstimatedRestFrameTime < 0f) Debug.Log("111111111111"); else Debug.Log(0);
        }

        IEnumerator InsertStepsCoroutine()
        {
            float subAccumulatedTime = 0f;
            for (int i = InsertStepNumber; i > 0; i--)
            {
                // insert a frame
                UpdateModel(1f / i);
                //Debug.Log(1f / i + " and " + Time.time);

                // Loop until the next step is reached
                while (subAccumulatedTime < UDPReceiver.AvgFrameTime / InsertStepNumber)
                {
                    subAccumulatedTime += Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }
                // A step is reached
                subAccumulatedTime -= UDPReceiver.AvgFrameTime / InsertStepNumber;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                Recenter();
            }
            switch (Controller.Mode)
            {
                case PlayMode.Stream:
                    // Update data
                    if (UDPReceiver.IsDataNew())
                    {
                        frameData.Parse(UDPReceiver.ReceivedData);
                        interpolateFrameRest = UDPReceiver.EstimatedRestFrameTime;
                        // new insert coroutine for new data
                        if (!AllowInterpolation)
                        {
                            StopCoroutine(InsertStepsCoroutine());
                            StartCoroutine(InsertStepsCoroutine());
                        }
                    }
                    // Update model every frame for interpolation
                    if (AllowInterpolation)
                    {
                        if (frameData.isValid) UpdateModel(Time.deltaTime / interpolateFrameRest);
                    }
                    break;
                case PlayMode.FileJson:
                    frameData = DataFrameController.GetCurrentFrame();
                    interpolateFrameRest = DataFrameController.RestFrameTime;
                    if (frameData.isValid)
                    {
                        if (AllowInterpolation) UpdateModel(Time.deltaTime / interpolateFrameRest);
                        else UpdateModel();
                    }
                    break;
            }
        }
    }
}
