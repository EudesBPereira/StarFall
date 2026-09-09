"""
Converts generated concept images (JPG/PNG on a magenta background) into the final sprites the project expects
(docs/ASSET_SPEC.md): transparent PNG, square canvas, centered, sized for mobile.

Usage:  python convert_images.py <source_root> [<dest_root>]
Default dest: Assets/_Project/Art/Final (relative to the repo root).

Rules:
- Magenta (#FF00FF) background is keyed out with a soft threshold (tolerates JPEG artifacts) and the pink
  spill on edge pixels is removed by un-mixing against the known background colour.
- Already-transparent PNGs keep their alpha. Black-background PNGs (environment renders) are keyed on black.
- 'icon' stays opaque (1024x1024). 'fortresswall' and 'hivewall' are full-canvas tiles (no keying).
- Output is cropped to content, padded to a square with a small margin and downscaled to a per-asset size.
"""
import sys, os
from pathlib import Path
import numpy as np
from PIL import Image

MAGENTA = np.array([255, 0, 255], dtype=np.float32)

# name -> (group folder, output size in px). Sizes = 2x the "canvas sugerido" of ASSET_SPEC.md.
SPEC = {
    # ships
    "ship": ("Ships", 128), "falcon": ("Ships", 128), "titan": ("Ships", 128), "phantom": ("Ships", 128),
    "novax": ("Ships", 144), "symbiont": ("Ships", 128), "nexus": ("Ships", 128),
    "companiondrone": ("Ships", 48), "flame": ("Ships", 48),
    # projectiles / effects
    "projectile": ("Projectiles", 48), "bullet": ("Projectiles", 48), "plasma": ("Projectiles", 64),
    "missile": ("Projectiles", 48), "rail": ("Projectiles", 48), "spore": ("Projectiles", 48), "web": ("Projectiles", 128),
    "dot": ("Projectiles", 96), "spark": ("Projectiles", 48), "circle": ("Items", 160), "ring": ("Projectiles", 160),
    # enemies
    "drone": ("Enemies", 96), "interceptor": ("Enemies", 96), "bomber": ("Enemies", 128), "kamikaze": ("Enemies", 96),
    "shielddrone": ("Enemies", 96), "turret": ("Enemies", 128), "asteroid": ("Enemies", 128),
    # mini-bosses
    "sentinelx": ("Bosses", 320), "widow": ("Bosses", 320), "reaperwing": ("Bosses", 288),
    # bosses
    "ironwarden": ("Bosses", 400), "destroyer": ("Bosses", 480), "bastion": ("Bosses", 480),
    "leviathan_head": ("Bosses", 256), "leviathan_segment": ("Bosses", 128), "corsairqueen": ("Bosses", 400),
    "assembler": ("Bosses", 480), "aetherguardian": ("Bosses", 400), "riftwalker": ("Bosses", 400),
    "hivequeen": ("Bosses", 480), "omegacore": ("Bosses", 480), "omegacore2": ("Bosses", 480), "omegacore3": ("Bosses", 480),
    # parts
    "turretpart": ("Bosses", 80), "fabricator": ("Bosses", 80), "crystal": ("Bosses", 80),
    # ui
    "panel": ("UI", 64), "logo": ("UI", 256), "icon": ("Icon", 1024),
    # environment
    "planet": ("Environment", 512), "colony": ("Environment", 512), "fortresswall": ("Environment", 512),
    "ruins": ("Environment", 512), "rift": ("Environment", 512), "starcore": ("Environment", 512),
    "hivewall": ("Environment", 512), "satellite": ("Environment", 128),
}
OPAQUE = {"icon", "fortresswall", "hivewall"}
# Sprites the game recolours at runtime must be pure white (luminance -> alpha-weighted white).
WHITE = {"dot", "spark", "ring", "flame"}
KEEP_MARGIN = {"panel": 0.0, "dot": 0.02, "ring": 0.02, "starcore": 0.0, "planet": 0.02}
DEFAULT_MARGIN = 0.04


def key_background(rgba: np.ndarray, bg: np.ndarray, lo: float, hi: float) -> np.ndarray:
    """Soft chroma key against a known background colour, with spill removal on partial pixels."""
    rgb = rgba[..., :3].astype(np.float32)
    dist = np.sqrt(((rgb - bg) ** 2).sum(axis=-1))
    alpha = np.clip((dist - lo) / (hi - lo), 0.0, 1.0)
    # un-mix: pixel = a*fg + (1-a)*bg  ->  fg = (pixel - (1-a)*bg) / a
    a3 = alpha[..., None]
    fg = np.where(a3 > 0.02, (rgb - (1.0 - a3) * bg) / np.maximum(a3, 0.02), rgb)
    fg = np.clip(fg, 0, 255)
    out = np.dstack([fg.astype(np.uint8), (alpha * 255).astype(np.uint8)])
    # keep any transparency the source already had
    out[..., 3] = np.minimum(out[..., 3], rgba[..., 3])
    return out


def is_magenta_bg(rgba: np.ndarray) -> bool:
    h, w = rgba.shape[:2]
    corners = np.array([rgba[0, 0, :3], rgba[0, w - 1, :3], rgba[h - 1, 0, :3], rgba[h - 1, w - 1, :3]], dtype=np.float32)
    return bool((np.sqrt(((corners - MAGENTA) ** 2).sum(axis=-1)) < 80).all())


