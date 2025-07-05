from PIL import Image, PngImagePlugin
import sys

def extract_all_png_metadata(png_path):
    with Image.open(png_path) as img:
        print("=== PIL Image Info ===")
        for k, v in img.info.items():
            print(f"{k}: {v}")

        # If the image is a PNG, try to get all text chunks
        if isinstance(img, PngImagePlugin.PngImageFile):
            print("\n=== PNG Text Chunks ===")
            for chunk, value in img.text.items():
                print(f"{chunk}: {value}")

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python extract_all_png_metadata.py <image.png>")
    else:
        extract_all_png_metadata(sys.argv[1]) 