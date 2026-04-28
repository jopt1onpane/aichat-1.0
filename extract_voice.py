import UnityPy
import os

BUNDLE_PATH = r"c:\Program Files (x86)\Steam\steamapps\common\Chill with You Lo-Fi Story\Chill With You_Data\StreamingAssets\aa\StandaloneWindows64\voice_assets_all_d827360ff935e665fae386e791960457.bundle"
OUTPUT_DIR = r"c:\Program Files (x86)\Steam\steamapps\common\Chill with You Lo-Fi Story\AIChat-1.8.3\AIChat-1.8.3\extracted_voices"

os.makedirs(OUTPUT_DIR, exist_ok=True)

env = UnityPy.load(BUNDLE_PATH)
count = 0

for obj in env.objects:
    if obj.type.name == "AudioClip":
        clip = obj.read()
        for name, data in clip.samples.items():
            out_path = os.path.join(OUTPUT_DIR, name)
            with open(out_path, "wb") as f:
                f.write(data)
            count += 1
            if count <= 5:
                print(f"  extracted: {name} ({len(data)} bytes)")

print(f"\nTotal: {count} voice files extracted to {OUTPUT_DIR}")
