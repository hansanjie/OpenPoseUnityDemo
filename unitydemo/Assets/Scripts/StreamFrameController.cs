using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace opdemo {
    public class StreamFrameController : MonoBehaviour {

        // Singleton
        private static StreamFrameController instance;

        // Configurable values
        [SerializeField] float thresholdFrameDelay = 0.8f;
        [SerializeField] float latePenaltyMultiplier = 1.2f;

        // Test UI
        [SerializeField] Text delayText;
        [SerializeField] Text stepText;

        private Queue<AnimUnitData> frameQueue = new Queue<AnimUnitData>();
        private AnimUnitData currentFrame = new AnimUnitData();

        private float currentFrameTime = 0f;
        private float frameSpeedMultiplier = 1f;

        public static void AppendNewFrameJson(string json) {
            AnimUnitData frameData = new AnimUnitData();//.FromJsonData(json); // MODIFIED
            if (frameData.isValid) {
                //frameData.receivedTime = Time.time;
                //instance.StartCoroutine(SetReceiveTimeTo(frameData));
                instance.frameQueue.Enqueue(frameData);
            }
        }

        public static AnimUnitData GetCurrentFrame() {
            if (instance.currentFrame != null) return instance.currentFrame;
            else return new AnimUnitData();
        }

        private static bool _dataNew = false;
        public static bool DataNew { // temp solution: only obtain once
            get {
                if (_dataNew) {
                    _dataNew = false;
                    return true;
                } else return false;
            }
            set {
                _dataNew = value;
            }
        }

        private void Awake() {
            instance = this;
        }

        // Use this for initialization
        void Start() {

        }

        public void SetDelayThres(float f) {
            f = Mathf.Clamp(f, 0f, 3f);
            thresholdFrameDelay = f;
            delayText.text = f.ToString("F1");
        }

        public void SetLatePenaltyMultipilier(float f) {
            latePenaltyMultiplier = f;
            stepText.text = f.ToString("F1");
        }

        public void IncreaseDelay() {
            SetDelayThres(thresholdFrameDelay + 0.1f);
        }

        public void DecreaseDelay() {
            SetDelayThres(thresholdFrameDelay - 0.1f);
        }

        // Update is called once per frame
        void Update() {
            foreach (AnimUnitData data in frameQueue) {
                if (data.receivedTime <= 0f) {
                    data.receivedTime = Time.time;
                }
            }
            if (Controller.Mode == PlayMode.Stream) {
                if (UDPReceiver.IsServerActive()) {
                    currentFrameTime += Time.deltaTime;
                    if (currentFrameTime * frameSpeedMultiplier > UDPReceiver.AvgFrameTime) {
                        if (frameQueue.Count > 0) {
                            currentFrame = frameQueue.Dequeue();
                            currentFrameTime = 0f;
                            DataNew = true;
                            //Debug.Log(Time.time - currentFrame.receivedTime);
                            if (Time.time - currentFrame.receivedTime > thresholdFrameDelay) { // play too slow
                                frameSpeedMultiplier *= latePenaltyMultiplier;
                            }
                        } else {
                            frameSpeedMultiplier = 1f;
                        }
                    }
                }
            }
        }
    }
}
