---
name: textbooker-frontend-review
description: Audit Textbooker Razor Pages and Pico/SCSS frontend changes for responsive layout, mobile/tablet/desktop/4K behavior, accessibility, motion, typography, interaction polish, and JavaScript/CSS responsibility. Use for landing-page changes, UI review, visual regressions, or “feels wrong” feedback in this repository.
---

# Textbooker Frontend Review

Use this concise repository profile before the general `design-taste-frontend` skill. Load `make-interfaces-feel-better` when typography, surfaces, animation, or interaction details are in scope. Consult the full design-taste skill only for an actual redesign or new landing-page composition.

## Design read

Treat Textbooker as a trust-first public marketplace for students and parents. Preserve the existing Pico language, brand blue, navigation, routes, copy, and accessibility work. Prefer targeted evolution over redesign.

Default dials: variance 4, motion 2–3, density 4–5.

## Audit sequence

1. Read the Razor page, page model/view components, page SCSS, shared layout, and page JavaScript.
2. Identify the real container and Pico breakpoints before changing widths.
3. Render or inspect approximately 320×800, 768×1024, 1440×900, and 3840×2160.
4. Check overflow, clipping, card density, tap targets, hero balance, and primary-action visibility.
5. Check reduced motion and dark mode when touched styles support them.

## Runtime verification

- Rebuild and restart the application after branch or SCSS changes. Do not use `--no-build` unless the current branch was built after its last edit.
- Stop only processes listening on the intended development ports, then start the server hidden and wait for the listener.
- Confirm the rendered page contains branch-specific markers; HTTP 200 alone can be a stale binary from another branch.
- Use Playwright screenshots yourself before asking for manual verification. Re-test after every visual adjustment rather than relying on source inspection.
- For clipping bugs, capture the exact failing viewport and inspect the full control box, not only document-level horizontal overflow.

## Layout

- Use CSS Grid and media queries for viewport layout. Do not use resize handlers to count cards.
- Keep ordinary content constrained. At ≥1920px, a landing container may grow to roughly 1600–1800px without stretching edge to edge.
- Add explicit tablet behavior. Do not jump directly from a two-column phone grid to four narrow cards at 576px.
- Use CSS to reveal an additional recent-item card on ultra-wide screens; keep data selection bounded in Razor.
- Hide decorative hero content where it crowds mobile tasks.
- For hamburger menus, verify open/closed state, stacking context, bottom padding, full button bounds, keyboard focus order, and both login/register variants on narrow screens.
- For animated hero tracks, verify the loop at several timestamps. Ensure duplicated sequences cover the viewport continuously and masks hide every rectangular edge at normal and zoomed views.

## Interaction and accessibility

- Target at least 40px visual control height where practical and never fall below WCAG 2.2's 24×24 CSS px minimum without an allowed spacing exception.
- Preserve visible focus, keyboard activation, reduced motion, truthful HTMX status, and lowercase ARIA tokens.
- Put accessible names and filter values in Razor/data attributes when known server-side.
- Avoid default-content entrance motion unless it communicates hierarchy. Do not let decorative infinite animation compete with the primary task.
- Use `will-change` only where sustained compositing is justified.

## Polish

- Keep headings balanced and body text readable at 45–75 characters.
- Use tabular numerals for prices.
- Outline images so white covers remain visible in light and dark themes.
- Use property-specific transitions and restrained press feedback.
- Verify the 4K hero optically; do not increase type solely because the viewport is large.

## Report

Separate source-derived concerns from screenshot-confirmed defects. Prioritize functional responsive problems over subjective polish. State which viewports and themes were actually tested; never claim a screenshot was inspected when only source was reviewed.
