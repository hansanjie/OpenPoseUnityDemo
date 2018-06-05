using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace opdemo
{
    public class InputController : MonoBehaviour
    {
        // Singleton
        //private static InputController Instance;

        private void Awake()
        {
            //Instance = this;
        }

        // Use this for initialization
        void Start()
        {

        }

        // scene & character control
        void LastHuman()
        {

        }

        void NextHuman()
        {

        }

        void LastScene()
        {

        }

        void NextScene()
        {

        }

        // anim control
        public void ToggleInterpolation()
        {
            CharacterAnimController.AllowInterpolation = !CharacterAnimController.AllowInterpolation;
        }

        // Update is called once per frame
        void Update()
        {
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
