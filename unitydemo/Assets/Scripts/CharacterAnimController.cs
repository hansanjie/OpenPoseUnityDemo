using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace opdemo
{
    public class CharacterAnimController : MonoBehaviour
    {
        // Global settings
        //[SerializeField] GameObject JointObject;
        public static Vector3 Offset = new Vector3(0f, 1f, 2f);
        public static bool AllowInterpolation = false;
        public static int InsertStepNumber = 2;
        public static bool AllowVerticalStablization = true;
        public static int CompareDifference = 5;
        public static float VerticalMovementThres = 0.02f;
        //private static float DisplayedFrameRate = 0f;
        //private static float LastDisplayedFrameTime = 0f;

        // Model settings
        [SerializeField] bool AllowFacialAnim = true;
        [SerializeField] List<Transform> Joints;
        [SerializeField] List<Transform> FacialJoints;
        [SerializeField] List<Transform> LowerFeetIndicators;
        [SerializeField] List<Quaternion> FacialInitRotations;
        [SerializeField] List<Quaternion> FacialFullRotations;

        // Animating data
        private AnimData frameData = new AnimData();
        private Vector3 InitRootPosition;
        private Vector3 SavedRootPosition;
        private Vector3 NextRootPosition;
        private Dictionary<int, Quaternion> InitRotations = new Dictionary<int, Quaternion>();
        private Dictionary<int, Quaternion> SavedRotations = new Dictionary<int, Quaternion>();
        private Dictionary<int, Quaternion> NextRotations = new Dictionary<int, Quaternion>();
        private float interpolateFrameRest;

        // Vertical stablization
        private float HeightDiff;
        private Queue<float> LastFeetHeights = new Queue<float>();
        private float GetLowestFeetHeight()
        {
            float minY = float.PositiveInfinity;
            foreach (Transform t in LowerFeetIndicators)
            {
                if (t.position.y < minY) minY = t.position.y;
            }
            return minY;
        }
        private void PushNewFeetHeights()
        {
            if (LowerFeetIndicators.Count <= 0) return;
            LastFeetHeights.Enqueue(GetLowestFeetHeight());
            while (LastFeetHeights.Count > CompareDifference + 1)
            {
                LastFeetHeights.Dequeue();
            }
        }
        private bool IsVerticalStable()
        {
            if (LastFeetHeights.Count <= CompareDifference)
            {
                return true;
            }
            for(int i = 0; i < CompareDifference; i++)
            {
                float[] heightArray = LastFeetHeights.ToArray();
                if (Mathf.Abs(heightArray[i] - heightArray[i + 1]) > VerticalMovementThres)
                {
                    return false;
                }
            }
            return true;
        }

        //private Vector3 HeightDifference;

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
                SavedRotations.Add(i, Joints[i].localRotation);
                NextRotations.Add(i, Joints[i].localRotation);
                //Instantiate(JointObject, Joints[i], false);
            }
            InitRootPosition = Joints[0].position;
        }
        
        private void InitSkeleton(List<Vector3> posList) // deprecated
        {
            Debug.Log(posList.Count);
            for (int i = 0; i < Mathf.Min(Joints.Count, posList.Count); i++)
            {
                if (Joints[i] == null) continue;
                Joints[i].position = new Vector3(-posList[i].x, posList[i].y, posList[i].z) / 100f;
            }
        }

        public Transform GetFocusCenter(CamFocusPart focus)
        {
            switch (focus)
            {
                case CamFocusPart.Hip:
                    return Joints[0];
                case CamFocusPart.Chest:
                    return Joints[6];
                case CamFocusPart.Head:
                    return Joints[12];
                default:
                    return Joints[0];
            }
        }

        public void Recenter()
        {
            Offset -= Joints[0].position - InitRootPosition;
        }

        /*public void AdjustHeight()
        {
            //float humanRootHeight = frameData.rootHeight / 100f;
            //float modelRootHeight = InitRootPosition.y;
            HeightDifference.y = InitRootPosition.y - frameData.rootHeight / 100f;
        }*/

        [ExecuteInEditMode]
        public void ConfigureInitFacialRotations()
        {
            FacialInitRotations = new List<Quaternion>();
            foreach(Transform j in FacialJoints)
            {
                Quaternion q = new Quaternion();
                if (j != null)
                {
                    q = j.localRotation;
                }
                FacialInitRotations.Add(q);
            }
        }

        [ExecuteInEditMode]
        public void ShowInitFacialRotations()
        {
            if (FacialInitRotations.Count < FacialJoints.Count)
            {
                Debug.LogError("Please configure first");
                return;
            }
            for (int i = 0; i < FacialJoints.Count; i++)
            {
                if (FacialJoints[i] != null)
                {
                    FacialJoints[i].localRotation = FacialInitRotations[i];
                }
            }
        }

        [ExecuteInEditMode]
        public void ConfigureFullFacialRotations()
        {
            FacialFullRotations = new List<Quaternion>();
            foreach (Transform j in FacialJoints)
            {
                Quaternion q = new Quaternion();
                if (j != null)
                {
                    q = j.localRotation;
                }
                FacialFullRotations.Add(q);
            }
        }

        [ExecuteInEditMode]
        public void ShowFullFacialRotations()
        {
            if (FacialFullRotations.Count < FacialJoints.Count)
            {
                Debug.LogError("Please configure first");
                return;
            }
            for (int i = 0; i < FacialJoints.Count; i++)
            {
                if (FacialJoints[i] != null)
                {
                    FacialJoints[i].localRotation = FacialFullRotations[i];
                }
            }
        }

        private void ChangeModelToLastSavedState()
        {
            Joints[0].position = SavedRootPosition;
            for (int i = 0; i < Joints.Count; i++)
            {
                if (Joints[i] == null) continue;
                Joints[i].localRotation = SavedRotations[i];
            }
        }

        private void UpdateModel(float interpolatePoint = 1f, bool forceDisableVerticalStablizing = false)
        {            
            // TODO: Unity and OP are using different coordinates, write a new script for transiting. 

            // global translation
            SavedRootPosition = Joints[0].position;
            NextRootPosition = -frameData.totalPosition / 100f;
            // Vertical stablizer
            if (AllowVerticalStablization && LowerFeetIndicators.Count > 0)
            {
                if (!forceDisableVerticalStablizing)
                {
                    //Joints[0].Translate(0f, HeightDiff, 0f, Space.World);
                    NextRootPosition.y += HeightDiff;
                }
            }
            Joints[0].position = Vector3.Lerp(SavedRootPosition, NextRootPosition + Offset, interpolatePoint);
            // global rotation
            /*Vector3 axisAngle = new Vector3(-frameData.jointAngles[0].x, frameData.jointAngles[0].y, frameData.jointAngles[0].z);
            Joints[0].rotation = InitRotations[0];
            Joints[0].Rotate(axisAngle, axisAngle.magnitude * Mathf.Rad2Deg, Space.World);
            Joints[0].Rotate(Vector3.left, 180f, Space.World);
            UpdatedRotations[0] = Joints[0].localRotation;
            Joints[0].rotation = InitRotations[0];*/
            // global rotation & joints rotations
            for (int i = 0; i < Joints.Count; i++)
            {
                if (Joints[i] == null) continue;
                SavedRotations[i] = Joints[i].localRotation;
            }
            // Root rotation
            Joints[0].rotation = InitRotations[0];
            Joints[0].Rotate(frameData.jointAngles[0], Space.World);
            NextRotations[0] = Joints[0].localRotation;
            Joints[0].rotation = InitRotations[0];
            // Other rotation
            for (int i = 1; i < Joints.Count; i++)
            {
                if (Joints[i] == null) continue;
                Joints[i].rotation = InitRotations[i];// * frameData.jointAngleToRotation(i);
                Joints[i].Rotate(AnimData.ToUnityAngles(frameData.jointAngles[i]), Space.World);
                //Joints[i].Rotate(Vector3.right, frameData.jointAngles[i].x, Space.World);
                //Joints[i].Rotate(Vector3.down, frameData.jointAngles[i].y, Space.World);
                //Joints[i].Rotate(Vector3.back, frameData.jointAngles[i].z, Space.World);
                NextRotations[i] = Joints[i].localRotation;
                Joints[i].rotation = InitRotations[i];
            }
            //UpdatedRotations[0] = Quaternion.Euler(180f, 0f, 0f) * UpdatedRotations[0]; // upside down?
            // apply global and joints rotations
            for (int i = 0; i < Joints.Count; i++)
            {
                //if (i < 20 || i > 61) continue;
                if (Joints[i] == null) continue;
                Joints[i].localRotation = Quaternion.Lerp(SavedRotations[i], NextRotations[i], interpolatePoint);
            }                
            // Facial expression
            UpdateFace(interpolatePoint);
        }

        private void UpdateFace(float interpolatePoint = 1f)
        {
            if (AllowFacialAnim)
            {
                for (int i = 0; i < 1; i++)
                {
                    if (FacialJoints[i] != null)
                    {
                        Quaternion goalRotation = Quaternion.Lerp(FacialInitRotations[i], FacialFullRotations[i], frameData.facialParams[i]);
                        FacialJoints[i].localRotation = Quaternion.Lerp(FacialJoints[i].localRotation, goalRotation, interpolatePoint);
                    }
                }
                for (int i = 1; i < FacialJoints.Count; i++)
                {
                    if (FacialJoints[i] != null)
                    {

                        Quaternion goalRotation = Quaternion.Lerp(FacialInitRotations[i], FacialFullRotations[i], 1f - frameData.facialParams[i]);
                        FacialJoints[i].localRotation = Quaternion.Lerp(FacialJoints[i].localRotation, goalRotation, interpolatePoint);
                    }
                }
            }
            //frameData.facialParams;
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
            switch (Controller.Mode)
            {
                case PlayMode.Stream:
                    // Update data
                    if (UDPReceiver.IsDataNew())
                    {
                        frameData = AnimData.FromJsonData(UDPReceiver.ReceivedData);
                        interpolateFrameRest = UDPReceiver.EstimatedRestFrameTime;
                        // Calculate vertical stablization
                        if (AllowVerticalStablization)
                        {
                            UpdateModel(1, true); // set to next state
                            PushNewFeetHeights(); // calculate feet heights
                            if (IsVerticalStable() || GetLowestFeetHeight() < 0f)
                            {
                                HeightDiff = -GetLowestFeetHeight();
                            }
                            ChangeModelToLastSavedState(); // change the model state back
                        }
                        // New insert coroutine for new data
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

                case PlayMode.FileBvh:
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
