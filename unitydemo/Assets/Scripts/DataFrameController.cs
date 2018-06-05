using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace opdemo
{
    public class DataFrameController : MonoBehaviour
    {
        // Singleton
        private static DataFrameController instance;

        [SerializeField] string fileName = "input.json";
        [SerializeField] float frameTime = 0.05f;
        [SerializeField] float speedMultiplier = 1.2f;
        private float accumulateFrameTime = 0f;
        private AnimDataSet dataSet;
        private int currentFrameNumber = 0;
        private bool playingAnimation = false;

        // Interface
        public static bool IsReady { get { try { return instance.dataSet.isValid; } catch { return false; } } }
        public static float RestFrameTime { get { return instance.frameTime - instance.accumulateFrameTime; } }
        public static List<Vector3> DefaultSkeletonData { get { if (IsReady) return instance.dataSet.default_skeleton; else return new List<Vector3>(); } }
        public static AnimData GetCurrentFrame()
        {
            if (IsReady)
                return new AnimData(instance.dataSet.dataList[instance.currentFrameNumber]);
            else
                return new AnimData("");
        }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            if (Controller.Mode == PlayMode.FileJson) InitData();
        }

        private void InitData()
        {
            string filePath = Path.Combine(Application.dataPath, fileName);
            if (File.Exists(filePath))
            {
                string dataAsJson = File.ReadAllText(filePath);
                dataSet = new AnimDataSet(dataAsJson);
                StartCoroutine(PlayAnimationCoroutine());
            }
            else
            {
                Debug.Log("File not exists");
            }
        }

        private void InputDetectionUpdate()
        {
            if (dataSet.isValid)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    currentFrameNumber = 0;
                }
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    playingAnimation = !playingAnimation;
                }
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    frameTime /= speedMultiplier;
                }
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    frameTime *= speedMultiplier;
                }
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    if (currentFrameNumber > 0) currentFrameNumber--;
                }
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    if (currentFrameNumber < dataSet.dataList.Count - 1) currentFrameNumber++;
                }
            }
        }

        private void Update()
        {
            InputDetectionUpdate();
        }

        IEnumerator PlayAnimationCoroutine()
        {
            while (true)
            {
                yield return new WaitUntil(() => { return playingAnimation; });

                accumulateFrameTime += Time.deltaTime;
                if (accumulateFrameTime > frameTime)
                {
                    accumulateFrameTime -= frameTime;
                    if (currentFrameNumber < dataSet.dataList.Count - 1) currentFrameNumber++;
                    else playingAnimation = false;
                    yield return null;
                }
            }
        }
    }
}
