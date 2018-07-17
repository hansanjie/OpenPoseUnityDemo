using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace opdemo.examples
{
    using opdemo.dll;
    public class HumanController2D : MonoBehaviour
    {
        public static int KeypointsCount = 25;
        public static float ScoreThres = 0.05f;

        [SerializeField] Transform JointsParent;

        private OPUnit unitData;
        private bool active;
        private List<Transform> joints = new List<Transform>();

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
            if (JointsParent == null) return;
            if (JointsParent.childCount != KeypointsCount)
            {
                Debug.Log("Invalid count of keypoints: " + joints.Count);
                return;
            }
            for (int i = 0; i < JointsParent.childCount; i++)
            {
                joints.Add(JointsParent.GetChild(i));
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (unitData != null && unitData.poseKeypoints.Count == KeypointsCount)
            {
                UpdatePositions();
            }            
        }
        
        private void UpdatePositions() // Only work in main thread
        {
            for (int i = 0; i < KeypointsCount; i++)
            {
                if (!active)
                {
                    joints[i].gameObject.SetActive(false);
                    continue;
                }

                if (unitData.poseKeypoints[i].z < ScoreThres)
                {
                    joints[i].gameObject.SetActive(false);
                }
                else
                {
                    joints[i].gameObject.SetActive(true);
                    joints[i].localPosition = new Vector3(unitData.poseKeypoints[i].x, unitData.poseKeypoints[i].y, 0f);
                }
            }
        }
    }
}

