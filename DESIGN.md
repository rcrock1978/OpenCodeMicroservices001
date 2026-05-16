# DESIGN.md — Ecommerce Application
> Design System & UI Specification · Version 1.0

---

## Table of Contents

1. [Design Philosophy](#1-design-philosophy)
2. [Brand Identity](#2-brand-identity)
3. [Color System](#3-color-system)
4. [Typography](#4-typography)
5. [Spacing & Grid](#5-spacing--grid)
6. [Component Library](#6-component-library)
7. [Page Layouts](#7-page-layouts)
8. [Microservices UI Contracts](#8-microservices-ui-contracts)
9. [Responsive Breakpoints](#9-responsive-breakpoints)
10. [Motion & Animation](#10-motion--animation)
11. [Accessibility](#11-accessibility)
12. [Dark Mode](#12-dark-mode)
13. [Icon System](#13-icon-system)
14. [State Management UI Patterns](#14-state-management-ui-patterns)

---

## 1. Design Philosophy

### Vision
**"Effortless Commerce"** — every interaction should feel inevitable. No friction. No confusion. The UI disappears and only the product remains.

### Core Principles

| Principle | Description |
|-----------|-------------|
| **Clarity first** | Information hierarchy is always obvious at a glance |
| **Speed perceived** | Skeleton loaders, optimistic UI, and instant feedback make the app feel instant |
| **Trust by design** | Security badges, clear pricing, and honest error messages build buyer confidence |
| **Progressive disclosure** | Show what's needed now; reveal complexity on demand |
| **Mobile-led** | Design for the smallest screen first; scale up, never down |

### Aesthetic Direction
**Refined Luxury Minimal** — clean white space, precise typography, warm neutrals with a single bold accent. Inspired by premium retail brands (SSENSE, Mr Porter). No gradients on core UI. No decorative clutter. Every pixel earns its place.

---

## 2. Brand Identity

### Logo Usage

```
Primary Logo:    [LOGO MARK] + Wordmark (horizontal)
Icon Only:       [LOGO MARK] — for favicons, app icons, 32px and below
Wordmark Only:   For co-branded contexts
```

### Logo Clear Space
Minimum clear space = **1× the height of the logo mark** on all sides.

### Logo Don'ts
- Do not rotate, skew, or distort
- Do not apply drop shadows
- Do not change logo colors outside approved palette
- Do not place on busy backgrounds without a solid backing

---

## 3. Color System

### Design Tokens (CSS Custom Properties)

```css
:root {
  /* ─── Brand ─────────────────────────────── */
  --color-accent:          #C8956C;   /* Warm terracotta — primary CTA */
  --color-accent-hover:    #B37D56;
  --color-accent-subtle:   #F5EDE6;   /* Tint for badges, highlights */

  /* ─── Neutrals ───────────────────────────── */
  --color-ink-900:         #1A1714;   /* Primary text */
  --color-ink-700:         #4A4540;   /* Secondary text */
  --color-ink-500:         #8A8480;   /* Placeholder, disabled */
  --color-ink-300:         #C4C0BC;   /* Borders, dividers */
  --color-ink-100:         #F0EDEA;   /* Surface, inputs */
  --color-ink-50:          #FAF8F6;   /* Page background */

  /* ─── Semantic ────────────────────────────── */
  --color-success:         #2D7A4F;
  --color-success-subtle:  #EAF5EF;
  --color-warning:         #A05C00;
  --color-warning-subtle:  #FEF3E2;
  --color-danger:          #C0392B;
  --color-danger-subtle:   #FDECEA;
  --color-info:            #1A5FA8;
  --color-info-subtle:     #E8F1FB;

  /* ─── Surface ─────────────────────────────── */
  --color-surface:         #FFFFFF;
  --color-surface-raised:  #FFFFFF;   /* Cards with elevation */
  --color-surface-overlay: rgba(26, 23, 20, 0.55); /* Modals */
  --color-page-bg:         #FAF8F6;
}
```

### Color Usage Rules

| Token | Use |
|-------|-----|
| `--color-accent` | Primary CTA buttons, links, active states, selected indicators |
| `--color-accent-subtle` | Tag backgrounds, promo banners, hover tints |
| `--color-ink-900` | H1, H2, product names, prices |
| `--color-ink-700` | Body copy, descriptions, labels |
| `--color-ink-500` | Placeholder text, meta info, helper text |
| `--color-ink-300` | Input borders (default), dividers, card borders |
| `--color-ink-100` | Input backgrounds, skeleton loaders |
| `--color-ink-50` | Page background, section backgrounds |

### Palette — Do Not Invent New Colors
All UI colors must use design tokens. No hardcoded hex values in components. If a new color is needed, add it to the token system first and document the rationale.

---

## 4. Typography

### Typeface Selections

```
Display / Headings:   "Playfair Display" — Serif, editorial, premium
Body / UI:            "DM Sans" — Humanist sans-serif, readable at small sizes
Mono (prices, codes): "JetBrains Mono" — for SKUs, order numbers, code
```

**Google Fonts import:**
```html
<link rel="preconnect" href="https://fonts.googleapis.com">
<link href="https://fonts.googleapis.com/css2?family=Playfair+Display:wght@400;600&family=DM+Sans:wght@300;400;500;600&family=JetBrains+Mono:wght@400;500&display=swap" rel="stylesheet">
```

### Type Scale

```css
:root {
  /* ─── Display ─────────────────────────────── */
  --text-display-2xl:  clamp(2.5rem, 5vw, 4rem);      /* Hero headlines */
  --text-display-xl:   clamp(2rem, 4vw, 3rem);         /* Section heroes */
  --text-display-lg:   clamp(1.5rem, 3vw, 2.25rem);    /* Page titles */

  /* ─── Headings ────────────────────────────── */
  --text-h1:           1.875rem;   /* 30px */
  --text-h2:           1.5rem;     /* 24px */
  --text-h3:           1.25rem;    /* 20px */
  --text-h4:           1.125rem;   /* 18px */
  --text-h5:           1rem;       /* 16px */

  /* ─── Body ────────────────────────────────── */
  --text-body-lg:      1.125rem;   /* 18px — lead paragraphs */
  --text-body:         1rem;       /* 16px — standard body */
  --text-body-sm:      0.875rem;   /* 14px — secondary body */

  /* ─── UI ──────────────────────────────────── */
  --text-label:        0.875rem;   /* 14px — form labels */
  --text-caption:      0.75rem;    /* 12px — captions, meta */
  --text-overline:     0.6875rem;  /* 11px — uppercase categories */

  /* ─── Font weights ────────────────────────── */
  --fw-light:   300;
  --fw-regular: 400;
  --fw-medium:  500;
  --fw-semi:    600;

  /* ─── Line heights ────────────────────────── */
  --lh-tight:  1.2;
  --lh-snug:   1.35;
  --lh-normal: 1.5;
  --lh-relaxed:1.7;

  /* ─── Letter spacing ──────────────────────── */
  --ls-tight:    -0.02em;
  --ls-normal:    0;
  --ls-wide:      0.04em;
  --ls-widest:    0.12em;  /* For OVERLINE labels */
}
```

### Typography Usage Guide

| Element | Font | Size | Weight | Line Height | Letter Spacing |
|---------|------|------|--------|-------------|----------------|
| Hero headline | Playfair Display | `display-2xl` | 600 | tight | -0.02em |
| Page title | Playfair Display | `h1` | 600 | snug | -0.01em |
| Product name | DM Sans | `h3` | 500 | snug | 0 |
| Price | DM Sans | `h3` | 600 | tight | -0.01em |
| Body copy | DM Sans | `body` | 400 | relaxed | 0 |
| Button label | DM Sans | `body-sm` | 600 | tight | 0.02em |
| Category label | DM Sans | `overline` | 500 | normal | 0.12em |
| SKU / Order ID | JetBrains Mono | `caption` | 400 | normal | 0 |
| Form label | DM Sans | `label` | 500 | normal | 0 |
| Helper text | DM Sans | `caption` | 400 | normal | 0 |

---

## 5. Spacing & Grid

### Base Unit
**4px** is the atomic unit. All spacing values must be multiples of 4.

```css
:root {
  --space-1:    4px;
  --space-2:    8px;
  --space-3:    12px;
  --space-4:    16px;
  --space-5:    20px;
  --space-6:    24px;
  --space-8:    32px;
  --space-10:   40px;
  --space-12:   48px;
  --space-16:   64px;
  --space-20:   80px;
  --space-24:   96px;
  --space-32:   128px;
}
```

### Layout Grid

```
Mobile (< 640px):
  Columns:  4
  Gutter:   16px
  Margin:   16px

Tablet (640px – 1024px):
  Columns:  8
  Gutter:   24px
  Margin:   32px

Desktop (1024px – 1440px):
  Columns:  12
  Gutter:   32px
  Margin:   48px
  Max content width: 1280px

Wide (> 1440px):
  Max content width: 1440px
  Content centered, margins grow
```

### Component Spacing Patterns

```
Card internal padding:    var(--space-6)  (24px)
Section vertical gap:     var(--space-20) (80px) desktop / var(--space-12) (48px) mobile
Form field gap:           var(--space-4)  (16px)
Button internal h-padding:var(--space-6)  (24px)
Button internal v-padding:var(--space-3)  (12px)
Nav height:               64px desktop / 56px mobile
```

### Border Radius

```css
:root {
  --radius-sm:   4px;    /* Tags, badges */
  --radius-md:   8px;    /* Inputs, small cards */
  --radius-lg:   12px;   /* Cards, modals */
  --radius-xl:   16px;   /* Large cards, drawers */
  --radius-full: 9999px; /* Pills, avatar circles */
}
```

---

## 6. Component Library

### 6.1 Buttons

#### Variants

```
Primary     — Filled accent background. Main CTA.
Secondary   — Outlined with ink border. Alternative action.
Ghost       — No border, ink text. Low-emphasis action.
Danger      — Filled danger background. Destructive actions.
```

#### Sizes

```
xs  — height: 28px · text: 12px · px: 12px
sm  — height: 36px · text: 14px · px: 16px
md  — height: 44px · text: 14px · px: 24px  (default)
lg  — height: 52px · text: 16px · px: 32px
xl  — height: 60px · text: 16px · px: 40px
```

#### States

```
Default    — base style
Hover      — background darkens 8%, cursor: pointer
Active     — background darkens 15%, scale: 0.98
Focus      — 2px outline, offset 2px, color: accent
Disabled   — opacity: 0.4, cursor: not-allowed
Loading    — spinner replaces text, pointer-events: none
```

#### CSS Specification

```css
.btn {
  display:         inline-flex;
  align-items:     center;
  justify-content: center;
  gap:             var(--space-2);
  font-family:     var(--font-sans);
  font-weight:     var(--fw-semi);
  letter-spacing:  0.02em;
  border-radius:   var(--radius-md);
  border:          1.5px solid transparent;
  cursor:          pointer;
  transition:      background 150ms ease, color 150ms ease,
                   border-color 150ms ease, transform 100ms ease,
                   opacity 150ms ease;
  white-space:     nowrap;
  text-decoration: none;
}

.btn-primary {
  background:  var(--color-accent);
  color:       #FFFFFF;
  border-color:var(--color-accent);
}
.btn-primary:hover  { background: var(--color-accent-hover); border-color: var(--color-accent-hover); }
.btn-primary:active { transform: scale(0.98); }

.btn-secondary {
  background:   transparent;
  color:        var(--color-ink-900);
  border-color: var(--color-ink-300);
}
.btn-secondary:hover { border-color: var(--color-ink-700); background: var(--color-ink-50); }

.btn-ghost {
  background:   transparent;
  color:        var(--color-ink-700);
  border-color: transparent;
}
.btn-ghost:hover { background: var(--color-ink-100); color: var(--color-ink-900); }

.btn:disabled,
.btn[aria-disabled="true"] {
  opacity: 0.4;
  pointer-events: none;
}
```

---

### 6.2 Product Card

The product card is the most repeated component. It must be pixel-perfect.

#### Anatomy

```
┌─────────────────────────┐
│                         │
│      Product Image      │  aspect-ratio: 4/5
│      (cover fit)        │
│                         │
│  [BADGE]  [♡ Wishlist]  │
├─────────────────────────┤
│  Category Label         │  overline · ink-500
│  Product Name           │  h5 · ink-900 · 2 lines max
│  Brand Name             │  caption · ink-500
│                         │
│  $129.00  ~~$180.00~~   │  price · accent | strikethrough · ink-300
│  ★★★★☆ (124)           │  caption · ink-500
└─────────────────────────┘
```

#### Interaction States

```
Default:      Card shadow: 0 1px 3px rgba(0,0,0,0.06)
Hover:        Image scale: 1.04, shadow: 0 8px 24px rgba(0,0,0,0.10)
              Quick-add button slides up from bottom
Active:       Image scale: 1.02
Focus-within: 2px accent outline on card
```

#### CSS

```css
.product-card {
  position:      relative;
  border-radius: var(--radius-lg);
  background:    var(--color-surface);
  overflow:      hidden;
  transition:    box-shadow 200ms ease, transform 200ms ease;
  box-shadow:    0 1px 3px rgba(0, 0, 0, 0.06);
}
.product-card:hover {
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.10);
}
.product-card__image-wrap {
  aspect-ratio: 4 / 5;
  overflow:     hidden;
  background:   var(--color-ink-100);
}
.product-card__image {
  width:      100%;
  height:     100%;
  object-fit: cover;
  transition: transform 350ms cubic-bezier(0.25, 0.46, 0.45, 0.94);
}
.product-card:hover .product-card__image { transform: scale(1.04); }

.product-card__body {
  padding: var(--space-4) var(--space-4) var(--space-5);
}
.product-card__category {
  font-size:      var(--text-overline);
  font-weight:    var(--fw-medium);
  letter-spacing: var(--ls-widest);
  text-transform: uppercase;
  color:          var(--color-ink-500);
  margin-bottom:  var(--space-1);
}
.product-card__name {
  font-size:    var(--text-h5);
  font-weight:  var(--fw-medium);
  color:        var(--color-ink-900);
  line-height:  var(--lh-snug);
  display:      -webkit-box;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
  overflow:     hidden;
}
.product-card__price {
  font-size:   var(--text-h4);
  font-weight: var(--fw-semi);
  color:       var(--color-ink-900);
}
.product-card__price-original {
  font-size:       var(--text-body-sm);
  color:           var(--color-ink-500);
  text-decoration: line-through;
  margin-left:     var(--space-2);
}
.product-card__price-sale { color: var(--color-danger); }
```

---

### 6.3 Badges & Tags

```css
.badge {
  display:        inline-flex;
  align-items:    center;
  gap:            var(--space-1);
  padding:        2px var(--space-2);
  font-size:      var(--text-caption);
  font-weight:    var(--fw-medium);
  letter-spacing: 0.03em;
  border-radius:  var(--radius-sm);
  white-space:    nowrap;
}

/* Variants */
.badge-sale    { background: var(--color-danger);         color: #FFFFFF; }
.badge-new     { background: var(--color-ink-900);        color: #FFFFFF; }
.badge-promo   { background: var(--color-accent);         color: #FFFFFF; }
.badge-soldout { background: var(--color-ink-100);        color: var(--color-ink-500); }
.badge-limited { background: var(--color-warning-subtle); color: var(--color-warning); }
```

---

### 6.4 Form Inputs

```css
.input {
  width:         100%;
  height:        44px;
  padding:       0 var(--space-4);
  background:    var(--color-surface);
  border:        1.5px solid var(--color-ink-300);
  border-radius: var(--radius-md);
  font-family:   var(--font-sans);
  font-size:     var(--text-body);
  color:         var(--color-ink-900);
  transition:    border-color 150ms ease, box-shadow 150ms ease;
  outline:       none;
}
.input::placeholder { color: var(--color-ink-500); }
.input:hover  { border-color: var(--color-ink-500); }
.input:focus  {
  border-color: var(--color-accent);
  box-shadow:   0 0 0 3px var(--color-accent-subtle);
}
.input.error  {
  border-color: var(--color-danger);
  box-shadow:   0 0 0 3px var(--color-danger-subtle);
}
.input:disabled {
  background: var(--color-ink-100);
  color:      var(--color-ink-500);
  cursor:     not-allowed;
}

/* Textarea */
.textarea {
  /* extends .input */
  height:     auto;
  min-height: 120px;
  padding:    var(--space-3) var(--space-4);
  resize:     vertical;
}

/* Select */
.select {
  /* extends .input */
  padding-right: var(--space-10);
  appearance:    none;
  background-image: url("data:image/svg+xml,..."); /* chevron icon */
  background-repeat:   no-repeat;
  background-position: right var(--space-4) center;
  cursor:        pointer;
}
```

#### Form Label Pattern

```html
<div class="form-field">
  <label class="form-label" for="email">
    Email address
    <span class="form-required" aria-label="required">*</span>
  </label>
  <input class="input" id="email" type="email" placeholder="you@example.com">
  <p class="form-helper">We'll send your order confirmation here.</p>
  <p class="form-error" role="alert">Please enter a valid email address.</p>
</div>
```

---

### 6.5 Navigation

#### Desktop Top Nav

```
┌────────────────────────────────────────────────────────────────────┐
│  [LOGO]    Shop  Collections  Sale  About     🔍  ♡  👤  🛒 (3)   │
└────────────────────────────────────────────────────────────────────┘
```

- Height: 64px
- Background: white with `border-bottom: 1px solid var(--color-ink-100)`
- Sticky on scroll with `backdrop-filter: blur(12px)` + subtle transparency
- Active nav item: accent color, 2px bottom border
- Cart icon shows item count badge (accent background)

#### Mobile Nav

```
┌──────────────────────┐
│  [LOGO]     🔍  🛒   │  Top bar: 56px
└──────────────────────┘

Bottom tab bar (fixed):
┌─────────────────────────────────────┐
│  🏠 Home  🔍 Search  ♡ Saved  👤 Account │
└─────────────────────────────────────┘
```

#### Mega Menu (Desktop)

```
Hover "Shop" reveals a full-width dropdown:

┌────────────────────────────────────────────────────────┐
│ WOMEN              MEN               KIDS              │
│ ──────────         ──────────        ──────────        │
│ Tops               Tops              Boys              │
│ Bottoms            Bottoms           Girls             │
│ Dresses            Outerwear         Shoes             │
│ Shoes              Shoes             Accessories       │
│                                                        │
│ ┌─────────────────────────────────────────┐           │
│ │  FEATURED: New Summer Collection →      │           │
│ └─────────────────────────────────────────┘           │
└────────────────────────────────────────────────────────┘
```

---

### 6.6 Cart Drawer

Slides in from the right (360px wide on desktop, full-width on mobile).

```
┌─────────────────────────────────────┐
│  Your Cart (3)              [✕ Close]│
├─────────────────────────────────────┤
│ [IMG]  Product Name                 │
│        Size: M  Color: Navy         │
│        Qty: [−] 1 [+]    $129.00   │
│                           [Remove]  │
├─────────────────────────────────────┤
│ [IMG]  Product Name                 │
│        ...                          │
├─────────────────────────────────────┤
│ ─────── ORDER SUMMARY ───────       │
│ Subtotal            $258.00         │
│ Shipping            FREE            │
│ Estimated tax       $20.64          │
│                     ──────          │
│ Total               $278.64         │
│                                     │
│ [    Proceed to Checkout    ]  CTA  │
│ [Continue Shopping]          ghost  │
└─────────────────────────────────────┘
```

---

### 6.7 Toast Notifications

```
Position: bottom-right, stacked, 16px gap
Width: 360px (desktop) / calc(100% - 32px) (mobile)
Duration: 4s auto-dismiss (error: stays until dismissed)
```

```css
.toast {
  display:       flex;
  align-items:   flex-start;
  gap:           var(--space-3);
  padding:       var(--space-4);
  border-radius: var(--radius-lg);
  box-shadow:    0 8px 24px rgba(0, 0, 0, 0.12);
  background:    var(--color-surface);
  border-left:   4px solid;
  font-size:     var(--text-body-sm);
  animation:     toast-in 250ms cubic-bezier(0.34, 1.56, 0.64, 1);
}
.toast-success { border-color: var(--color-success); }
.toast-error   { border-color: var(--color-danger); }
.toast-info    { border-color: var(--color-info); }
.toast-warning { border-color: var(--color-warning); }
```

---

### 6.8 Skeleton Loaders

```css
@keyframes shimmer {
  0%   { background-position: -600px 0; }
  100% { background-position:  600px 0; }
}
.skeleton {
  background: linear-gradient(
    90deg,
    var(--color-ink-100) 25%,
    var(--color-ink-50)  50%,
    var(--color-ink-100) 75%
  );
  background-size: 1200px 100%;
  animation: shimmer 1.5s infinite;
  border-radius: var(--radius-sm);
}
```

Use for: product cards, hero sections, navigation, product detail images. All skeleton shapes must mirror the actual content's dimensions exactly.

---

### 6.9 Rating Stars

```
Full star:    ★  color: #F5A623
Empty star:   ☆  color: var(--color-ink-300)
Half star:    ½  custom SVG clip
```

Always accompany with numeric rating and review count in accessible text:
```html
<div class="rating" role="img" aria-label="4.5 out of 5 stars, 124 reviews">
  ...
  <span class="rating-count">(124)</span>
</div>
```

---

## 7. Page Layouts

### 7.1 Homepage

```
┌────────────────────────────────────────────┐
│               NAVIGATION                   │
├────────────────────────────────────────────┤
│                                            │
│              HERO SECTION                  │
│    Full-width image / video background     │
│    Headline · Subtext · CTA Button         │
│    min-height: 85vh                        │
│                                            │
├────────────────────────────────────────────┤
│   CATEGORY QUICK-LINKS  (horizontal scroll)│
│   [Women] [Men] [Kids] [Sale] [New In]     │
├────────────────────────────────────────────┤
│                                            │
│   FEATURED COLLECTION                      │
│   Section heading + 4-column product grid  │
│                                            │
├────────────────────────────────────────────┤
│   EDITORIAL BANNER  (full-bleed image)     │
│   Text overlay + CTA                       │
├────────────────────────────────────────────┤
│   TRENDING NOW                             │
│   Horizontally scrollable card row         │
├────────────────────────────────────────────┤
│   TESTIMONIALS / TRUST SIGNALS             │
│   ★ Reviews · Free Returns · Secure Pay   │
├────────────────────────────────────────────┤
│               FOOTER                       │
└────────────────────────────────────────────┘
```

### 7.2 Product Listing Page (PLP)

```
┌────────────────────┬───────────────────────┐
│  FILTERS (left)    │   PRODUCT GRID        │
│                    │                       │
│  Category          │  ┌───┐ ┌───┐ ┌───┐  │
│  Size              │  │   │ │   │ │   │  │
│  Color             │  └───┘ └───┘ └───┘  │
│  Price range       │  ┌───┐ ┌───┐ ┌───┐  │
│  Brand             │  │   │ │   │ │   │  │
│  Rating            │  └───┘ └───┘ └───┘  │
│                    │                       │
│  [Clear filters]   │  [Load More / Paginate│
└────────────────────┴───────────────────────┘

Filter panel: 260px fixed left, collapsible on tablet
Grid columns: 4 (desktop) / 2 (tablet+) / 1 (mobile)
Sort bar: Relevance · Price · Rating · Newest
Active filters: shown as removable chips above grid
```

### 7.3 Product Detail Page (PDP)

```
┌───────────────────────────────────────────────────┐
│  Breadcrumb: Home > Women > Tops > Product Name   │
├──────────────────────┬────────────────────────────┤
│                      │  Brand Name                │
│   IMAGE GALLERY      │  Product Name              │
│   [Main Image]       │  ★★★★☆ 124 reviews        │
│                      │                            │
│   [Thumbnails row]   │  $129.00  ~~$180.00~~      │
│                      │                            │
│                      │  Color: Navy               │
│                      │  [○] [●] [○] [○]           │
│                      │                            │
│                      │  Size: M  [Size Guide →]   │
│                      │  [XS][S][M][L][XL][XXL]    │
│                      │                            │
│                      │  Quantity: [−] 1 [+]       │
│                      │                            │
│                      │  [  Add to Cart  ]  CTA    │
│                      │  [♡ Add to Wishlist] ghost │
│                      │                            │
│                      │  ✓ Free shipping over $100 │
│                      │  ✓ Free returns · 30 days  │
│                      │  ✓ Secure checkout         │
├──────────────────────┴────────────────────────────┤
│  [Description] [Details] [Reviews] [Shipping]     │
│  Tabbed content section                           │
├───────────────────────────────────────────────────┤
│  YOU MAY ALSO LIKE  —  related products carousel  │
└───────────────────────────────────────────────────┘
```

### 7.4 Checkout Flow

```
Step indicator: [1 Cart] → [2 Shipping] → [3 Payment] → [4 Confirm]

Layout: 2-column (form left 55% / order summary right 45%)
On mobile: summary collapses to top accordion

Step 1 — Cart Review
Step 2 — Shipping Address + Method
Step 3 — Payment (Card / Apple Pay / PayPal)
Step 4 — Order Confirmation (success screen + email prompt)
```

### 7.5 User Account Dashboard

```
┌──────────────────┬────────────────────────────────┐
│  SIDEBAR         │  MAIN CONTENT                  │
│                  │                                │
│  → Orders        │  Order history table           │
│  → Wishlist      │  Filters: All / Processing /   │
│  → Addresses     │          Shipped / Delivered   │
│  → Payment       │                                │
│  → Profile       │                                │
│  → Settings      │                                │
│  ─────────       │                                │
│  Sign Out        │                                │
└──────────────────┴────────────────────────────────┘

Sidebar: 220px, sticky; collapses to horizontal tabs on mobile
```

---

## 8. Microservices UI Contracts

Each frontend feature maps to a backend microservice. The UI must gracefully handle each service's failure mode.

### Service → UI Mapping

| Microservice | UI Surface | Failure UI |
|---|---|---|
| **Product Catalogue** | PLP grid, PDP, search results | Skeleton → "Unable to load products. Try again." |
| **Inventory** | Stock status badge, size availability | Show sizes, grey-out unavailable, no hard block |
| **Pricing** | Price display, sale badges | Show last known price, flag as "price may vary" |
| **Cart** | Cart drawer, mini-cart count | Persist local cart, sync on reconnect |
| **User Auth** | Login modal, account dashboard | Guest mode fallback, no hard redirects |
| **Orders** | Order history, order detail | "Orders temporarily unavailable" empty state |
| **Reviews** | Star rating, review tab | Hide rating if unavailable, no broken stars |
| **Search** | Search dropdown, results page | Fallback to catalogue browse |
| **Recommendations** | "You may also like" carousel | Hide section silently — never show empty |
| **Payments** | Checkout payment step | Clear error, retry prompt, alternative methods |
| **Shipping** | Shipping method selector | Default to standard; notify user |
| **Promotions** | Promo banner, coupon field | Silently skip if promo service down |

### API Response Shape (Standard)

```typescript
interface ApiResponse<T> {
  data:    T | null;
  error:   ApiError | null;
  meta: {
    requestId:  string;
    timestamp:  string;
    pagination?: {
      page:       number;
      perPage:    number;
      total:      number;
      totalPages: number;
    };
  };
}

interface ApiError {
  code:    string;   // e.g. "PRODUCT_NOT_FOUND"
  message: string;   // Human-readable
  field?:  string;   // For form validation errors
}
```

---

## 9. Responsive Breakpoints

```css
/* Mobile-first breakpoints */
:root {
  --bp-sm:  640px;   /* Phablet+ */
  --bp-md:  768px;   /* Tablet portrait */
  --bp-lg:  1024px;  /* Tablet landscape / small desktop */
  --bp-xl:  1280px;  /* Desktop */
  --bp-2xl: 1536px;  /* Wide desktop */
}

/* Usage */
@media (min-width: 640px)  { /* sm */ }
@media (min-width: 768px)  { /* md */ }
@media (min-width: 1024px) { /* lg */ }
@media (min-width: 1280px) { /* xl */ }
```

### Responsive Behaviour Summary

| Component | Mobile | Tablet | Desktop |
|-----------|--------|--------|---------|
| Product grid | 2 cols | 3 cols | 4 cols |
| Nav | Bottom tabs | Top bar | Top bar + mega menu |
| Filters | Bottom sheet | Side drawer | Persistent left panel |
| Cart | Full-screen | Right drawer | Right drawer |
| Checkout | Single column | Single column | Two column |
| Hero | 60vh, text below | 75vh, text overlay | 85vh, text overlay |
| Footer | Single column | Two columns | Four columns |

---

## 10. Motion & Animation

### Principles
- **Purposeful** — every animation communicates state, guides attention, or provides feedback
- **Fast** — UI transitions: 150–200ms. Content reveals: 300–400ms. Never longer than 500ms for UI
- **Reduced motion** — all animations must respect `prefers-reduced-motion: reduce`

### Standard Easings

```css
:root {
  --ease-out:     cubic-bezier(0.25, 0.46, 0.45, 0.94);   /* Exits, fades out */
  --ease-in:      cubic-bezier(0.55, 0.06, 0.68, 0.19);   /* Entrances */
  --ease-spring:  cubic-bezier(0.34, 1.56, 0.64, 1);       /* Popups, toasts, bounce */
  --ease-linear:  linear;                                   /* Loaders, spinners */
}
```

### Animation Timing Guide

| Interaction | Duration | Easing |
|-------------|----------|--------|
| Button hover/active | 150ms | ease-out |
| Input focus | 150ms | ease-out |
| Tooltip appear | 150ms | ease-spring |
| Modal open | 250ms | ease-spring |
| Modal close | 200ms | ease-out |
| Cart drawer slide | 300ms | ease-out |
| Page transition | 300ms | ease-out |
| Toast slide in | 250ms | ease-spring |
| Skeleton shimmer | 1500ms | linear (infinite) |
| Image zoom on hover | 350ms | ease-out |

### Reduced Motion

```css
@media (prefers-reduced-motion: reduce) {
  *, *::before, *::after {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
  }
}
```

---

## 11. Accessibility

### Targets
- **WCAG 2.1 Level AA** minimum on all public pages
- **Level AAA** on critical paths: checkout, account, forms

### Color Contrast Ratios

| Pair | Ratio | Pass |
|------|-------|------|
| ink-900 on white | 17.8:1 | ✅ AAA |
| ink-700 on white | 7.2:1 | ✅ AAA |
| accent on white | 3.4:1 | ✅ AA large text |
| white on accent | 3.4:1 | ✅ AA large text |
| ink-500 on white | 4.6:1 | ✅ AA |

> Note: `--color-accent` (#C8956C) passes AA at 18px+ or bold 14px+. Never use for body text.

### Keyboard Navigation
- All interactive elements reachable by `Tab` in DOM order
- No keyboard traps (except modals — trap focus within open modal)
- Visible focus indicators on all interactive elements: `outline: 2px solid var(--color-accent); outline-offset: 2px`
- Skip-to-content link as first focusable element on every page

### ARIA Patterns

```html
<!-- Product card -->
<article class="product-card" aria-label="Linen Blazer, $129, 30% off">

<!-- Cart count -->
<button aria-label="Open cart, 3 items">
  <span aria-hidden="true">🛒</span>
  <span class="sr-only">3 items in cart</span>
</button>

<!-- Rating -->
<div role="img" aria-label="4.5 out of 5 stars">

<!-- Loading state -->
<div aria-live="polite" aria-busy="true">Loading products...</div>

<!-- Error message -->
<p role="alert">Unable to add item to cart. Please try again.</p>
```

### Screen Reader Utilities

```css
.sr-only {
  position:   absolute;
  width:      1px;
  height:     1px;
  padding:    0;
  margin:     -1px;
  overflow:   hidden;
  clip:       rect(0, 0, 0, 0);
  white-space:nowrap;
  border:     0;
}
```

---

## 12. Dark Mode

Dark mode uses a separate token layer. The semantic meaning of tokens stays the same; only the resolved values change.

```css
@media (prefers-color-scheme: dark) {
  :root {
    --color-accent:          #D4A882;
    --color-accent-hover:    #E0B996;
    --color-accent-subtle:   #2D1F14;

    --color-ink-900:         #F0EDEA;
    --color-ink-700:         #C4C0BC;
    --color-ink-500:         #7A7672;
    --color-ink-300:         #3A3632;
    --color-ink-100:         #2A2622;
    --color-ink-50:          #1E1B18;

    --color-surface:         #252220;
    --color-surface-raised:  #2D2A27;
    --color-page-bg:         #1A1714;

    --color-success:         #4CAF82;
    --color-success-subtle:  #1A2E22;
    --color-warning:         #E09040;
    --color-warning-subtle:  #2D2010;
    --color-danger:          #E85D5D;
    --color-danger-subtle:   #2E1010;
    --color-info:            #5B9BD4;
    --color-info-subtle:     #0E1E32;
  }
}
```

Users can also toggle manually via a `data-theme="dark"` attribute on `<html>`. Preference is saved to `localStorage` under key `theme`.

---

## 13. Icon System

### Library: Lucide Icons
Source: `lucide-react` or CDN (`https://unpkg.com/lucide@latest`)

Consistent sizing: `16px` inline, `20px` standard UI, `24px` navigation/CTA.

### Common Icons in Use

| Icon Name | Usage |
|-----------|-------|
| `shopping-cart` | Cart |
| `heart` | Wishlist |
| `search` | Search |
| `user` | Account |
| `menu` | Mobile hamburger |
| `x` | Close / dismiss |
| `chevron-down` | Accordion, select |
| `chevron-right` | Breadcrumb, list item |
| `star` | Rating (filled) |
| `star-half` | Half rating |
| `check` | Success, checkbox |
| `alert-circle` | Error/warning |
| `truck` | Shipping |
| `shield-check` | Security / trust |
| `rotate-ccw` | Returns |
| `credit-card` | Payment |
| `package` | Order / product |
| `filter` | Filter panel toggle |
| `sliders-horizontal` | Sort / adjust |
| `grid-2x2` | Grid view |
| `list` | List view |
| `zoom-in` | Image zoom |
| `share-2` | Share product |

---

## 14. State Management UI Patterns

### Loading States (per component)

```
Global loading:     Top progress bar (4px, accent color) — NProgress style
Page-level:         Full skeleton layout matching real content structure
Section-level:      Skeleton cards within section bounds
Inline/button:      Spinner replaces button label; button disabled
```

### Empty States

Every empty state must have:
1. **Illustration or icon** (subtle, on-brand)
2. **Headline** — what's missing
3. **Body text** — why and what to do
4. **CTA** — primary action to resolve the empty state

```
Empty cart:      "Your cart is empty"  →  [Start Shopping]
No results:      "No results for 'X'"  →  [Clear filters] or [Browse all]
No orders:       "No orders yet"       →  [Start shopping]
Empty wishlist:  "Nothing saved yet"   →  [Browse products]
Error state:     "Something went wrong"→  [Try again]
```

### Optimistic UI

Apply to: add to cart, add to wishlist, quantity update, coupon apply.

Pattern:
1. Immediately update UI with expected result
2. Show subtle loading indicator on the affected element
3. On success: confirm state (brief success toast)
4. On failure: revert UI, show error toast with retry

### Pagination vs Infinite Scroll

| Context | Pattern |
|---------|---------|
| PLP grid | Infinite scroll (mobile) / Load More button (desktop) |
| Order history | Pagination with page numbers |
| Reviews | Load More button |
| Search results | Pagination |

---

## Appendix A — Naming Conventions

### CSS Class Naming: BEM
```
.block {}
.block__element {}
.block--modifier {}
.block__element--modifier {}

Examples:
.product-card {}
.product-card__image {}
.product-card__price--sale {}
.btn--primary {}
.btn--lg {}
```

### Design Token Naming
```
--{category}-{property}-{variant}
--color-accent-hover
--text-body-sm
--space-4
--radius-lg
```

---

## Appendix B — File Structure

```
src/
├── styles/
│   ├── tokens.css          ← All design tokens (single source of truth)
│   ├── reset.css           ← Minimal CSS reset
│   ├── typography.css      ← Type scale, heading styles
│   ├── utilities.css       ← sr-only, skeleton, etc.
│   └── animations.css      ← Keyframes
│
├── components/
│   ├── ui/
│   │   ├── Button/
│   │   ├── Input/
│   │   ├── Badge/
│   │   ├── Toast/
│   │   └── Skeleton/
│   ├── product/
│   │   ├── ProductCard/
│   │   ├── ProductGallery/
│   │   ├── ProductRating/
│   │   └── ProductPrice/
│   ├── cart/
│   │   ├── CartDrawer/
│   │   ├── CartItem/
│   │   └── CartSummary/
│   ├── layout/
│   │   ├── Navigation/
│   │   ├── Footer/
│   │   └── PageWrapper/
│   └── checkout/
│       ├── StepIndicator/
│       ├── AddressForm/
│       └── PaymentForm/
│
└── pages/
    ├── home/
    ├── plp/
    ├── pdp/
    ├── cart/
    ├── checkout/
    └── account/
```

---

*Last updated: 2026 · Maintained by the Product Design & Frontend Engineering team*
*For questions, open a design system issue or ping #design-system in Slack.*
