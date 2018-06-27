using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipController : MonoBehaviour {

    [SerializeField] Text text;

    private void Start()
    {
        HideTooltip();
    }

    public void ShowTooltip(string tooltip)
    {
        text.text = tooltip;
        gameObject.SetActive(true);
        GetComponent<RectTransform>().position = Input.mousePosition;
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        GetComponent<RectTransform>().position = Input.mousePosition;
	}
}
