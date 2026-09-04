import os
import glob
import numpy as np
import cv2 as cv
import torch
from torch.utils.data import Dataset, DataLoader

class BrainMRIDataset(Dataset):
    def __init__(self, base_dir, transform=None):
        self.transform = transform
        self.image_paths = []
        self.mask_paths = []

        # Go to the directories to pair image and mask slices
        search_pattern = os.path.join(base_dir, 'lgg-mri-segmentation', 'kaggle_3m', '*', '*_*.tif')
        all_files = glob.glob(search_pattern)

        for file_path in all_files:
            if 'mask' not in file_path:
                mask_path = file_path.replace('.tif', '_mask.tif')
                if os.path.exists(mask_path):
                    self.image_paths.append(file_path)
                    self.mask_paths.append(mask_path)

        print(f"[Dataset] Verified and paired {len(self.image_paths)} slice paths.")

    def __len__(self):
        return len(self.image_paths)

    def __getitem__(self, idx):
        img = cv.imread(self.image_paths[idx])
        img = cv.cvtColor(img, cv.COLOR_BGR2RGB)
        mask = cv.imread(self.mask_paths[idx], cv.IMREAD_GRAYSCALE)

        # Min-Max Normalization
        img = img.astype(np.float32) / 255.0
        mask = mask.astype(np.float32) / 255.0

        if self.transform:
            augmented = self.transform(image=img, mask=mask)
            img, mask = augmented['image'], augmented['mask']

        img = np.transpose(img, (2, 0, 1))
        mask = np.expand_dims(mask, axis=0)

        return torch.tensor(img, dtype=torch.float32), torch.tensor(mask, dtype=torch.float32)

def get_data_loaders(base_dir, batch_size=16, train_ratio=0.8):
    full_dataset = BrainMRIDataset(base_dir)
    train_size = int(train_ratio * len(full_dataset))
    val_size = len(full_dataset) - train_size

    train_dataset, val_dataset = torch.utils.data.random_split(
        full_dataset, [train_size, val_size], generator=torch.Generator().manual_seed(42)
    )

    train_loader = DataLoader(train_dataset, batch_size=batch_size, shuffle=True, num_workers=2, pin_memory=True)
    val_loader = DataLoader(val_dataset, batch_size=batch_size, shuffle=False, num_workers=2, pin_memory=True)

    return train_loader, val_loader
