from dataset import BrainMRIDataset

dataset = BrainMRIDataset(".")

positive = 0
negative = 0

for i in range(len(dataset)):
    _, mask = dataset[i]

    if mask.sum() > 0:
        positive += 1
    else:
        negative += 1

print(f"Positive slices: {positive}")
print(f"Negative slices: {negative}")
print(f"Positive ratio: {positive/(positive+negative):.2%}")
