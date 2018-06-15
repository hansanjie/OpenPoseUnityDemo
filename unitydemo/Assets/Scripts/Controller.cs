using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace opdemo
{
    public class Controller : MonoBehaviour
    {
        // Singleton
        private static Controller instance = null;

        // Code settings
        public bool DefaultAllowInterpolation = false;
        public int InBetweenStepNumber = 2;
        public bool AllowFeetStablization = true;
        public int StablizationCompareSequence = 3;
        public float StableMovementThreshold = 0.02f;
        public string DefaultInputFileName = "input";

        private static PlayMode defaultMode = PlayMode.Stream;

        private PlayMode mode = PlayMode.Default;

        private bool inMain = false;

        // Interface
        public static PlayMode Mode { get { try { if (instance.mode == PlayMode.Default) return defaultMode; else return instance.mode; } catch { return defaultMode; } } set { try { instance.mode = value; } catch { } } }
        public static void StartPlay(PlayMode mode = PlayMode.Default)
        {
            Mode = mode;
            instance.LoadMain();
        }

        public void SelectStream()
        {
            mode = PlayMode.Stream;
            LoadMain();
        }

        public void SelectJson()
        {
            mode = PlayMode.FileJson;
            LoadMain();
        }

        public void SelectBvh()
        {
            mode = PlayMode.FileBvh;
            LoadMain();
        }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            CharacterAnimController.AllowInterpolation = DefaultAllowInterpolation;
            CharacterAnimController.InsertStepNumber = InBetweenStepNumber;
            CharacterAnimController.AllowVerticalStablization = AllowFeetStablization;
            CharacterAnimController.CompareDifference = StablizationCompareSequence;
            CharacterAnimController.VerticalMovementThres = StableMovementThreshold;
            DataFrameController.FileName = DefaultInputFileName;
        }

        private void LoadMain()
        {
            DontDestroyOnLoad(gameObject);
            inMain = true;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
        }

        private void LoadMenu()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
            Destroy(gameObject);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (inMain)
                {
                    LoadMenu();
                } else
                {
                    Application.Quit();
                }                
            }
        }
    }

    public enum PlayMode
    {
        Default,
        Stream,
        FileJson,
        FileFbx,
        FileBvh
    }
}
