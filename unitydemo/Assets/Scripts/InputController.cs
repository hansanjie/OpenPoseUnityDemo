using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace opdemo
{
    public class InputController : MonoBehaviour
    {
        // Singleton
        //private static InputController Instance;

        [SerializeField] GameObject InterpolationCheckmark;

        private void Awake()
        {
            //Instance = this;
        }

        // Use this for initialization
        void Start()
        {
            InterpolationCheckmark.SetActive(CharacterAnimController.AllowInterpolation);
        }

        // anim control
        public void ToggleInterpolation()
        {
            CharacterAnimController.AllowInterpolation = !CharacterAnimController.AllowInterpolation;
            if (InterpolationCheckmark != null)
            {
                InterpolationCheckmark.SetActive(CharacterAnimController.AllowInterpolation);
            }
        }

        public void ReCenter()
        {
            GetComponent<SceneController>().Recenter();
        }

        public void ReVertical()
        {
            GetComponent<SceneController>().Revertical();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                ReCenter();
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                ReVertical();
            }
            // TODO
            // scene & character control
            /*if (Input.GetKeyDown(KeyCode.Comma))
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
            }*/
        }
    }
}