def uniform_corner_bg(rgba: np.ndarray):
    """Returns the mean corner colour when the image is opaque and its four corners agree (any flat backdrop)."""
    if not bool((rgba[..., 3] > 250).all()):
        return None
    h, w = rgba.shape[:2]
    c = np.array([rgba[0, 0, :3], rgba[0, w - 1, :3], rgba[h - 1, 0, :3], rgba[h - 1, w - 1, :3]], dtype=np.float32)
    return c.mean(axis=0) if float(c.std(axis=0).max()) < 12.0 else None


def to_white(rgba: np.ndarray) -> np.ndarray:
    lum = (0.299 * rgba[..., 0] + 0.587 * rgba[..., 1] + 0.114 * rgba[..., 2]) / 255.0
    alpha = (rgba[..., 3].astype(np.float32) / 255.0) * np.clip(lum * 1.15, 0, 1)
    out = np.zeros_like(rgba)
    out[..., :3] = 255
    out[..., 3] = (alpha * 255).astype(np.uint8)
    return out


def is_black_bg(rgba: np.ndarray) -> bool:
    h, w = rgba.shape[:2]
    corners = np.array([rgba[0, 0, :3], rgba[0, w - 1, :3], rgba[h - 1, 0, :3], rgba[h - 1, w - 1, :3]], dtype=np.float32)
    return bool((corners.max(axis=-1) < 24).all()) and bool((rgba[..., 3] > 0).all())


def crop_square(rgba: np.ndarray, margin: float) -> np.ndarray:
    alpha = rgba[..., 3]
    ys, xs = np.where(alpha > 8)
    if len(xs) == 0:
        return rgba
    x0, x1, y0, y1 = xs.min(), xs.max() + 1, ys.min(), ys.max() + 1
    content = rgba[y0:y1, x0:x1]
    ch, cw = content.shape[:2]
    side = int(max(ch, cw) * (1.0 + 2 * margin)) + 2
    canvas = np.zeros((side, side, 4), dtype=np.uint8)
    oy, ox = (side - ch) // 2, (side - cw) // 2
    canvas[oy:oy + ch, ox:ox + cw] = content
    return canvas


def resize(rgba: np.ndarray, size: int) -> Image.Image:
    im = Image.fromarray(rgba, "RGBA")
    if im.width == size:
        return im
    # premultiply before filtering so transparent pixels do not bleed dark fringes
    arr = np.array(im).astype(np.float32)
    a = arr[..., 3:4] / 255.0
    prem = np.dstack([arr[..., :3] * a, arr[..., 3:4]]).astype(np.uint8)
    small = Image.fromarray(prem, "RGBA").resize((size, size), Image.LANCZOS)
    s = np.array(small).astype(np.float32)
    sa = s[..., 3:4] / 255.0
    rgb = np.where(sa > 0.004, s[..., :3] / np.maximum(sa, 0.004), 0)
    return Image.fromarray(np.dstack([np.clip(rgb, 0, 255), s[..., 3:4]]).astype(np.uint8), "RGBA")


def convert(src: Path, dest_root: Path):
    name = src.stem.lower()
    if name not in SPEC:
        return f"SKIP (unknown name) {src}"
    group, size = SPEC[name]
    im = Image.open(src).convert("RGBA")
    rgba = np.array(im)

    if name in OPAQUE:
        out = Image.fromarray(rgba, "RGBA").resize((size, size), Image.LANCZOS)
        if name == "icon":
            out = out.convert("RGB").convert("RGBA")  # force opaque
        mode = "opaque"
    else:
        if is_magenta_bg(rgba):
            keyed = key_background(rgba, MAGENTA, lo=70.0, hi=150.0)
            mode = "magenta"
        elif is_black_bg(rgba):
            keyed = key_background(rgba, np.zeros(3, np.float32), lo=18.0, hi=60.0)
            mode = "black"
        elif (bg := uniform_corner_bg(rgba)) is not None:
            keyed = key_background(rgba, bg, lo=40.0, hi=110.0)
            mode = "corner"
        else:
            keyed = rgba
            mode = "alpha"
        if name in WHITE:
            keyed = to_white(keyed)
        keyed = crop_square(keyed, KEEP_MARGIN.get(name, DEFAULT_MARGIN))
        out = resize(keyed, size)

    dest = dest_root / group / f"{name}.png"
    dest.parent.mkdir(parents=True, exist_ok=True)
    out.save(dest, "PNG", optimize=True)
    return f"{name:18} {mode:8} {im.width}x{im.height} -> {size}x{size}  {dest.relative_to(dest_root)}"


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)
    src_root = Path(sys.argv[1])
    repo = Path(__file__).resolve().parents[2]
    dest_root = Path(sys.argv[2]) if len(sys.argv) > 2 else repo / "Assets" / "_Project" / "Art" / "Final"
    seen = set()
    for src in sorted(src_root.rglob("*")):
        if src.suffix.lower() not in (".jpg", ".jpeg", ".png", ".webp"):
            continue
        name = src.stem.lower()
        if name in seen:
            print(f"DUP  skipped {src} (already converted from another folder)")
            continue
        seen.add(name)
        print(convert(src, dest_root))
    missing = sorted(set(SPEC) - seen)
    print(f"\nconverted: {len(seen)}   missing: {len(missing)} -> {', '.join(missing) if missing else 'none'}")


if __name__ == "__main__":
    main()
