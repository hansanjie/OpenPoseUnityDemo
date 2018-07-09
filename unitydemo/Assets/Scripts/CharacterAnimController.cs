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
        [SerializeField] float facialParamMultiplier = 2f;
        [SerializeField] SkinnedMeshRenderer blendMesh;
        [SerializeField] List<Transform> Joints;
        [SerializeField] List<Transform> LowerFeetIndicators;
        
        // Animating data
        private AnimUnitData unitData = new AnimUnitData();
        private Vector3 InitRootGlobalPosition;
        private Vector3 SavedRootPosition;
        private Vector3 NextRootPosition;
        private Dictionary<int, Quaternion> InitRotations = new Dictionary<int, Quaternion>();
        private Dictionary<int, Quaternion> SavedRotations = new Dictionary<int, Quaternion>();
        private Dictionary<int, Quaternion> NextRotations = new Dictionary<int, Quaternion>();
        private float frameTime;
        private Coroutine currentFrameCoroutine;
        public void PushNewUnitData(AnimUnitData unitData, float frameTime)
        {
            this.unitData = unitData;
            this.frameTime = frameTime;

            if (currentFrameCoroutine != null)
                StopCoroutine(currentFrameCoroutine);

            switch (Controller.Mode)
            {
                case PlayMode.Stream:
                    if (AllowVerticalStablization) ComputeNewHeightDiff(); // Vertical stablization
                    currentFrameCoroutine = StartCoroutine(FrameSteppingCoroutine());
                    break;
                case PlayMode.FileJson:
                    if (AllowVerticalStablization) ComputeNewHeightDiff(); // Vertical stablization
                    currentFrameCoroutine = StartCoroutine(FrameInterpolatingCoroutine());
                    break;
                case PlayMode.FileBvh:
                    if (AllowVerticalStablization) ComputeNewHeightDiff(); // Vertical stablization
                    currentFrameCoroutine = StartCoroutine(FrameInterpolatingCoroutine());
                    break;
                case PlayMode.FileFbx:
                    break;
            }
        }

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
        private void ComputeNewHeightDiff()
        {
            UpdateModel(1f); // set to next state
            PushNewFeetHeights(); // calculate feet heights
            if (IsVerticalStable() || GetLowestFeetHeight() < 0f) // condition: normal standing or foot below ground
            {
                HeightDiff -= GetLowestFeetHeight(); // calculate bias
            }
            ChangeModelToLastSavedState(); // change the model state back
        }

        // Initialize
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

        // Used in SceneController for camera focus
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

        // Reset the model to the center
        public void Recenter()
        {
            OffsetPosition += InitRootGlobalPosition - Joints[0].position;
            transform.position = OffsetPosition;
        }

        // Reset the model to vertical position
        public void Revertical()
        {
            OffsetRotation = Quaternion.Inverse(Joints[0].localRotation);
            transform.rotation = OffsetRotation;

            Recenter();
        }
        
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
            // TODO: Unity and OP are using different coordinates, use Matrix to transfer.

            // Reset offsets
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;

            // Global translation
            SavedRootPosition = Joints[0].localPosition;
            NextRootPosition = -unitData.totalPosition / 100f;
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
                Joints[i].Rotate(unitData.jointAngles[i], Space.World);
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
            if (blendMesh != null)
            {
                for (int i = 0; i < unitData.facialParams.Count; i++)
                {
                    float currentWeight = blendMesh.GetBlendShapeWeight(i);
                    float interpolatedWeight = Mathf.Lerp(currentWeight, unitData.facialParams[i] * 100f / facialParamMultiplier, interpolatePoint);
                    blendMesh.SetBlendShapeWeight(i, interpolatedWeight);
                }
            }
        }

        private void UpdateModelAndFace(float interpolatePoint = 1f)
        {
            UpdateModel(interpolatePoint);
            UpdateFace(interpolatePoint);
        }

        IEnumerator FrameSteppingCoroutine() // Discrete interpolation
        {
            float subAccumulatedTime = 0f;
            for (int i = InsertStepNumber; i > 0; i--)
            {
                // insert a frame
                UpdateModelAndFace(1f / i);
                //Debug.Log(1f / i + " and " + Time.time);

                // Loop until the next step is reached
                while (subAccumulatedTime < frameTime / InsertStepNumber)
                {
                    subAccumulatedTime += Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }
                // A step is reached
                subAccumulatedTime -= frameTime / InsertStepNumber;
            }
        }

        IEnumerator FrameInterpolatingCoroutine() // Continuous interpolation
        {
            float frameTime = this.frameTime;
            float passedFrameTime = 0f;

            while (frameTime - passedFrameTime > 0f)
            {
                UpdateModelAndFace(Time.deltaTime / (frameTime - passedFrameTime));
                passedFrameTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}