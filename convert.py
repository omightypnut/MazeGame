import torch
ck = torch.load(r'C:\Users\Josh\MazeGame\results\seeker_v9\Seeker\Seeker-492456.pt')
print(ck['Policy'].keys())