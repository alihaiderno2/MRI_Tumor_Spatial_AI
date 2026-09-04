#  AI-Driven Neuro-Oncology Segmentation & Mixed Reality Surgical Spatial Viewer

![Python 3.10+](https://img.shields.io/badge/Python-3.10%2B-blue.svg)![PyTorch](https://img.shields.io/badge/PyTorch-2.0%2B-orange.svg)![Unity Engine](https://img.shields.io/badge/Unity-6.0%20\(6000.0\)-black.svg)![XR Foundation](https://img.shields.io/badge/XR-Foundation%20%2F%20OpenXR-brightgreen.svg)![Medical Precision](https://img.shields.io/badge/Calibration-Sub--Millimeter%20Affine-critical.svg)An end-to-end biomedical engineering pipeline bridging **Deep Learning Semantic Segmentation of Multi-Sequence MRI** with a **Metric-Locked Extended Reality (XR) Holographic Viewer** for preoperative surgical planning.

---

## 🔬 Showcase: Dual-Domain Research Output

| Computer Vision Segmentation Pipeline | Mixed Reality Spatial Interaction Engine |
| --- | --- |
|  |  |
| *Deep U-Net lesion boundary isolation evaluated against ground-truth FLAIR annotations* | *Sub-millimeter co-registered holographic twin in Unity 6 XR workspace* |

---

## Project Overview & Clinical Objective

Translating standard multi-slice 2D MRI scans into actionable 3D spatial awareness remains a significant hurdle in neuro-oncology. This framework solves this limitation across two primary engineering stages:

1. **The Computer Vision Subsystem:** Automated semantic segmentation of Lower-Grade Gliomas (LGG) across multi-planar scans using an optimized U-Net, followed by 3D continuous manifold surface extraction via Marching Cubes.
2. **The Spatial XR Subsystem:** Ingestion of reconstructed geometric boundaries into an interactive Unity XR environment, utilizing deterministic affine matrices to preserve physical millimeter-to-world ratios ($0.686\\text{ mm} \\times 0.686\\text{ mm} \\times 5.000\\text{ mm}$).
3. **Surgeon Telemetry & Inspection:** Real-time dual-sided shader rendering with Quaternion Slerp-damped orbit controls, tissue layer visibility toggles, and live clinical volume telemetry.

---

## 🛠️ System Architecture

```
                              [Raw Patient MRI Scans]
                                         │
           ┌─────────────────────────────┴─────────────────────────────┐
           ▼                                                           ▼
[Computer Vision Subsystem]                                 [Spatial XR Subsystem]
┌──────────────────────────────┐                            ┌──────────────────────────────┐
│ • Min-Max Intensity Scaling  │                            │ • Universal Render Pipeline  │
│ • 4-Tier Deep U-Net Network  │                            │ • Double-Sided Mesh Shader   │
│ • Hybrid BCE-Dice Loss       │                            │ • Sub-cm Near-Clip Camera    │
│ • Marching Cubes (σ = 0.5)   │                            │ • Damped Slerp Orbiting      │
└──────────────┬───────────────┘                            └──────────────┬───────────────┘
               │                                                            │
               │ (.OBJ Polygon Surface)              (Affine Registration) │
               └───────────────────────►      ◄─────────────────────────────┘
                                        │
                                        ▼
                          [Interactive Holographic Workstation]
                          - Millimetric Physical Scale Locking
                          - Independent Tissue Visibility Toggles
                          - Live Volumetric Estimation (cm³)
```

---

##  Mathematical Rigor & Technical Formulation

### 1. Hybrid Objective Optimization (Class-Imbalanced Segmentation)

Lesion sparsity across neuro-imaging datasets induces strong background bias ($\\approx 35%$ positive lesion slices). To optimize boundary precision, our model trains on a joint Binary Cross-Entropy and Dice Loss objective:

$$\\mathcal{L}*{Total} = \\mathcal{L}*{BCE} + \\mathcal{L}\_{Dice}$$

$$\\mathcal{L}*{Total} = -\\frac{1}{N}\\sum*{i=1}^{N} \\left\[ y_i \\log \\hat{y}*i + (1-y_i)\\log(1-\\hat{y}i) \\right\] + \\left(1 - \\frac{2\\sum{i=1}^N y_i\\hat{y}i + \\epsilon}{\\sum{i=1}^N y_i + \\sum*{i=1}^N \\hat{y}\_i + \\epsilon}\\right)$$

Where $\\epsilon = 1 \\times 10^{-6}$ ensures numerical stability.

### 2. Isosurface Boundary Extraction

After inferencing axial 2D probability tensors $\\hat{Y} \\in \\mathbb{R}^{D \\times H \\times W}$, Marching Cubes evaluates the discrete scalar field $\\Phi(x,y,z)$ at an isovalue threshold $\\sigma\_{iso} = 0.5$:

$$\\mathcal{M}\_{iso} = \\left{ (x, y, z) \\in \\mathbb{R}^3 \\mid \\Phi(x, y, z) = 0.5 \\right}$$

Vertex normal vectors $\\vec{N}$ are computed directly across the scalar density gradient:

$$\\vec{N}(x, y, z) = \\nabla \\Phi(x, y, z) = \\left\[ \\frac{\\partial \\Phi}{\\partial x}, \\frac{\\partial \\Phi}{\\partial y}, \\frac{\\partial \\Phi}{\\partial z} \\right\]^T$$

### 3. Metric Affine Registration (DICOM → Unity XR)

Unity defines world space as $1\\text{ Unit} = 1\\text{ Meter}$. Integer voxel coordinates $\[i, j, k\]^T$ are projected into physical coordinates via an affine scale operator derived from DICOM pixel spacing ($\\Delta x, \\Delta y = 0.686\\text{ mm}$) and slice thickness ($\\Delta z = 5.000\\text{ mm}$):

$$\\begin{bmatrix} X\_{world} \\ Y\_{world} \\ Z\_{world} \\end{bmatrix} = \\begin{bmatrix} \\frac{\\Delta x}{1000} & 0 & 0 \\ 0 & \\frac{\\Delta z}{1000} & 0 \\ 0 & 0 & \\frac{\\Delta y}{1000} \\end{bmatrix} \\begin{bmatrix} i \\ k \\ j \\end{bmatrix} = \\begin{bmatrix} 0.000686 \\cdot i \\ 0.005000 \\cdot k \\ 0.000686 \\cdot j \\end{bmatrix}$$

Because our volumetric reconstruction normalizes slice thickness prior to polygon generation, the mesh maintains uniform physical proportions:

$$S\_{uniform} = \\frac{\\Delta x}{1000} = 6.86 \\times 10^{-4}\\text{ m/voxel}$$

---

## 📂 Repository File Structure

```text
MRI_Tumor_Spatial_AI/
├── cv_pipeline/
│   ├── dataset_loader.py                   <-- Multi-slice tensor pairing loader
│   ├── debugging.py                        <-- Debug/inspection utilities
│   ├── loss_functions.py                   <-- Hybrid BCE-Dice loss
│   ├── reconstruct.py                      <-- Marching Cubes mesh exporter (.OBJ)
│   ├── requirements.txt                    <-- Python dependencies
│   ├── train_config.yaml                   <-- Model hyperparameters & paths
│   └── unet.py                             <-- Deep U-Net architecture
│
├── docs/
│   ├── figures/
│   │   ├── marching_cubes_reconstruction.png  <-- Extracted 3D isosurface preview
│   │   ├── pipeline_architecture.png          <-- System architecture diagram
│   │   ├── segmentation_overlay_grid.png      <-- AI inference vs. Ground Truth
│   │   └── unity_spatial_calibration.png      <-- Calibrated holographic view
│   └── methodology.pdf                        <-- Formal research methodology paper
│
└── unity_xr/
    └── Assets/
        ├── Prefabs/
        │   └── Patient_Coordinate_System.prefab <-- Calibrated anatomical template
        └── Scripts/
            ├── MedicalCoordinateCalibrator.cs   <-- Metric locking & affine registration
            ├── SurgeonViewportController.cs     <-- Damped Slerp 3D orbit controls
            └── MedicalUIController.cs           <-- Tissue visibility toggles & telemetry
```

---

## 🚀 Execution & Quickstart Guide

### Phase 1: Computer Vision & Model Inference

```bash
# 1. Navigate to vision workspace
cd cv_pipeline

# 2. Install requirements
pip install -r requirements.txt

# 3. Run 3D surface mesh extraction from MRI volume
python reconstruct.py
```

### Phase 2: Unity XR Engine Setup

1. Open Unity Hub and add the `/unity_xr` project folder (configured for Unity 6.0 LTS).
2. Open the main scene: `Assets/Scenes/AR_Medical_Workspace.unity`.
3. Locate `Patient_Coordinate_System.prefab` in `Assets/Prefabs/` and place it in the hierarchy.
4. Press **Play**:
   - **Right-Click Drag:** Orbit the anatomical volume with smoothed Quaternion damping.
   - **Mouse Scroll:** Optical zoom without near-plane mesh clipping.
   - **UI Overlay:** Toggle cerebral mantle visibility to expose internal lesion anatomy

