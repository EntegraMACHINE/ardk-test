using Niantic.ARDK.AR.Scanning;
using Niantic.ARDK.Extensions.Scanning;
using UnityEngine;

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
    [SerializeField] private PointCloudVisualizer _pointCloudVisualizer;

    [SerializeField] private GameObject _scannedObjectPrefab;
    [SerializeField] private Transform _scannedObjectParent;

    private GameObject scannedObject;

    private bool _isScanning = false;
    public bool IsScanning => _isScanning;

    private void Start()
    {
        _scanManager.SetVisualizer(_pointCloudVisualizer);
        _scanManager.ScanProcessed += ScanResultHandler;
    }

    private void Update()
    {
        IScanner.State state = _scanManager.ScannerState;

        if (state == IScanner.State.Processing)
            OnScanProcessingEvent?.Invoke(true, _scanManager.GetScanProgress());
        else
            OnScanProcessingEvent?.Invoke(false, 0);
    }

    public void StartScanningButtonPressed()
    {
        _isScanning = true;
        _scanManager.StartScanning();
        Log(StartScaningText);
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
            if (scannedObject == null)
            {
                scannedObject = Instantiate(_scannedObjectPrefab, _scannedObjectParent);
            }
            Bounds meshBoundary = texturedMesh.mesh.bounds;
            scannedObject.transform.localPosition = -1 * meshBoundary.center;
            scannedObject.transform.localScale = Vector3.one / meshBoundary.extents.magnitude;
            scannedObject.GetComponent<MeshFilter>().sharedMesh = texturedMesh.mesh;
            if (texturedMesh.texture != null)
                scannedObject.GetComponent<Renderer>().material.mainTexture = texturedMesh.texture;
        }
    }

    private void Log(string message)
    {
        OnRecordMessagekEvent?.Invoke(message);
    }
}
