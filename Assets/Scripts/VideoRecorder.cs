using System;
using System.Threading.Tasks;
using NatML.Recorders;
using NatML.Recorders.Clocks;
using UnityEngine;

public class VideoRecorder : MonoBehaviour
{
    private const string NoBackCameraDetectedText = "Camera device not found.";
    private const string RecodingStartText = "Recording started.";

    public delegate void OnRecordMessage(string message);
    public event OnRecordMessage OnRecordMessageEvent;

    public delegate void OnSetAspectRatioFitter(float width, float height, bool videoVerticallyMirrored, int videoRotationAngle);
    public event OnSetAspectRatioFitter OnSetAspectRatioFitterEvent;

    public delegate void OnSetBackground(WebCamTexture texture);
    public event OnSetBackground OnSetBackgroundEvent;

    private WebCamTexture _webCamTexture;

    private bool _isCameraAvailable;
    public bool IsCameraAvailable => _isCameraAvailable;
    private bool _isVideoRecording = false;
    public bool IsVideoRecording => _isVideoRecording;

    private void Start()
    {
        if (_isCameraAvailable = TryGetCameraDevice(out WebCamDevice device))
        {
            _webCamTexture = new WebCamTexture(device.name, Screen.width, Screen.height);
            _webCamTexture.Play();
            return;
        }

        Log(NoBackCameraDetectedText);
        _isCameraAvailable = false;
    }

    private bool TryGetCameraDevice(out WebCamDevice device)
    {
        device = default;

        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length <= 0)
            return false;

        for (int i = 0; i < devices.Length; i++)
            if (devices[i].isFrontFacing)
            {
                device = devices[i];
                return true;
            }

        return false;
    }

    private void Update()
    {
        if (!_isCameraAvailable)
            return;

        OnSetBackgroundEvent?.Invoke(_webCamTexture);
        OnSetAspectRatioFitterEvent?.Invoke((float)_webCamTexture.width, (float)_webCamTexture.height, _webCamTexture.videoVerticallyMirrored, _webCamTexture.videoRotationAngle);
    }

    public async void StartRecording()
    {
        _isVideoRecording = true;
        Log(RecodingStartText);

        MP4Recorder recorder = new MP4Recorder(_webCamTexture.width, _webCamTexture.height, 30);
        RealtimeClock clock = new RealtimeClock();

        while (_isVideoRecording)
        {
            recorder.CommitFrame(_webCamTexture.GetPixels32(), clock.timestamp);
            await Task.Yield();
        }

        string recordingPath = await recorder.FinishWriting();
        NativeGallery.Permission permission =
            NativeGallery.SaveVideoToGallery(recordingPath, "ARDK", $"ardk-video-{DateTime.Now.ToString("MM-DDTHH:mm:ss")}.mp4",
                (result, path) => Log($"Video save result: {result}. Path: {path}."));
    }

    public void StopRecording()
    {
        _isVideoRecording = false;
    }

    private void Log(string message)
    {
        OnRecordMessageEvent?.Invoke(message);
    }
}
