using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

namespace opdemo.examples
{
    using opdemo.dll;

    public class OPWrapper : OP_API
    {
        protected override void OPOutput(string message, byte[] imageData, int type = 0)
        {
            base.OPOutput(message, imageData, type);

            if (OutputController.instance != null)
            {
                switch (type)
                {
                    case 0:
                        //OutputController.instance.PushNewOutput(message);
                        OutputController.instance.PushNewImage(imageData);
                        break;
                    case 1:
                        //OutputController.instance.PushNewImage(message);
                        break;
                }
            }            
        }

        protected override void Awake()
        {
            base.Awake();

            OPSetParameter(OPFlag.HAND);
            OPRun();
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
