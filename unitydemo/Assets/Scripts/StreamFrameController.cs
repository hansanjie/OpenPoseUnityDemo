using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace opdemo {
    public class StreamFrameController : MonoBehaviour {

        // Singleton
        public static StreamFrameController instance { get; private set; }
        private void Awake() { instance = this; }

        // Configurable values
        [SerializeField] float thresholdFrameDelay = 0.8f;
        [SerializeField] float latePenaltyMultiplier = 1.1f;

        // UI
        [SerializeField] Text delayText;

        // Frame data storage
        private Queue<AnimData> frameQueue = new Queue<AnimData>(); // frames awaits
        private AnimData currentFrame;

        // Time controlling variables
        private float currentTime = 0f; // for setting the received time for frames
        private float currentFrameTime = 0f;
        private float frameSpeedMultiplier = 1f;

        // Interface
        public void AppendNewFrameJson(string json) {
            AnimData frameData = AnimData.FromJsonData(json);
            frameData.receivedTime = currentTime;
            frameQueue.Enqueue(frameData);
        }
        public AnimData GetCurrentFrame() {
            if (currentFrame != null) return currentFrame;
            else return null;
        }

        // UI
        private void SetDelayThres(float f) {
            f = Mathf.Clamp(f, 0f, 3f);
            thresholdFrameDelay = f;
            delayText.text = f.ToString("F1");
        }
        public void IncreaseDelay() {
            SetDelayThres(thresholdFrameDelay + 0.1f);
        }
        public void DecreaseDelay() {
            SetDelayThres(thresholdFrameDelay - 0.1f);
        }
        
        // Updates
        void Update() {
            // Update receive time
            currentTime = Time.time;

            // Frame control
            if (Controller.Mode == PlayMode.Stream) {
                if (UDPReceiver.IsServerActive()) {
                    currentFrameTime += Time.deltaTime;
                    if (currentFrameTime * frameSpeedMultiplier > UDPReceiver.AvgFrameTime) { // New frame
                        if (frameQueue.Count > 0) {
                            currentFrame = frameQueue.Dequeue();
                            currentFrameTime = 0f; // -= UDPReceiver.AvgFrameTime / frameSpeedMultiplier;
                            CharacterAnimController.PushNewFrameData(currentFrame);
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
