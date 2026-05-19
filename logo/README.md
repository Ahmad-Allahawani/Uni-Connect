# UniConnect Logo Assets

Brand colors: Indigo #3D52A0 · Violet #7091E6 · Amber #F59E0B · Periwinkle #93B4FF · Dark #0F172A
Font: Plus Jakarta Sans 800 (already loaded in your app)

## Files

### SVG source (master files)
| File | Use |
|---|---|
| `icon.svg` | Master icon — use for any custom export |
| `logo-color.svg` | Full lockup, light backgrounds |
| `logo-dark.svg` | Full lockup, dark backgrounds |
| `logo-monochrome.svg` | Single-color, print/emboss |

### PNG exports
| File | Use |
|---|---|
| `icon-16x16.png` | Browser tab fallback |
| `icon-32x32.png` | Taskbar, small UI |
| `icon-48x48.png` | Sidebar avatar |
| `icon-64x64.png` | Profile card |
| `icon-128x128.png` | App store, email |
| `icon-256x256.png` | High-DPI / retina |
| `icon-512x512.png` | App store hero |
| `logo-color-2x.png` | Full lockup @2x for presentations |
| `logo-dark-2x.png` | Full lockup reversed @2x |

### Browser
| File | Use |
|---|---|
| `favicon.ico` | Multi-size favicon (16/24/32/48/64/128/256) |

### Code
| File | Use |
|---|---|
| `NAVBAR_SNIPPET.cshtml` | Drop-in replacement for _DashboardLayout.cshtml |

---

## How to deploy

### Step 1 — favicon
Copy `favicon.ico` to:
```
Uni-Connect/Uni-Connect/wwwroot/favicon.ico
```
(overwrite the existing one)

### Step 2 — navbar icon
Open `_DashboardLayout.cshtml` and find:
```html
<div class="nav-logo-icon"><i class="ph-bold ph-graduation-cap" ...></i></div>
```
Replace the entire `<div class="nav-logo-icon">` element with the SVG from `NAVBAR_SNIPPET.cshtml`.

### Step 3 — home page (optional)
If your Home/Index.cshtml has a hero logo, use `logo-color.svg` inline or reference `icon-128x128.png`.

### Step 4 — PNG assets folder
Copy all PNG files to:
```
Uni-Connect/Uni-Connect/wwwroot/images/logo/
```
Then reference them as `/images/logo/icon-64x64.png` etc.

---

## Icon meaning

- **Left page** (white, full opacity) — the learner opening the book for help
- **Right page** (white, 40% opacity) — the peer helper; same shape, lighter weight
- **Spine** (dark center bar) — UniConnect itself, holding both students together
- **Amber dot** — the student with the question; amber = your points/rewards system
- **Blue-white dot** — the peer helper in the network
- **Rising lines** — knowledge flowing upward from students into the shared book
