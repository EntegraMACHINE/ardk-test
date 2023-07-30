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

    public delegate void OnCameraEnabled(WebCamTexture texture);
    public event OnCameraEnabled OnCameraEnabledEvent;

    public delegate void OnCameraDisabled();
    public event OnCameraDisabled OnCameraDisabledEvent;

    private WebCamTexture _webCamTexture;

    private bool _isCameraAvailable;
    public bool IsCameraAvailable => _isCameraAvailable;
    private bool _isVideoRecording = false;
    public bool IsVideoRecording => _isVideoRecording;

    private void Start()
    {
        if (InitCamera())
            EnableCamera();
    }

    private bool InitCamera()
    {
        if (_isCameraAvailable = TryGetCameraDevice(out WebCamDevice device))
        {
            _webCamTexture = new WebCamTexture(device.name, Screen.width, Screen.height);
            return true;
        }

        Log(NoBackCameraDetectedText);
        _isCameraAvailable = false;
        return false;
    }

    public void EnableCamera()
    {
        _webCamTexture?.Play();
        OnCameraEnabledEvent?.Invoke(_webCamTexture);
    }

    public void DisableCamera()
    {
        _webCamTexture?.Stop();
        OnCameraDisabledEvent?.Invoke();
    }

    private bool TryGetCameraDevice(out WebCamDevice device)
    {
        device = default;

        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length <= 0)
            return false;

        for (int i = 0; i < devices.Length; i++)
            if (!devices[i].isFrontFacing)
            {
                device = devices[i];
                return true;
            }

        return false;
    }

    public async void StartRecordingButtonPressed()
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

    public void StopRecordingButtonPressed()
    {
        _isVideoRecording = false;
    }

    private void Log(string message)
    {
        OnRecordMessageEvent?.Invoke(message);
    }
}
