from PIL import Image, ImageDraw
import os

W, H = 780, 620
R = 44

img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
draw = ImageDraw.Draw(img)
draw.rounded_rectangle((0, 0, W - 1, H - 1), radius=R, fill=(255, 255, 255, 255))

out_dir = os.path.join(os.path.dirname(__file__), "..", "UI", "CompleteCard")
os.makedirs(out_dir, exist_ok=True)
out_path = os.path.join(out_dir, "complete_card_panel.png")
img.save(out_path)
print(out_path)
