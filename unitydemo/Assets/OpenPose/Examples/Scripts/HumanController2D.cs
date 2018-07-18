using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace opdemo.examples
{
    using opdemo.dll;
    public class HumanController2D : MonoBehaviour
    {
        public static int PoseKeypointsCount = 25;
        public static int HandKeypointsCount = 21;
        public static float ScoreThres = 0.05f;

        [SerializeField] Transform PoseParent;
        [SerializeField] Transform LHandParent;
        [SerializeField] Transform RHandParent;

        private OPUnit unitData;
        private bool active;
        private List<Transform> poseJoints = new List<Transform>();
        private List<Transform> lHandJoints = new List<Transform>();
        private List<Transform> rHandJoints = new List<Transform>();

        public void PushNewUnitData (OPUnit unitData) 
        {
            if (unitData != null)
            {
                this.unitData = unitData;
            }
        }

        public void SetActive(bool active)
        {
            this.active = active;
        }
        
        // Use this for initialization
        void Start()
        {
            InitJoints();
        }

        private void InitJoints()
        {
            if (PoseParent != null)
            {
                Debug.Assert(PoseParent.childCount == PoseKeypointsCount);
                if (PoseParent.childCount == PoseKeypointsCount)
                {
                    for (int i = 0; i < PoseParent.childCount; i++)
                    {
                        poseJoints.Add(PoseParent.GetChild(i));
                    }
                }                
            }
            if (LHandParent != null)
            {
                Debug.Assert(LHandParent.childCount == HandKeypointsCount);
                if (LHandParent.childCount == HandKeypointsCount)
                {
                    for (int i = 0; i < LHandParent.childCount; i++)
                    {
                        lHandJoints.Add(LHandParent.GetChild(i));
                    }
                }
            }
            if (RHandParent != null)
            {
                Debug.Assert(RHandParent.childCount == HandKeypointsCount);
                if (RHandParent.childCount == HandKeypointsCount)
                {
                    for (int i = 0; i < RHandParent.childCount; i++)
                    {
                        rHandJoints.Add(RHandParent.GetChild(i));
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (unitData != null && unitData.poseKeypoints.Count == PoseKeypointsCount)
            {
                UpdatePositions();
            }            
        }
        
        private void UpdatePositions() // Only work in main thread
        {
            for (int i = 0; i < poseJoints.Count; i++)
            {
                if (!active || i >= unitData.poseKeypoints.Count)
                {
                    poseJoints[i].gameObject.SetActive(false);
                    continue;
                }

                if (unitData.poseKeypoints[i].z < ScoreThres)
                {
                    poseJoints[i].gameObject.SetActive(false);
                }
                else
                {
                    poseJoints[i].gameObject.SetActive(true);
                    poseJoints[i].localPosition = new Vector3(unitData.poseKeypoints[i].x, unitData.poseKeypoints[i].y, 0f);
                }
            }

            for (int i = 0; i < lHandJoints.Count; i++)
            {
                if (!active || i >= unitData.handKeypoints_L.Count)
                {
                    lHandJoints[i].gameObject.SetActive(false);
                    continue;
                }

                if (unitData.handKeypoints_L[i].z < ScoreThres)
                {
                    lHandJoints[i].gameObject.SetActive(false);
                } else
                {
                    lHandJoints[i].gameObject.SetActive(true);
                    lHandJoints[i].localPosition = new Vector3(unitData.handKeypoints_L[i].x, unitData.handKeypoints_L[i].y, 0f);
                }
            }

            for (int i = 0; i < rHandJoints.Count; i++)
            {
                if (!active || i >= unitData.handKeypoints_R.Count)
                {
                    rHandJoints[i].gameObject.SetActive(false);
                    continue;
                }

                if (unitData.handKeypoints_R[i].z < ScoreThres)
                {
                    rHandJoints[i].gameObject.SetActive(false);
                }
                else
                {
                    rHandJoints[i].gameObject.SetActive(true);
                    rHandJoints[i].localPosition = new Vector3(unitData.handKeypoints_R[i].x, unitData.handKeypoints_R[i].y, 0f);
                }
            }
        }
    }
}

