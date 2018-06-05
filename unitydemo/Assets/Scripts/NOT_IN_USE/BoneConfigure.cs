using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class BoneConfigure : MonoBehaviour
{
    [SerializeField] bool HandSize;
    [SerializeField] Transform parent;
    [SerializeField] Transform child;

    private void Start()
    {
        parent = transform.parent;
        child = parent.GetChild(0);
    }

    public void Configure()
    {
        transform.position = parent.position / 2f + child.position / 2f;
        transform.LookAt(child);
        transform.Rotate(90f, 0f, 0f, Space.Self);
        float scale = (child.position - parent.position).magnitude / 2f / parent.lossyScale.y;
        if (!HandSize) transform.localScale = new Vector3(transform.localScale.x, scale, transform.localScale.z);
        else transform.localScale = new Vector3(transform.localScale.x / 4f, scale, transform.localScale.z / 4f);
        DestroyImmediate(this);
    }
}

[CustomEditor(typeof(BoneConfigure))]
public class BoneConfigureEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        if (GUILayout.Button("Configure"))
        {
            ((BoneConfigure)target).Configure();
        }
    }
}
