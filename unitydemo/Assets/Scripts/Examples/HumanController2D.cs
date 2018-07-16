using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace opdemo.examples
{
    using opdemo.dll;
    public class HumanController2D : MonoBehaviour
    {
        [SerializeField] List<GameObject> components = new List<GameObject>();

        private OPUnit unitData;

        public void PushNewOutputUnitData (OPUnit unitData) 
        {
            if (unitData != null)
            {
                this.unitData = unitData;
            }
        }

        private void UpdatePositions()
        {
            if (components.Count < 25)
            {
                //Debug.Log("not enough components");
                return;
            }
            if (unitData == null || unitData.poseKeypoints.Count < 25)
            {
                //Debug.Log("unitdata invalid");
                return;
            }
            for (int i = 0; i < 25; i++)
            {
                if (unitData.poseKeypoints[i].z < 0.05f)
                {
                    components[i].SetActive(false);
                } else
                {
                    components[i].SetActive(true);
                    components[i].GetComponent<RectTransform>().localPosition = new Vector3(unitData.poseKeypoints[i].x, unitData.poseKeypoints[i].y, 0f);
                }
            }
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            UpdatePositions();
        }
    }
}

