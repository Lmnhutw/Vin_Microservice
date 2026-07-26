---
name: Vin Shop
description: A composed storefront where every product feels selected and every action feels traceable.
colors:
  ink: "#17202a"
  ink-soft: "#263342"
  paper: "#f7f7f2"
  white: "#ffffff"
  routing-accent: "#d9ff43"
  routing-accent-deep: "#b9e22d"
  muted: "#68717b"
  line: "#cfd3cf"
  danger: "#a32f2a"
  success: "#205d3e"
typography:
  display:
    fontFamily: "Segoe UI, Arial, sans-serif"
    fontSize: "clamp(2.8rem, 12vw, 9rem)"
    fontWeight: 950
    lineHeight: 0.92
    letterSpacing: "-0.04em"
  body:
    fontFamily: "Segoe UI, Arial, sans-serif"
    fontSize: "1rem"
    fontWeight: 400
    lineHeight: 1.6
  label:
    fontFamily: "Segoe UI, Arial, sans-serif"
    fontSize: "0.7rem"
    fontWeight: 900
    lineHeight: 1.4
    letterSpacing: "0.14em"
rounded:
  control: "8px"
  surface: "14px"
spacing:
  xs: "8px"
  sm: "12px"
  md: "16px"
  lg: "32px"
  xl: "64px"
components:
  button-primary:
    backgroundColor: "{colors.ink}"
    textColor: "{colors.white}"
    rounded: "{rounded.control}"
    padding: "12px 18px"
    height: "48px"
  button-accent:
    backgroundColor: "{colors.routing-accent}"
    textColor: "{colors.ink}"
    rounded: "{rounded.control}"
    padding: "12px 18px"
    height: "48px"
  input:
    backgroundColor: "{colors.white}"
    textColor: "{colors.ink}"
    rounded: "{rounded.control}"
    padding: "12px 13px"
    height: "48px"
---

# Design System: Vin Shop

## Overview

**Creative North Star: "The Dispatch Catalogue"**

Vin Shop borrows the clarity of a well-run dispatch desk and the confidence of a contemporary product catalogue. Large product photography and decisive type create desire; routing marks, compact identifiers, and crisp status language quietly suggest that every action has a destination.

The system is energetic without becoming playful. Customer pages persuade through scale and photography. Cart, account, admin, and architecture surfaces become calmer and denser while retaining the same ink, paper, and routing vocabulary.

**Key Characteristics:**

- Full-width visual anchors with restrained copy.
- Ink-dark structural surfaces against clean paper-like content fields.
- One electric routing accent used for action and state.
- Square, directional composition softened only where touch ergonomics require it.
- Motion communicates arrival, routing, and state change.

## Colors

Use a restrained strategy: mineral ink and paper neutrals carry the interface, while one high-energy routing color identifies the next action or current state.

- **Mineral Ink:** navigation, primary buttons, display text, and operational summaries.
- **Dispatch Paper:** the main reading surface and quiet whitespace.
- **Routing Acid:** the single high-energy action and state marker.
- **Transit Grey:** secondary copy, borders, and inactive metadata.

**The Routing Mark Rule.** The accent marks a destination, selection, or successful state; it is never scattered as decoration.

## Typography

Display type should be wide, direct, and poster-like. Body and control text should be a neutral workhorse with excellent small-size clarity. Until a bundled font is selected, use the platform sans stack and create distinction through scale, width, weight, and composition.

- **Display:** extra-heavy, tightly tracked, capped at the 9rem hero scale.
- **Body:** regular weight with a 1.6 line height and a 65–75 character measure.
- **Route label:** compact, bold, uppercase metadata with deliberate tracking.

**The One Poster Rule.** Only one message per viewport earns display scale; all supporting language remains compact.

## Layout

Customer-facing landing content uses edge-to-edge sections with a constrained inner grid. Product grids alternate between dense catalogue scanning and one oversized featured item. Operational screens use a 12-column desktop grid, a simplified tablet grid, and one-column mobile flow.

Persistent navigation counts against the first viewport. Touch targets are at least 44px. Text measure stays within 65–75 characters. On mobile, filters become a controlled drawer and cart totals remain close to the primary checkout action.

## Elevation & Depth

Depth comes from overlapping planes, image crop, and directional movement. Routine content does not float. Use a border or a shadow, never both. Shadows have a visible vertical offset and soft blur.

## Shapes

Primary content planes are squared or gently clipped. Interactive controls use compact corners for touch comfort; pills are reserved for filters, statuses, and small metadata. Product imagery may use asymmetric clipping when it strengthens the dispatch direction.

## Components

- Navigation behaves like a routing header: brand, primary destinations, account state, and cart count are immediately scannable.
- Primary buttons are solid routing marks with explicit verb labels.
- Secondary controls use ink text and a clear boundary without ornamental fill.
- Product tiles let photography dominate; metadata and price align on a strict baseline.
- Inputs expose label, focus, validation, and recovery text without layout shift.
- Toasts announce the completed action and offer recovery when useful.

## Do's and Don'ts

### Do

- Use real product imagery and make its crop intentional.
- Make loading, empty, error, disabled, and success states visible.
- Let alignment and scale create hierarchy before adding containers.
- Preserve familiar commerce affordances even when the composition is distinctive.
- Honor reduced-motion preferences.

### Don't

- Build the page from a mosaic of equal rounded cards.
- Use gradients, glass, glow, or decorative grids as a substitute for product content.
- Put marketing language inside admin or operational surfaces.
- Repeat uppercase eyebrow labels over every section.
- Invent stock, shipping, customer, pricing, or performance claims.
