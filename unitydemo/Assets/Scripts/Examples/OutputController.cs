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

        private OPFrame currentFrame;

        public void PushNewOutput(string json)
        {
            currentFrame = OPFrame.FromJson(json);
            for (int i = 0; i < currentFrame.units.Count; i++)
            {
                if (i > 2) break;
                humans[0].PushNewOutputUnitData(currentFrame.units[0]);
            }
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

