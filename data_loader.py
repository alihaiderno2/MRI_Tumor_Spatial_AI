import os
import torch

# 1. Verify the CUDA availability first
device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
print(f"Executing on device: {device}")

# 2. Inject Kaggle Credentials Directly (Replace with your actual details)
os.environ['KAGGLE_USERNAME'] = "alihaiderno1"
os.environ['KAGGLE_KEY'] = "KGAT_99d5d7d9b25c28f588262060ac9228d5"

# 3. Download and Extract
if not os.path.exists('lgg-mri-segmentation'):
    print("Downloading LGG Segmentation Dataset...")
    !kaggle datasets download -d mateuszbuda/lgg-mri-segmentation
    !mkdir -p lgg-mri-segmentation
    !unzip -q lgg-mri-segmentation.zip -d lgg-mri-segmentation
    print("Data successfully staged!")
else:
    print("Data environment already configured.")
