using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace opdemo.examples
{
    public class VisualSpriteController : MonoBehaviour
    {
        [SerializeField] RectTransform Joint0;
        [SerializeField] RectTransform Joint1;
        [SerializeField] float pin = 0.5f;
        [SerializeField] float scaleMultiplier = 1f;

        private Image image { get { return GetComponent<Image>(); } }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Joint0.gameObject.activeSelf && Joint1.gameObject.activeSelf)
            {
                image.enabled = true;
                Vector2 diff = Joint0.position - Joint1.position;
                transform.position = (1f - pin) * Joint0.position + pin * Joint1.position;
                transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg);
                transform.localScale = diff.magnitude * scaleMultiplier * Vector3.one;
            } else
            {
                image.enabled = false;
            }
        }
    }
}
