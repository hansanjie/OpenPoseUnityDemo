using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace opdemo
{
    public class FaceWeightsController : MonoBehaviour
    {
        private AnimData frameData;
        private SkinnedMeshRenderer blendMesh { get { return GetComponent<SkinnedMeshRenderer>(); } }
        [SerializeField] float multiplier = 2f;

        // Use this for initialization
        void Start()
        {
            
        }

        private void UpdateFace()
        {
            for (int i = 0; i < frameData.facialParams.Count; i++)
            {
                blendMesh.SetBlendShapeWeight(i, frameData.facialParams[i] * 100f / multiplier);
            }
        }

        // Update is called once per frame
        void Update()
        {
            switch (Controller.Mode)
            {
                case PlayMode.Stream:
                    // Update data
                    //if (UDPReceiver.IsDataNew())
                    //{
                        //frameData = AnimData.FromJsonData(UDPReceiver.ReceivedData);
                        //UpdateFace();
                        //interpolateFrameRest = UDPReceiver.EstimatedRestFrameTime;
                        // Calculate vertical stablization
                        //if (AllowVerticalStablization)
                        //{
                            //UpdateModel(1f, true); // set to next state
                            //PushNewFeetHeights(); // calculate feet heights
                            //if (IsVerticalStable() || GetLowestFeetHeight() < 0f)
                            //{
                            //    HeightDiff -= GetLowestFeetHeight();
                            //}
                            //ChangeModelToLastSavedState(); // change the model state back
                        //}
                        // New insert coroutine for new data
                        //if (!AllowInterpolation)
                        //{
                            //StopCoroutine(InsertStepsCoroutine());
                            //StartCoroutine(InsertStepsCoroutine());
                        //}
                    //}
                    // Update model every frame for interpolation
                    //if (AllowInterpolation)
                    //{
                        //if (frameData.isValid) UpdateModel(Time.deltaTime / interpolateFrameRest);
                    //}
                    break;
            }
        }
    }
}
