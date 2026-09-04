import torch
import torch.nn as nn
import torch.optim as optim
from tqdm import tqdm
from dataset import get_data_loaders
from model import UNet

# Hybrid Loss function optimized for the image segmentation task
class DiceBCELoss(nn.Module):

  def __init__(self):
    super(DiceBCELoss,self).__init__()
    self.bce = nn.BCEWithLogitsLoss()

  def forward(self,inputs,targets,smooth = 1e-6):

    bce_loss = self.bce(inputs,targets)

    # to convert Logits to probabilities
    inputs = torch.sigmoid(inputs)

    inputs = inputs.view(-1)
    targets = targets.view(-1)

    intersection = (inputs * targets).sum()
    dice_loss = 1 - (2.*intersection + smooth)/(inputs.sum() + targets.sum() + smooth)
    return bce_loss + dice_loss

def train_one_epoch(model,loader,optimizer,loss_fn,device):
  model.train()
  loop = tqdm(loader, desc= "Training")
  epoch_loss = 0

  for batch_idx, (images,masks) in enumerate(loop):
    images = images.to(device)
    masks = masks.to(device)

    # forward pass
    predictions = model(images)
    loss = loss_fn(predictions, masks)

    # backward pass
    optimizer.zero_grad()
    loss.backward()
    optimizer.step()

    epoch_loss += loss.item()
    loop.set_postfix(loss = loss.item())

  return epoch_loss / len(loader)

def validate(model, loader, loss_fn, device):
  model.eval()
  epoch_loss = 0.0

  with torch.no_grad():
      for images, masks in loader:
          images, masks = images.to(device), masks.to(device)
          predictions = model(images)
          loss = loss_fn(predictions, masks)
          epoch_loss += loss.item()

  return epoch_loss / len(loader)


if __name__ == "__main__":

  BATCH_SIZE = 16
  LEARNING_RATE = 3e-4
  EPOCHS = 10
  DEVICE = torch.device("cuda" if torch.cuda.is_available() else "cpu")
  print(f"Launching pipelines on target hardware: {DEVICE}")

  train_loader, val_loader = get_data_loaders(base_dir='.', batch_size=BATCH_SIZE)

  model = UNet(in_channels=3, out_channels=1).to(DEVICE)
  loss_fn = DiceBCELoss()
  optimizer = optim.Adam(model.parameters(), lr=LEARNING_RATE)
  best_loss = float('inf')

  # Main Training Cycle

  for epoch in range(EPOCHS):

    print(f"\n Epoch {epoch+1}/{EPOCHS} ")
    train_loss = train_one_epoch(model, train_loader, optimizer, loss_fn, DEVICE)
    val_loss = validate(model, val_loader, loss_fn, DEVICE)

    print(f"Epoch Summary => Train Loss: {train_loss:.4f} | Validation Loss: {val_loss:.3f}")

    if val_loss < best_loss:
            best_loss = val_loss
            torch.save(model.state_dict(), "best_unet_model.pth")
            print("=> Target parameters updated. Saved 'best_unet_model.pth'")
