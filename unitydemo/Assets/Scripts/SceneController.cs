using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace opdemo
{
    public class SceneController : MonoBehaviour
    {

        // Singleton
        //private static SceneController instance;

        [SerializeField] List<GameObject> HumanModels;
        [SerializeField] List<GameObject> SceneModels;
        private int _HumanModelIndex = 0, _SceneModelIndex = 0;
        public int HumanModelIndex { set { setHumanModel(value); } get { return _HumanModelIndex; } }
        public int SceneModelIndex { set { setSceneModel(value); } get { return _SceneModelIndex; } }

        // Use this for initialization
        void Start()
        {
            if (HumanModels.Count == 0 || SceneModels.Count == 0)
            {
                Debug.Log("Empty list in Models or Scenes");
                return;
            }
            ResetAll();
            HumanModelIndex = 0;
            SceneModelIndex = 0;
        }

        private void ResetAll(bool human = true, bool scene = true)
        {
            if (human)
                foreach (GameObject o in HumanModels)
                {
                    o.SetActive(false);
                }

            if (scene)
                foreach (GameObject o in SceneModels)
                {
                    o.SetActive(false);
                }
        }

        public void NextScene()
        {
            SceneModelIndex++;
        }

        public void LastScene()
        {
            SceneModelIndex--;
        }

        public void NextHuman()
        {
            HumanModelIndex++;
        }

        public void LastHuman()
        {
            HumanModelIndex--;
        }

        public void Recenter()
        {
            HumanModels[HumanModelIndex].GetComponent<CharacterAnimController>().Recenter();
        }

        private void setHumanModel(int index)
        {
            if (index < 0)
            {
                setHumanModel(HumanModels.Count + index % HumanModels.Count);
                return;
            }
            else if (index >= HumanModels.Count)
            {
                setHumanModel(index % HumanModels.Count);
                return;
            }
            else
            {
                HumanModels[_HumanModelIndex].SetActive(false);
                _HumanModelIndex = index;
                HumanModels[_HumanModelIndex].SetActive(true);
                CameraController.FocusCenter = HumanModels[_HumanModelIndex].GetComponent<CharacterAnimController>().GetCenter();
            }
        }

        private void setSceneModel(int index)
        {
            if (index < 0)
            {
                setSceneModel(SceneModels.Count + index % SceneModels.Count);
                return;
            }
            else if (index >= SceneModels.Count)
            {
                setSceneModel(index % SceneModels.Count);
                return;
            }
            else
            {
                SceneModels[_SceneModelIndex].SetActive(false);
                _SceneModelIndex = index;
                SceneModels[_SceneModelIndex].SetActive(true);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Comma))
            {
                LastHuman();
            }
            if (Input.GetKeyDown(KeyCode.Period))
            {
                NextHuman();
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                LastScene();
            }
            if (Input.GetKeyDown(KeyCode.Slash))
            {
                NextScene();
            }
        }
    }
}
