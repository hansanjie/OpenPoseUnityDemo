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
        public static Vector3 OffsetPosition = Vector3.zero;
        public static Quaternion OffsetRotation = Quaternion.identity;
        public static bool AllowInterpolation = true;
        public static int InsertStepNumber = 2;
        public static bool AllowVerticalStablization = true;
        public static int CompareDifference = 3;
        public static float VerticalMovementThres = 0.02f;
        //private static float DisplayedFrameRate = 0f;
        //private static float LastDisplayedFrameTime = 0f;

        // Model settings
        [SerializeField] bool AllowFacialAnim = true;
        [SerializeField] float facialParamMultiplier = 2f;
        [SerializeField] SkinnedMeshRenderer blendMesh;
        [SerializeField] List<Transform> Joints;
        //[SerializeField] List<Transform> FacialJoints;
        [SerializeField] List<Transform> LowerFeetIndicators;
        //[SerializeField] List<Quaternion> FacialInitRotations;
        //[SerializeField] List<Quaternion> FacialFullRotations;

        // Animating data
        private AnimData frameData = new AnimData();
        private Vector3 InitRootGlobalPosition;
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
                InitRotations.Add(i, Joints[i].localRotation);
                SavedRotations.Add(i, Joints[i].localRotation);
                NextRotations.Add(i, Joints[i].localRotation);
                //Instantiate(JointObject, Joints[i], false);
            }
            InitRootGlobalPosition = Joints[0].position;
        }
        
        /*private void InitSkeleton(List<Vector3> posList) // deprecated
        {
            Debug.Log(posList.Count);
            for (int i = 0; i < Mathf.Min(Joints.Count, posList.Count); i++)
            {
                if (Joints[i] == null) continue;
                Joints[i].position = new Vector3(-posList[i].x, posList[i].y, posList[i].z) / 100f;
            }
        }*/

        public Transform GetFocusCenter(CamFocusPart focus)
        {
            switch (focus)
            {
                case CamFocusPart.Hip:
                    return Joints[0];
                case CamFocusPart.Chest:
                    return Joints[6];
                case CamFocusPart.Head:
                    if (Joints[15] != null) return Joints[15];
                    else return Joints[12];
                default:
                    return Joints[0];
            }
        }

        public void Recenter()
        {
            OffsetPosition += InitRootGlobalPosition - Joints[0].position;
        }

        public void Revertical()
        {
            OffsetRotation = Quaternion.Inverse(Joints[0].localRotation);
        }

        /*public void AdjustHeight()
        {
            //float humanRootHeight = frameData.rootHeight / 100f;
            //float modelRootHeight = InitRootPosition.y;
            HeightDifference.y = InitRootPosition.y - frameData.rootHeight / 100f;
        }*/

        /*[ExecuteInEditMode]
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
        }*/

        private void ChangeModelToLastSavedState()
        {
            Joints[0].localPosition = SavedRootPosition;
            for (int i = 0; i < Joints.Count; i++)
            {
                if (Joints[i] == null) continue;
                Joints[i].localRotation = SavedRotations[i];
            }
        }

        private void UpdateModel(float interpolatePoint = 1f)
        {
            // TODO: Unity and OP are using different coordinates, write a new script for transiting. 

            // Reset offsets
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;

            // Global translation
            SavedRootPosition = Joints[0].localPosition;
            NextRootPosition = -frameData.totalPosition / 100f;
            Joints[0].localPosition = Vector3.Lerp(SavedRootPosition, NextRootPosition, interpolatePoint);
            // Global rotation
            /*Vector3 axisAngle = new Vector3(-frameData.jointAngles[0].x, frameData.jointAngles[0].y, frameData.jointAngles[0].z);
            Joints[0].rotation = InitRotations[0];
            Joints[0].Rotate(axisAngle, axisAngle.magnitude * Mathf.Rad2Deg, Space.World);
            Joints[0].Rotate(Vector3.left, 180f, Space.World);
            UpdatedRotations[0] = Joints[0].localRotation;
            Joints[0].rotation = InitRotations[0];*/
            // Global rotation & joints rotations
            for (int i = 0; i < Joints.Count; i++)
            {
                if (Joints[i] == null) continue;
                SavedRotations[i] = Joints[i].localRotation;
            }
            transform.rotation = Quaternion.identity;
            for (int i = 0; i < Joints.Count; i++)
            {
                if (Joints[i] == null) continue;
                Joints[i].localRotation = InitRotations[i];
                //if (i == 0) Joints[0].Rotate(180f, 0f, 0f, Space.World);
                Joints[i].Rotate(frameData.jointAngles[i], Space.World);
                if (i == 0) 
                {
                    Joints[0].Rotate(180f, 0f, 0f, Space.World); // upside down
                }
                NextRotations[i] = Joints[i].localRotation;
                Joints[i].localRotation = InitRotations[i];
            }
            // Apply global and joints rotations
            for (int i = 0; i < Joints.Count; i++)
            {
                //if (i < 20 || i > 61) continue;
                if (Joints[i] == null) continue;
                Joints[i].localRotation = Quaternion.Lerp(SavedRotations[i], NextRotations[i], interpolatePoint);
            }

            // Set offsets
            transform.position = OffsetPosition;
            transform.rotation = OffsetRotation;

            // Vertical stablizer
            if (AllowVerticalStablization && LowerFeetIndicators.Count > 0)
            {
                transform.Translate(Vector3.up * HeightDiff, Space.World);
            }
        }

        private void UpdateFace(float interpolatePoint = 1f)
        {
            if (AllowFacialAnim)
            {
                /*for (int i = 0; i < Mathf.Min(1, FacialJoints.Count); i++)
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
                }*/
                if (blendMesh != null)
                {
                    for (int i = 0; i < frameData.facialParams.Count; i++)
                    {
                        float currentWeight = blendMesh.GetBlendShapeWeight(i);
                        float interpolatedWeight = Mathf.Lerp(currentWeight, frameData.facialParams[i] * 100f / facialParamMultiplier, interpolatePoint);
                        blendMesh.SetBlendShapeWeight(i, interpolatedWeight);
                    }
                }
            }
        }

        private void UpdateModelAndFace(float interpolatePoint = 1f)
        {
            UpdateModel(interpolatePoint);
            UpdateFace(interpolatePoint);
        }

        IEnumerator InsertStepsCoroutine()
        {
            float subAccumulatedTime = 0f;
            for (int i = InsertStepNumber; i > 0; i--)
            {
                // insert a frame
                UpdateModelAndFace(1f / i);
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
                    {
                        // Update data
                        if (StreamFrameController.DataNew)
                        {
                            frameData = StreamFrameController.GetCurrentFrame();
                            //interpolateFrameRest = UDPReceiver.EstimatedRestFrameTime;
                            // Calculate vertical stablization
                            if (frameData.isValid) {
                                if (AllowVerticalStablization) {
                                    UpdateModel(1f); // set to next state
                                    PushNewFeetHeights(); // calculate feet heights
                                    if (IsVerticalStable() || GetLowestFeetHeight() < 0f) {
                                        HeightDiff -= GetLowestFeetHeight();
                                    }
                                    ChangeModelToLastSavedState(); // change the model state back
                                }
                                // New insert coroutine for new data
                                //if (AllowInterpolation)
                                {
                                    StopCoroutine(InsertStepsCoroutine());
                                    StartCoroutine(InsertStepsCoroutine());
                                }
                            }
                        }
                        // Update model every frame for interpolation
                        /*if (AllowInterpolation)
                        {
                            if (frameData.isValid)
                            {
                                UpdateModelAndFace(Time.deltaTime / interpolateFrameRest);
                            }
                        }*/
                    }
                    break;
                case PlayMode.FileJson:
                    {
                        frameData = DataFrameController.GetCurrentFrame();
                        bool newData = DataFrameController.RestFrameTime > interpolateFrameRest;
                        interpolateFrameRest = DataFrameController.RestFrameTime;
                        if (frameData.isValid)
                        {
                            if (AllowVerticalStablization)
                            {
                                if (newData) // push new feet data
                                {
                                    UpdateModel(1f); // set to next state
                                    PushNewFeetHeights(); // calculate feet heights
                                    ChangeModelToLastSavedState(); // change the model state back
                                }

                                if (IsVerticalStable() || GetLowestFeetHeight() < 0f)
                                {
                                    HeightDiff -= GetLowestFeetHeight();
                                }
                            }
                            if (AllowInterpolation)
                            {
                                UpdateModelAndFace(Time.deltaTime / interpolateFrameRest);
                            }
                            else
                            {
                                UpdateModelAndFace();
                            }
                        }
                    }
                    break;
                case PlayMode.FileBvh:
                    {
                        frameData = DataFrameController.GetCurrentFrame();
                        bool newData = DataFrameController.RestFrameTime > interpolateFrameRest;
                        interpolateFrameRest = DataFrameController.RestFrameTime;
                        if (frameData.isValid)
                        {
                            if (AllowVerticalStablization)
                            {
                                if (newData) // push new feet data
                                {
                                    UpdateModel(1f); // set to next state
                                    PushNewFeetHeights(); // calculate feet heights
                                    ChangeModelToLastSavedState(); // change the model state back
                                }

                                if (IsVerticalStable() || GetLowestFeetHeight() < 0f)
                                {
                                    HeightDiff -= GetLowestFeetHeight();
                                }
                            }
                            if (AllowInterpolation) UpdateModel(Time.deltaTime / interpolateFrameRest);
                            else UpdateModel();
                        }
                    }
                    break;
            }
        }
    }
}