using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace opdemo
{
    public class DataFrameController : MonoBehaviour
    {
        // Singleton
        private static DataFrameController instance;

        // UI
        [SerializeField] GameObject PlayButton, PauseButton;
        [SerializeField] Text SpeedText, FrameNumberText;
        [SerializeField] Slider FrameSlider;

        // Data and frame info
        private AnimDataSet dataSet = new AnimDataSet();
        private float frameTime { get { return dataSet.frameTime; } }
        private int frameNumber { get { return dataSet.dataList.Count; } }
        private float speedMultiplier = 1.0f;
        private int currentFrameNumber = 0;
        private bool playingAnimation = false;
        private float accumulateFrameTime = 0f;

        // Interface
        public static string FileName = "C:\\Users\\tz1\\Documents\\OpenPoseUnityDemo\\unitydemo\\Assets\\InputFiles\\new_full.bvh";
        public static bool IsReady { get { try { return instance.dataSet.isValid; } catch { return false; } } }
        public static float RestFrameTime { get { return instance.frameTime / instance.speedMultiplier - instance.accumulateFrameTime; } }
        //public static List<Vector3> DefaultSkeletonData { get { if (IsReady) return instance.dataSet.default_skeleton; else return new List<Vector3>(); } }
        public static AnimUnitData GetCurrentFrame()
        {
            if (IsReady)
                return instance.dataSet.dataList[instance.currentFrameNumber];
            else
                return new AnimUnitData();
        }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            if (Controller.Mode == PlayMode.FileJson) InitDataJson(FileName);
            if (Controller.Mode == PlayMode.FileBvh) InitDataBvh(FileName);
        }

        private void InitDataJson(string file)
        {
            //string filePath = Path.Combine(Application.dataPath, file);
            string filePath = file;
            if (File.Exists(filePath))
            {
                string dataAsJson = File.ReadAllText(filePath);
                dataSet = AnimDataSet.FromJsonData(dataAsJson);
            }
            else
            {
                Debug.Log("File not exists");
            }
        }

        private void InitDataBvh(string file)
        {
            //string filePath = Path.Combine(Application.dataPath, file);
            string filePath = file;
            if (File.Exists(filePath))
            {
                string data = File.ReadAllText(filePath);
                dataSet = AnimDataSet.FromBvhData(data);
            }
            else
            {
                Debug.Log("File not exists");
            }
        }

        // Operations
        public void SetFrameNumberFloat(float f)
        {
            SetFrameNumber(Mathf.RoundToInt(f));
        }
        public void SetFrameNumber(int n)
        {
            currentFrameNumber = Mathf.Clamp(n, 0, frameNumber - 1);
        }
        public void PlayOrPause()
        {
            playingAnimation = !playingAnimation;
        }
        public void Stop()
        {
            SetFrameNumber(0);
            playingAnimation = false;
        }
        public void SpeedUp()
        {
            if (speedMultiplier < 2f) speedMultiplier += 0.1f;
            else speedMultiplier += 0.5f;
            speedMultiplier = Mathf.Clamp(speedMultiplier, 0.1f, 5f);
        }
        public void SpeedDown()
        {
            if (speedMultiplier < 2.1f) speedMultiplier -= 0.1f;
            else speedMultiplier -= 0.5f;
            speedMultiplier = Mathf.Clamp(speedMultiplier, 0.1f, 5f);
        }
        public void LastFrame()
        {
            if (currentFrameNumber > 0) currentFrameNumber--;
        }
        public void NextFrame()
        {
            if (currentFrameNumber < frameNumber - 1) currentFrameNumber++;
        }

        // User input
        private void InputDetectionUpdate()
        {
            if (dataSet.isValid)
            {
                if (Input.GetKeyDown(KeyCode.R)) Stop();
                if (Input.GetKeyDown(KeyCode.Space)) PlayOrPause();
                if (Input.GetKeyDown(KeyCode.UpArrow)) SpeedUp();
                if (Input.GetKeyDown(KeyCode.DownArrow)) SpeedDown();
                if (Input.GetKeyDown(KeyCode.LeftArrow)) LastFrame();
                if (Input.GetKeyDown(KeyCode.RightArrow)) NextFrame();
            }
        }

        private void PlayAnimationUpdate()
        {
            //float totalTime = 0f;
            if (playingAnimation)
            {
                //totalTime += Time.deltaTime;
                accumulateFrameTime += Time.deltaTime;
                while (accumulateFrameTime > frameTime / speedMultiplier) // entering new frame
                {
                    accumulateFrameTime -= frameTime / speedMultiplier;
                    if (currentFrameNumber < frameNumber - 1) currentFrameNumber++; // anim not finished yet
                    else playingAnimation = false; // anim finished
                }
            }
        }

        private void UIUpdate()
        {
            PauseButton.SetActive(playingAnimation);
            PlayButton.SetActive(!playingAnimation);
            SpeedText.text = speedMultiplier.ToString("F1");
            FrameNumberText.text = currentFrameNumber.ToString();
            FrameSlider.maxValue = frameNumber - 1;
            FrameSlider.value = currentFrameNumber;
        }

        private void Update()
        {
            InputDetectionUpdate();
            PlayAnimationUpdate();
            UIUpdate();
        }
    }
}
