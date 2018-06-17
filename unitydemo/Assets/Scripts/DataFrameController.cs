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

        [SerializeField] float frameTime = 0.05f;
        [SerializeField] float speedMultiplier = 1.2f;
        private float accumulateFrameTime = 0f;
        private AnimDataSet dataSet = new AnimDataSet();
        private int currentFrameNumber = 0;
        private bool playingAnimation = false;

        // Interface
        public static string FileName = "./InputFiles/new_full.bvh";
        public static bool IsReady { get { try { return instance.dataSet.isValid; } catch { return false; } } }
        public static float RestFrameTime { get { return instance.frameTime - instance.accumulateFrameTime; } }
        //public static List<Vector3> DefaultSkeletonData { get { if (IsReady) return instance.dataSet.default_skeleton; else return new List<Vector3>(); } }
        public static AnimData GetCurrentFrame()
        {
            if (IsReady)
                return instance.dataSet.dataList[instance.currentFrameNumber];
            else
                return new AnimData();
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
                frameTime = dataSet.frameTime;
                StartCoroutine(PlayAnimationCoroutine());
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
                frameTime = dataSet.frameTime;
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
            //float totalTime = 0f;
            while (true)
            {
                if (playingAnimation)
                {
                    //totalTime += Time.deltaTime;
                    accumulateFrameTime += Time.deltaTime;
                    while (accumulateFrameTime > frameTime) // entering new frame
                    {
                        accumulateFrameTime -= frameTime;
                        if (currentFrameNumber < dataSet.dataList.Count - 1) currentFrameNumber++; // anim not finished yet
                        else playingAnimation = false; // anim finished
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
