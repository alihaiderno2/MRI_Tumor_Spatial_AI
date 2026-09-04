using UnityEngine;

public class MedicalCoordinateCalibrator : MonoBehaviour
{
    [Header("Anatomical Data Inputs")]
    public GameObject tumorMeshObject;
    public GameObject referenceBrainObject;

    [Header("MRI Spatial Metadata Constants")]
    [Tooltip("Match this to your uniform pixel spacing (in mm)")]
    public float medicalScaleFactor = 0.686f; 

    [Header("Matrix Grid Context")]
    public int matrixWidth = 256;
    public int matrixHeight = 256;
    public int totalSlices = 40;

    void Start()
    {
        ExecuteTrueMedicalAlignment();
    }

    void ExecuteTrueMedicalAlignment()
    {
        if (tumorMeshObject == null || referenceBrainObject == null) return;

        Transform tumorGeometry = tumorMeshObject.transform.Find("default") != null ? tumorMeshObject.transform.Find("default") : tumorMeshObject.transform;
        Transform brainGeometry = referenceBrainObject.transform.Find("default") != null ? referenceBrainObject.transform.Find("default") : referenceBrainObject.transform;

        float targetBrainWidthMeters = (matrixWidth * medicalScaleFactor) / 1000f; 

        MeshFilter brainMf = brainGeometry.GetComponent<MeshFilter>();
        if (brainMf != null && brainMf.sharedMesh != null)
        {
            Vector3 rawBrainSize = brainMf.sharedMesh.bounds.size;

            float uniformBrainScale = targetBrainWidthMeters / rawBrainSize.x;
            referenceBrainObject.transform.localScale = new Vector3(uniformBrainScale, uniformBrainScale, uniformBrainScale);
            referenceBrainObject.transform.localPosition = Vector3.zero;
            brainGeometry.localPosition = Vector3.zero;
        }

        
        float uniformTumorScale = medicalScaleFactor / 1000f;
        tumorMeshObject.transform.localScale = new Vector3(uniformTumorScale, uniformTumorScale, uniformTumorScale);

        tumorMeshObject.transform.localPosition = Vector3.zero;
        tumorGeometry.localPosition = Vector3.zero;
        tumorMeshObject.transform.localRotation = Quaternion.identity;
        tumorGeometry.localRotation = Quaternion.identity;

        AlignTrueGeometricCenters(tumorGeometry, brainGeometry);
        
        PositionWorkspaceInFrontOfARCamera();
    }

    void AlignTrueGeometricCenters(Transform tumorGeo, Transform brainGeo)
    {
        Renderer tumorRenderer = tumorGeo.GetComponent<Renderer>();
        Renderer brainRenderer = brainGeo.GetComponent<Renderer>();

        if (tumorRenderer != null && brainRenderer != null)
        {
            Vector3 brainCenter = brainRenderer.bounds.center;
            Vector3 tumorCenter = tumorRenderer.bounds.center;

            Vector3 offset = brainCenter - tumorCenter;
            tumorMeshObject.transform.position += offset;
            
            Debug.Log("[Medical Calibration] Geometry centers locked. Tumor shape normalized.");
        }
    }

    void PositionWorkspaceInFrontOfARCamera()
    {
        Camera arCamera = Camera.main;
        if (arCamera != null)
        {
            Vector3 spawnPos = arCamera.transform.position + (arCamera.transform.forward * 0.5f);
            transform.position = spawnPos;
            
            transform.LookAt(arCamera.transform.position);
            transform.Rotate(0, 180, 0); 
        }
    }
}