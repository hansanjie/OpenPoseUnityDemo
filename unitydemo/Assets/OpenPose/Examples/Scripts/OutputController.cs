using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace opdemo.examples
{
    using opdemo.dll;

    public class OutputController : MonoBehaviour
    {
        #region Singleton instance
        public static OutputController instance { get; private set; }
        private void Awake() {
            if (instance = null) instance = this;
            else
            {
                Destroy(instance);
                instance = this;
            }
        }
        #endregion

        [SerializeField] List<HumanController2D> humans;
        [SerializeField] ImageDisplayer imageDisplayer;

        private OPFrame currentFrame;

        public void PushNewOutput(string json)
        {
            currentFrame = OPFrame.FromJson(json);
            int unitsCount = currentFrame.units.Count;
            for (int i = 0; i < humans.Count; i++)
            {
                if (i < unitsCount)
                {
                    humans[i].PushNewUnitData(currentFrame.units[i]);
                    humans[i].SetActive(true);
                }
                else
                {
                    humans[i].SetActive(false);
                }
            }
        }

        public void PushNewImage(string json)
        {
            imageDisplayer.PushNewImageData(OPImage.FromJson(json));
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

