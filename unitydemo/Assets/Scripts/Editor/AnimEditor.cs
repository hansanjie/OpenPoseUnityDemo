// /* 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace opdemo
{
    [CustomEditor(typeof(CharacterAnimController))]
    public class FacialRotationConfiture : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Configure init facial rotations"))
            {
                ((CharacterAnimController)target).ConfigureInitFacialRotations();
            }

            if (GUILayout.Button("Show init facial rotations"))
            {
                ((CharacterAnimController)target).ShowInitFacialRotations();
            }

            if (GUILayout.Button("Configure full facial rotations"))
            {
                ((CharacterAnimController)target).ConfigureFullFacialRotations();
            }

            if (GUILayout.Button("Show full facial rotations"))
            {
                ((CharacterAnimController)target).ShowFullFacialRotations();
            }
        }
    }
}
/**/