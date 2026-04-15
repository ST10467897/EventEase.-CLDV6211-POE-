---
name: ui-ux
description: UI/UX polish guidelines — typography, spacing, colors, cards, forms, tables, nav, buttons, accessibility. Use when building or improving frontend views.
---

# UI/UX Polish Skill

Apply these principles when building or improving frontend views to ensure consistent, professional UI quality.

## Typography

- **Weight scale**: 400 (body text), 500 (labels, nav links), 600 (subheadings, table headers), 700 (page headings)
- **Letter-spacing**: -0.3px on headings for a tighter, modern look
- **Small labels**: uppercase, letter-spacing 0.5px, font-weight 600, font-size 0.7-0.75rem, color #888
- **Page headings**: 1.3rem, weight 700, color matches primary palette

## Spacing

- **Card padding**: 1.25rem (stat cards), 1.5rem (form cards)
- **Section gaps**: use Bootstrap `g-4` for grid gaps
- **Page header**: bottom margin 1.5rem, short accent underline (40px wide, 3px tall) instead of full-width border
- **Breathing room**: prefer 1.5-2rem vertical spacing between major sections

## Colors

- Define a palette with CSS custom properties on `:root`
- Use opacity variants for backgrounds: `rgba(accent, 0.1)` for icon badges, `rgba(accent, 0.15)` for active nav pills
- Each entity/category gets its own accent color for left borders and icon badges
- Danger/delete actions use red tones, success uses green, neutral uses gray

## Cards

- `border: none` — use shadow instead of borders
- `border-radius: 12px`
- `box-shadow: 0 2px 12px rgba(0,0,0,0.06)` default, `0 6px 20px rgba(0,0,0,0.1)` on hover
- `transition: transform 0.2s, box-shadow 0.2s` with `translateY(-2px)` on hover
- **Stat cards**: colored left border (3px), icon badge on right, label + large number on left
- **Entity cards**: image on top (fixed height with object-fit:cover), content below, action buttons in footer

## Forms

- Use standard Bootstrap form controls with consistent border-radius (6-8px)
- **File upload areas**: dashed border (2px dashed #ccc), centered icon + text, hover changes border color to accent
- Group related fields, use clear labels above inputs
- Validation messages in `.text-danger` below the relevant input
- Use `enctype="multipart/form-data"` when forms include file uploads
- Show current image preview above upload area on edit forms

## Tables

- **Header**: dark background (primary color), white text, font-weight 600, no border
- **Rows**: `vertical-align: middle` on all `td` elements
- Use `table-hover` for interactive row highlighting
- Wrap tables in cards with `card-body p-0` for flush edges

## Navigation

- Active page gets a subtle pill background: `rgba(accent, 0.15)` with `border-radius: 6px`
- Use controller name matching to set active state dynamically
- User info (icon + username) and logout on the right, separated by a divider
- All nav links get font-weight 500 and smooth transition on hover

## Buttons

- **Primary action**: accent-colored `.btn-accent` (filled)
- **Secondary actions**: `.btn-outline-secondary` or `.btn-outline-primary`
- **Danger actions**: `.btn-outline-danger` with red text, not filled
- Use Bootstrap Icons inside buttons with spacing (`me-1` or `me-2`)
- Small buttons (`.btn-sm`) for card actions and table row actions

## Empty States

- Center content vertically with `text-center py-5`
- Large muted icon (`.display-3` or `.display-4`)
- Brief message below the icon
- Call-to-action button if applicable (e.g., "Create Booking")

## Accessibility

- Ensure color contrast meets WCAG AA (4.5:1 for text, 3:1 for large text)
- All interactive elements must have visible focus states
- Use semantic HTML: `<nav>`, `<main>`, `<footer>`, `<table>`, `<form>`
- Images must have `alt` attributes
- Form inputs must have associated `<label>` elements
- Don't rely on color alone to convey information — pair with icons or text
