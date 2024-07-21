from Models import VisionModel
from PIL import Image
import torch.amp.autocast_mode
from pathlib import Path
import torch
import torchvision.transforms.functional as TVF
import web
import json
import os
import base64
from io import BytesIO
from log import Log

urls = (
	'/predictTags', 'predictTags',
	'/suggestedTags', 'suggestedTags'
)

dir_path = os.path.dirname(os.path.realpath(__file__))
path = dir_path + '/models'  # Change this to where you downloaded the model
THRESHOLD = 0.4

model = VisionModel.load_model(path)
model.eval()
model = model.to('cpu')

with open(Path(path) / 'top_tags.txt', 'r') as f:
	top_tags = [line.strip() for line in f.readlines() if line.strip()]

if __name__ == "__main__":
    app = web.application(urls, globals())
    app.run(Log)

class suggestedTags:
	def POST(self):
		data = json.loads(web.data())
		searchString = str(data["searchString"])
		limit = int(data["limit"])

		return [i for i in top_tags if i.startswith(searchString)][:limit]

class predictTags:
	def GET(self, name):
		return "true"
	def POST(self):
		data = json.loads(web.data())
		value = str.encode(data["image"])
		image = Image.open(BytesIO(base64.decodebytes(value)))
		tag_string, scores = predict(image)

		result = ''

		for tag, score in sorted(scores.items(), key=lambda x: x[1], reverse=True):
			if score > 0.35:
				result += f'{tag}: {score:.3f}\n'

		return result

def prepare_image(image: Image.Image, target_size: int) -> torch.Tensor:
	# Pad image to square
	image_shape = image.size
	max_dim = max(image_shape)
	pad_left = (max_dim - image_shape[0]) // 2
	pad_top = (max_dim - image_shape[1]) // 2

	padded_image = Image.new('RGB', (max_dim, max_dim), (255, 255, 255))
	padded_image.paste(image, (pad_left, pad_top))

	# Resize image
	if max_dim != target_size:
		padded_image = padded_image.resize((target_size, target_size), Image.BICUBIC)
	
	# Convert to tensor
	image_tensor = TVF.pil_to_tensor(padded_image) / 255.0

	# Normalize
	image_tensor = TVF.normalize(image_tensor, mean=[0.48145466, 0.4578275, 0.40821073], std=[0.26862954, 0.26130258, 0.27577711])

	return image_tensor


@torch.no_grad()
def predict(image: Image.Image):
	image_tensor = prepare_image(image, model.image_size)
	batch = {
		'image': image_tensor.unsqueeze(0).to('cpu'),
	}

	with torch.amp.autocast_mode.autocast('cpu', enabled=True, cache_enabled=False):
		preds = model(batch)
		tag_preds = preds['tags'].sigmoid().cpu()
	
	scores = {top_tags[i]: tag_preds[0][i] for i in range(len(top_tags))}
	predicted_tags = [tag for tag, score in scores.items() if score > THRESHOLD]
	tag_string = ', '.join(predicted_tags)

	return tag_string, scores