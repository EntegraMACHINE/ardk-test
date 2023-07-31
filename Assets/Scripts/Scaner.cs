using System.Collections.Generic;
using System.Threading.Tasks;
using Niantic.ARDK.AR.Scanning;
using Niantic.ARDK.Extensions;
using Niantic.ARDK.Extensions.Scanning;
using Niantic.ARDK.LocationService;
using Niantic.ARDK.VPSCoverage;
using UnityEngine;
using LocationInfo = Niantic.ARDK.LocationService.LocationInfo;
using LocationServiceStatus = Niantic.ARDK.LocationService.LocationServiceStatus;

public class Scaner : MonoBehaviour
{
    private const string StartScaningText = "Scaning started.";
    private const string StopScaningText = "Scaning stoped.";
    private const string StartProcessingText = "Processing started.";
    private const string CancelProcessingText = "Processing stoped.";
    private const string SaveScanText = "Current scan saved.";
    private const string RestartScanText = "Scan manager restarted.";

    public delegate void OnScanProcessing(bool state, float value);
    public event OnScanProcessing OnScanProcessingEvent;

    public delegate void OnScanMessage(string message);
    public event OnScanMessage OnRecordMessagekEvent;

    [SerializeField] private ARScanManager _scanManager;
    [SerializeField] private ARSessionManager _sessionManager;
    [SerializeField] private PointCloudVisualizer _pointCloudVisualizer;

    [SerializeField] private GameObject _scannedObjectPrefab;
    [SerializeField] private Transform _scannedObjectParent;

    private GameObject _scannedObject;

    private IScanTargetClient _scanTargetClient;
    private LocationInfo _locationInfo;
    private bool _isLocationInfoAvailable = false;

    private bool _isScanning = false;
    public bool IsScanning => _isScanning;

    private async void Start()
    {
        _scanManager.SetVisualizer(_pointCloudVisualizer);
        _scanManager.ScanProcessed += ScanResultHandler;

        _isLocationInfoAvailable = await GetLocation();
    }

    private void OnEnable()
    {
        _scanManager.Restart();
    }

    private async Task<bool> GetLocation()
    {
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.CoarseLocation))
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.CoarseLocation);

        ILocationService _locationService = LocationServiceFactory.Create(Niantic.ARDK.RuntimeEnvironment.LiveDevice);
        _locationService.Start();

        int maxWait = 20;
        while (_locationService.Status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            await Task.Delay(1000);
            maxWait--;
        }

        if (maxWait < 1)
        {
            Log("Get location info timeout.");
            return false;
        }

        if (_locationService.Status == LocationServiceStatus.UnknownError ||
            _locationService.Status == LocationServiceStatus.DeviceAccessError ||
            _locationService.Status == LocationServiceStatus.PermissionFailure)
        {
            Log("Unable to determine device location.");
            return false;
        }
        else
        {
            Log($"Location: {_locationService.LastData.Coordinates}.");
            _locationInfo = _locationService.LastData;
        }

        _locationService.Stop();
        return true;
    }

    private void Update()
    {
        IScanner.State state = _scanManager.ScannerState;

        if (state == IScanner.State.Processing)
            OnScanProcessingEvent?.Invoke(true, _scanManager.GetScanProgress());
        else
            OnScanProcessingEvent?.Invoke(false, 0);
    }

    public async void StartScanningButtonPressed()
    {
        _scanTargetClient = ScanTargetClientFactory.Create(Niantic.ARDK.RuntimeEnvironment.LiveDevice);
        ScanTargetResponse response = await _scanTargetClient.RequestScanTargetsAsync(_locationInfo.Coordinates, 2000);

        if (response == null)
        {
            Log("Scan targets response is null.");
            return;
        }

        if (response.status == ResponseStatus.Success)
        {
            _scanManager.SetScanTargetId(response.scanTargets[0].scanTargetIdentifier);
            _scanManager.StartScanning();
            Log($"{StartScaningText} Location: {response.scanTargets[0].name}");
            _isScanning = true;
        }
        else Log($"Scan targets response status: {response.status.ToString()}");
    }

    public void StopScannigButtonPressed()
    {
        _isScanning = false;
        _scanManager.StopScanning();
        Log(StopScaningText);
    }

    public void ProcessButtonPressed()
    {
        IScanner.State state = _scanManager.ScannerState;
        if (state == IScanner.State.ScanCompleted)
        {
            _scanManager.StartProcessing();
            Log(StartProcessingText);
        }
    }

    public void CancelButtonPressed()
    {
        _scanManager.CancelProcessing();
        Log(CancelProcessingText);
    }

    public void SaveButtonPressed()
    {
        _scanManager.SaveCurrentScan();
        Log(SaveScanText);

        string currentScanId = _scanManager.GetScanId();
        SavedScan savedScan = _scanManager.GetSavedScan(currentScanId);

        _scanManager.UploadScan(currentScanId,
            (float progress) => Log(progress.ToString()),
            (bool success, string error) =>
            {
                if (success)
                    Log($"Upload {savedScan.GetScanLocationData()?[0].latitude}, {savedScan.GetScanLocationData()?[0].longitude} success.");
                else
                    Log(error);
            });
    }

    public void RestartButtonPressed()
    {
        _scanManager.Restart();
        Log(RestartScanText);
    }

    private void ScanResultHandler(IScanner.ScanProcessedArgs args)
    {
        TexturedMesh texturedMesh = args.TexturedMesh;

        if (texturedMesh != null)
        {
            if (_scannedObject == null)
            {
                _scannedObject = Instantiate(_scannedObjectPrefab, _scannedObjectParent);
            }
            Bounds meshBoundary = texturedMesh.mesh.bounds;
            _scannedObject.transform.localPosition = -1 * meshBoundary.center;
            _scannedObject.transform.localScale = Vector3.one / meshBoundary.extents.magnitude;
            _scannedObject.GetComponent<MeshFilter>().sharedMesh = texturedMesh.mesh;
            if (texturedMesh.texture != null)
                _scannedObject.GetComponent<Renderer>().material.mainTexture = texturedMesh.texture;
        }
    }

    private void Log(string message)
    {
        OnRecordMessagekEvent?.Invoke(message);
    }
}
