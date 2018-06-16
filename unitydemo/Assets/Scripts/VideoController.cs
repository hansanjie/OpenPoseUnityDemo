using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RockVR.Video;

public class VideoController : MonoBehaviour {

    public Color AvaliableColor;
    public Color RecordingColor;
    public Color NotAvaliableColor;

    [SerializeField] Image IndexPointImage;
    [SerializeField] GameObject StartPanel;
    [SerializeField] GameObject RecordingPanel;
    [SerializeField] GameObject ProcessPanel;
    [SerializeField] GameObject PauseText;
    [SerializeField] GameObject ResumeText;

	// Use this for initialization
	void Start () {
        HideAllButtons();
        StartPanel.SetActive(true);
	}

    public void StartRecording()
    {
        VideoCaptureCtrl.instance.StartCapture();
    }

    public void PauseOrResume()
    {
        VideoCaptureCtrl.instance.ToggleCapture();
    }

    public void StopRecording()
    {
        VideoCaptureCtrl.instance.StopCapture();
    }

    private void HideAllButtons()
    {
        StartPanel.SetActive(false);
        RecordingPanel.SetActive(false);
        ProcessPanel.SetActive(false);
    }

    private void SetPauseResumeText(bool paused)
    {
        PauseText.SetActive(!paused);
        ResumeText.SetActive(paused);
    }

    private void UpdateUI()
    {
        switch (VideoCaptureCtrl.instance.status)
        {
            case VideoCaptureCtrlBase.StatusType.NOT_START:
                IndexPointImage.color = AvaliableColor;
                HideAllButtons();
                StartPanel.SetActive(true);
                break;
            case VideoCaptureCtrlBase.StatusType.STARTED:
                IndexPointImage.color = RecordingColor;
                HideAllButtons();
                SetPauseResumeText(false);
                RecordingPanel.SetActive(true);
                break;
            case VideoCaptureCtrlBase.StatusType.PAUSED:
                IndexPointImage.color = NotAvaliableColor;
                HideAllButtons();
                SetPauseResumeText(true);
                RecordingPanel.SetActive(true);
                break;
            case VideoCaptureCtrlBase.StatusType.STOPPED:
                IndexPointImage.color = NotAvaliableColor;
                HideAllButtons();
                ProcessPanel.SetActive(true);
                break;
            case VideoCaptureCtrlBase.StatusType.FINISH:
                IndexPointImage.color = AvaliableColor;
                HideAllButtons();
                StartPanel.SetActive(true);
                break;
            default: break;
        }
    }

    // Update is called once per frame
    void Update () {
        UpdateUI();
	}
}
