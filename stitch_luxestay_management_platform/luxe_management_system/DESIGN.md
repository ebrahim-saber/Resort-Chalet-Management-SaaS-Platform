---
name: Luxe Management System
colors:
  surface: '#f9f9fc'
  surface-dim: '#dadadd'
  surface-bright: '#f9f9fc'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f3f3f6'
  surface-container: '#eeedf1'
  surface-container-high: '#e8e8eb'
  surface-container-highest: '#e2e2e5'
  on-surface: '#1a1c1e'
  on-surface-variant: '#42474e'
  inverse-surface: '#2f3033'
  inverse-on-surface: '#f1f0f4'
  outline: '#72777e'
  outline-variant: '#c2c7ce'
  surface-tint: '#3a6285'
  primary: '#002741'
  on-primary: '#ffffff'
  primary-container: '#0f3d5e'
  on-primary-container: '#81a8ce'
  inverse-primary: '#a3caf2'
  secondary: '#006a66'
  on-secondary: '#ffffff'
  secondary-container: '#72f7ef'
  on-secondary-container: '#00716c'
  tertiary: '#371f00'
  on-tertiary: '#ffffff'
  tertiary-container: '#553200'
  on-tertiary-container: '#cd9a5f'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#cee5ff'
  primary-fixed-dim: '#a3caf2'
  on-primary-fixed: '#001d33'
  on-primary-fixed-variant: '#204a6b'
  secondary-fixed: '#72f7ef'
  secondary-fixed-dim: '#51dad3'
  on-secondary-fixed: '#00201e'
  on-secondary-fixed-variant: '#00504c'
  tertiary-fixed: '#ffddba'
  tertiary-fixed-dim: '#f4bc7e'
  on-tertiary-fixed: '#2b1700'
  on-tertiary-fixed-variant: '#643f0b'
  background: '#f9f9fc'
  on-background: '#1a1c1e'
  surface-variant: '#e2e2e5'
typography:
  display-lg:
    fontFamily: Inter
    fontSize: 48px
    fontWeight: '600'
    lineHeight: 56px
    letterSpacing: -0.02em
  display-lg-mobile:
    fontFamily: Inter
    fontSize: 32px
    fontWeight: '600'
    lineHeight: 40px
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Inter
    fontSize: 32px
    fontWeight: '500'
    lineHeight: 40px
    letterSpacing: -0.01em
  headline-lg-mobile:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '500'
    lineHeight: 32px
  headline-md:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '500'
    lineHeight: 32px
  body-lg:
    fontFamily: Inter
    fontSize: 18px
    fontWeight: '400'
    lineHeight: 28px
  body-md:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  label-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '600'
    lineHeight: 20px
    letterSpacing: 0.05em
  label-sm:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '500'
    lineHeight: 16px
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  unit: 8px
  container-max: 1440px
  gutter: 24px
  margin-desktop: 64px
  margin-tablet: 32px
  margin-mobile: 20px
  section-gap: 80px
---

## Brand & Style

This design system is engineered for high-end resort and chalet management, focusing on a "Digital Concierge" experience. The brand personality is professional yet inviting, mirroring the seamless service found in five-star hospitality. 

The aesthetic leverages **Glassmorphism** and **Minimalism** to create a sense of airiness and depth. Large margins and generous whitespace ensure the UI never feels cluttered, allowing high-resolution property photography to serve as a primary design element. The emotional response should be one of calm control and quiet luxury, using soft background blurs and subtle gradients to imply sophistication without sacrificing functional clarity.

## Colors

The palette is anchored by **Deep Ocean Blue**, providing a foundation of trust and authority. **Turquoise** is used for primary actions and highlights, reflecting tropical or aquatic luxury, while **Warm Gold** is reserved for premium accents, loyalty indicators, and high-value status markers.

The background uses **Soft Ivory** to reduce the harshness of pure white, creating a more "paper-like" editorial feel. Surface containers are **Pure White** with varying levels of opacity when used in glassmorphic contexts. Functional colors (Success, Warning, Error) are desaturated to maintain the sophisticated atmosphere, avoiding overly "loud" system alerts.

## Typography

The design system utilizes **Inter** for all roles to maintain a clean, modernist aesthetic that scales perfectly from dense data grids to immersive hero sections. 

- **Display & Headlines:** Feature tighter letter spacing and medium weights to create a strong visual anchor.
- **Body Text:** Uses a slightly increased line height (1.5x) to ensure readability during long management sessions.
- **Labels:** Small labels and utility text use a semi-bold weight and subtle tracking (letter spacing) to maintain legibility at small scales, often utilizing uppercase for section headers to provide clear visual hierarchy.

## Layout & Spacing

The layout philosophy is based on a **Fixed Grid** for desktop (12 columns) and a **Fluid Grid** for smaller devices. To evoke luxury, the system intentionally uses "excessive" spacing—larger margins and section gaps than standard SaaS applications.

- **Desktop (1440px+):** 64px outer margins with 24px gutters.
- **Tablet (768px - 1439px):** 32px outer margins with 20px gutters.
- **Mobile (<767px):** 20px outer margins with 16px gutters.

Vertical rhythm follows an 8px base unit. Component internal padding should be generous (typically 24px or 32px) to prevent data from appearing cramped.

## Elevation & Depth

Depth is established through a combination of **Glassmorphism** and **Ambient Shadows**.

1.  **Level 0 (Base):** Soft Ivory background.
2.  **Level 1 (Cards/Panels):** Pure White surface with a 1px stroke (#FFFFFF at 40% opacity) and a very soft, diffused shadow (Offset: 0, 4px; Blur: 20px; Color: Primary at 5% opacity).
3.  **Level 2 (Floating Nav/Modals):** Semi-transparent white (80% opacity) with a 20px Backdrop Blur. These elements appear to float significantly above the content.
4.  **Interactive States:** On hover, cards should subtly lift (shadow expands) and the background blur intensity should increase.

Gradients should be used sparingly—primarily as subtle overlays on images to ensure text legibility (Linear: Transparent to Deep Ocean Blue at 60% opacity).

## Shapes

The shape language is defined by large, inviting radii. The default `rounded-md` is **8px** for small UI elements like buttons and inputs. Larger containers, such as property cards and dashboard panels, use `rounded-lg` (**16px**) or `rounded-xl` (**24px**) to create a soft, modern silhouette.

Circular shapes are reserved strictly for user avatars and icon containers to provide a counterpoint to the predominantly rectangular grid.

## Components

### Buttons & Controls
- **Primary:** Deep Ocean Blue background with white text. High-contrast, sharp, and authoritative. 
- **Secondary:** Turquoise stroke with Turquoise text. Used for secondary actions.
- **Ghost/Tertiary:** No background; Warm Gold text. Used for "Cancel" or low-priority navigation.

### Floating Navigation
The primary navigation should be a "Floating Bar" docked at the top or bottom, featuring a 20px backdrop blur, a 1px white border, and a subtle drop shadow. This ensures the resort photography remains visible behind the UI.

### Property Cards
Cards are image-driven. The image should occupy the top 60% of the card, with a subtle transition into the white surface below. Metadata (price, location, availability) should use `label-md` for high clarity.

### Glassmorphic Panels
Used for sidebars and filtering overlays. These should utilize a `rgba(255, 255, 255, 0.7)` background with a `backdrop-filter: blur(12px)`.

### Input Fields
Inputs should have a Soft Ivory background and a 1px border (#D1D5DB). On focus, the border transitions to Turquoise with a subtle outer glow (0 0 0 4px Turquoise at 10% opacity).