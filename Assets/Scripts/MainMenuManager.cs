using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private VideoRecorder _videoRecorder;
    [SerializeField] private Scaner _scaner;

    [SerializeField] private GameObject _videoRecorderUI;
    [SerializeField] private GameObject _scanerUI;

    [SerializeField] private Button _toVideoRecorderButton;
    [SerializeField] private Button _toScanButton;

    [SerializeField] private Button _startVideoRecordingButton;
    [SerializeField] private Button _stopVideoRecordingButton;

    [SerializeField] private Button _startScanningButton;
    [SerializeField] private Button _stopScanningButton;
    [SerializeField] private Button _processButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Slider _progressbar;

    [SerializeField] private RawImage _videoRecorderBackground;
    [SerializeField] private AspectRatioFitter _videoRecorderAspectRatioFitter;

    [SerializeField] private Text _videoRecorderLog;
    [SerializeField] private Text _scanerLog;

    private enum AppState
    {
        Record,
        Scan
    }

    private AppState _state;

    private void Update()
    {
        if (_state == AppState.Record) UpdateRecordingUI();
        if (_state == AppState.Scan) UpdateScanUI();
    }

    private void OnEnable()
    {
        _videoRecorder.OnRecordMessageEvent += UpdateVideoRecorderLog;
        _videoRecorder.OnSetAspectRatioFitterEvent += SetVideoRecorderAspectRatioFitter;
        _videoRecorder.OnSetBackgroundEvent += SetVideoRecorderBackgroung;
        _scaner.OnRecordMessagekEvent += UpdateScanerLog;
        _scaner.OnScanProcessingEvent += ShowProcessProgress;

        _toVideoRecorderButton.onClick.AddListener(ToVideoRecorderButtonPressed);
        _toScanButton.onClick.AddListener(ToSacnButtonPressed);

        _startVideoRecordingButton.onClick.AddListener(_videoRecorder.StartRecording);
        _stopVideoRecordingButton.onClick.AddListener(_videoRecorder.StopRecording);

        _startScanningButton.onClick.AddListener(_scaner.StartScanning);
        _stopScanningButton.onClick.AddListener(_scaner.StopScannig);
        _processButton.onClick.AddListener(_scaner.Process);
        _cancelButton.onClick.AddListener(_scaner.Cancel);
        _saveButton.onClick.AddListener(_scaner.Save);

        ToVideoRecorderButtonPressed();
    }

    private void OnDisable()
    {
        _toVideoRecorderButton.onClick.RemoveAllListeners();
        _toScanButton.onClick.RemoveAllListeners();

        _startScanningButton.onClick.RemoveAllListeners();
        _stopScanningButton.onClick.RemoveAllListeners();

        _startScanningButton.onClick.RemoveAllListeners();
        _stopScanningButton.onClick.RemoveAllListeners();
        _processButton.onClick.RemoveAllListeners();
        _cancelButton.onClick.RemoveAllListeners();
        _saveButton.onClick.RemoveAllListeners();

        _videoRecorder.OnRecordMessageEvent -= UpdateVideoRecorderLog;
        _videoRecorder.OnSetAspectRatioFitterEvent -= SetVideoRecorderAspectRatioFitter;
        _videoRecorder.OnSetBackgroundEvent -= SetVideoRecorderBackgroung;
        _scaner.OnRecordMessagekEvent -= UpdateScanerLog;
        _scaner.OnScanProcessingEvent -= ShowProcessProgress;
    }

    private void ToSacnButtonPressed()
    {
        if (_videoRecorder.IsVideoRecording)
            _videoRecorder.StopRecording();

        _videoRecorderUI.SetActive(false);
        _scanerUI.SetActive(true);
        _videoRecorder.gameObject.SetActive(false);
        _scaner.gameObject.SetActive(true);

        _state = AppState.Scan;
    }

    private void ToVideoRecorderButtonPressed()
    {
        _scanerUI.SetActive(false);
        _videoRecorderUI.SetActive(true);
        _scaner.gameObject.SetActive(false);
        _videoRecorder.gameObject.SetActive(true);

        _state = AppState.Record;
    }

    private void UpdateRecordingUI()
    {
        _startVideoRecordingButton.enabled = _videoRecorder.IsCameraAvailable;
        _stopVideoRecordingButton.enabled = _videoRecorder.IsCameraAvailable;

        _stopVideoRecordingButton.gameObject.SetActive(_videoRecorder.IsVideoRecording);
        _startVideoRecordingButton.gameObject.SetActive(!_videoRecorder.IsVideoRecording);
    }

    private void UpdateScanUI()
    {
        _stopScanningButton.gameObject.SetActive(_scaner.IsScanning);
        _startScanningButton.gameObject.SetActive(!_scaner.IsScanning);
    }

    private void ShowProcessProgress(bool state, float value)
    {
        _progressbar.gameObject.SetActive(state);
        _progressbar.value = value;
    }

    private void UpdateVideoRecorderLog(string message)
    {
        _videoRecorderLog.text = message;
    }

    private void UpdateScanerLog(string message)
    {
        _scanerLog.text = message;
    }

    private void SetVideoRecorderAspectRatioFitter(float width, float height, bool videoVerticallyMirrored, int videoRotationAngle)
    {
        float ration = width / height;
        _videoRecorderAspectRatioFitter.aspectRatio = ration;

        float scaleY = videoVerticallyMirrored ? -1f : 1f;
        _videoRecorderBackground.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        int orient = -videoRotationAngle;
        _videoRecorderBackground.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
    }

    private void SetVideoRecorderBackgroung(WebCamTexture texture)
    {
        _videoRecorderBackground.texture = texture;
    }
}
