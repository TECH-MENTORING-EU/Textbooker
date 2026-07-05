# TextBooker accessibility statement

**Last updated:** 2026-06-29

**Scope:** the public TextBooker application, administration panel, and account management area

**Conformance target:** WCAG 2.2 Level AA (W3C Recommendation)

**Related standard:** EN 301 549 v3.2.1 (European Accessibility Act)

## Conformance statement

TECH-MENTORING-EU is committed to making TextBooker conform to WCAG 2.2 Level AA.
The current accessibility work is tracked in this document and in
[issue #13](https://github.com/TECH-MENTORING-EU/Textbooker/issues/13).

### Current status: technically implemented, pending full manual verification

The automated axe-core suite reports no A/AA violations on the pages it covers.
This result is not, by itself, a declaration of full WCAG conformance. Criteria
that require human judgement and page states outside the automated suite must
pass the manual procedure described below.

## Technical implementation

Accessibility is implemented with:

- semantic HTML5 landmarks (`header`, `nav`, `main`, `section`, `aside`, `footer`);
- ARIA 1.2 only where native HTML does not provide the required semantics;
- SCSS in `wwwroot/scss/_a11y.scss` for visible focus, reduced motion,
  forced-colors support, target sizes, and the skip link;
- HTMX and Razor Pages live regions for dynamic validation and status messages;
- limited ARIA support in the Blazor Server chat island, which remains a known gap.

### Key files

| File | Responsibility |
|---|---|
| `wwwroot/scss/_a11y.scss` | Skip link, focus visibility, reduced motion, forced colors, target size |
| `wwwroot/js/site.js` | Dialog focus return, dropdown state, Escape handling, HTMX announcements |
| `Pages/Shared/_Layout.cshtml` | Landmarks, skip link, and help dialog semantics |
| `Pages/Shared/_LoginPartial.cshtml` | Native account menu and expanded state |
| `Areas/Admin/Pages/_AdminNav.cshtml` | Valid, conditional `aria-current` values |
| `Booker.AccessibilityTests/accessibility.spec.ts` | axe-core and keyboard behavior tests |
| `.github/workflows/accessibility.yml` | Accessibility checks for pull requests and `main` |

## WCAG 2.2 A and AA checklist

Legend: **Covered** means supported by implementation and available tests;
**Partial** means additional work is known; **Manual** means manual verification
is still required; **N/A** means the criterion does not apply. These statuses are
implementation evidence, not an independent conformance declaration.

### Principle 1: Perceivable

| Criterion | Level | Status | Evidence or note |
|---|---:|---|---|
| 1.1.1 Non-text Content | A | Covered | Alternative text for book covers and avatars; decorative icons are hidden |
| 1.2.1–1.2.5 Time-based Media | A/AA | N/A | The application has no audio or video content |
| 1.3.1 Info and Relationships | A | Covered | Table captions/scopes, lists, and landmarks |
| 1.3.2 Meaningful Sequence | A | Covered | DOM order matches the visual order |
| 1.3.3 Sensory Characteristics | A | Covered | Instructions do not rely only on color or shape |
| 1.3.4 Orientation | AA | Covered | No orientation lock |
| 1.3.5 Identify Input Purpose | AA | Covered | Autocomplete metadata on identity forms |
| 1.4.1 Use of Color | A | Covered | Filter state also uses text and `aria-pressed` |
| 1.4.2 Audio Control | A | N/A | No audio content |
| 1.4.3 Contrast (Minimum) | AA | Manual | axe covers baseline states; remaining states require manual review |
| 1.4.4 Resize Text | AA | Covered | Relative units and no zoom restriction |
| 1.4.5 Images of Text | AA | Covered | The logo is vector-based |
| 1.4.10 Reflow | AA | Covered | Playwright verifies 320 CSS px; tables scroll in named regions |
| 1.4.11 Non-text Contrast | AA | Manual | Form boundaries require final manual confirmation |
| 1.4.12 Text Spacing | AA | Manual | No blocking styles; manual verification remains required |
| 1.4.13 Content on Hover or Focus | AA | Covered | Help content is keyboard-operable and dismissible |

### Principle 2: Operable

| Criterion | Level | Status | Evidence or note |
|---|---:|---|---|
| 2.1.1 Keyboard | A | Covered | Carousel, menus, dialogs, and filters are keyboard-operable |
| 2.1.2 No Keyboard Trap | A | Covered | Dialogs can be dismissed with Escape |
| 2.1.4 Character Key Shortcuts | A | Covered | No single-character shortcuts |
| 2.2.1 Timing Adjustable | A | Covered | Renewable 14-day cookie falls under the over-20-hour exception |
| 2.2.2 Pause, Stop, Hide | A | Covered | Reduced-motion preferences disable animation |
| 2.3.1 Three Flashes | A | Covered | No flashing animation |
| 2.4.1 Bypass Blocks | A | Covered | Skip link targets `#main-content` |
| 2.4.2 Page Titled | A | Covered | Pages set `ViewData["Title"]` |
| 2.4.3 Focus Order | A | Covered | Logical DOM and focus order |
| 2.4.4 Link Purpose (In Context) | A | Covered | Action labels identify their targets |
| 2.4.5 Multiple Ways | AA | Covered | Main navigation, sitemap, and listing search |
| 2.4.6 Headings and Labels | AA | Covered | Section headings and form labels |
| 2.4.7 Focus Visible | AA | Covered | Global `:focus-visible` styles |
| 2.4.11 Focus Not Obscured (Minimum) | AA | Manual | No persistent overlays; final manual verification required |
| 2.5.1 Pointer Gestures | A | Covered | Keyboard alternatives are available |
| 2.5.2 Pointer Cancellation | A | Covered | Controls activate on click or keyboard completion |
| 2.5.3 Label in Name | A | Covered | Accessible names include visible labels |
| 2.5.4 Motion Actuation | A | N/A | No device-motion interaction |
| 2.5.7 Dragging Movements | AA | Covered | Cropper provides arrow keys and directional buttons |
| 2.5.8 Target Size (Minimum) | AA | Covered | Playwright measures visible mobile controls |

### Principle 3: Understandable

| Criterion | Level | Status | Evidence or note |
|---|---:|---|---|
| 3.1.1 Language of Page | A | Covered | The document language is `pl` |
| 3.1.2 Language of Parts | AA | Covered | User-facing content is consistently Polish |
| 3.2.1 On Focus | A | Covered | Focus does not change context |
| 3.2.2 On Input | A | Covered | Context-changing actions use explicit submission |
| 3.2.3 Consistent Navigation | AA | Covered | Shared navigation is consistent |
| 3.2.4 Consistent Identification | AA | Covered | Shared components retain consistent names |
| 3.2.6 Consistent Help | A | Covered | Help is provided in the shared layout |
| 3.3.1 Error Identification | A | Covered | Validation uses alerts and field-level messages |
| 3.3.2 Labels or Instructions | A | Covered | Help text is connected with `aria-describedby` |
| 3.3.3 Error Suggestion | AA | Covered | Validation describes the cause and correction |
| 3.3.4 Error Prevention | AA | Covered | Destructive actions require confirmation |
| 3.3.7 Redundant Entry | A | Covered | No repeated entry within the same process |
| 3.3.8 Accessible Authentication | AA | Covered | No image-based CAPTCHA |

### Principle 4: Robust

| Criterion | Level | Status | Evidence or note |
|---|---:|---|---|
| 4.1.2 Name, Role, Value | A | Covered | Custom controls expose programmatic semantics |
| 4.1.3 Status Messages | AA | Covered | Validation and HTMX updates use live regions |

AAA criteria are outside this Level AA conformance target.

## Open verification gates

1. Complete an NVDA and VoiceOver audit of authenticated and administrator views,
   including validation errors and HTMX updates.
2. Verify 200% and 400% zoom and WCAG 1.4.12 text spacing on all key pages.
3. Manually verify target-size exceptions and controls hidden on initial render.
4. Extend axe-core coverage whenever new pages, states, or user roles are added.

## Testing

### Manual

1. Navigate every key page with Tab, Shift+Tab, Enter, Space, and Escape.
2. Test `/`, `/Book/{id}`, `/Add`, `/Edit/{id}`, identity pages, profiles,
   account management, and administrator pages with NVDA or VoiceOver.
3. Test browser zoom at 200% and 400% without horizontal document scrolling.
4. Enable `prefers-reduced-motion: reduce` and `forced-colors: active`.

### Automated

Run `npm run test:a11y`. The Playwright and axe-core suite covers public,
identity, profile, account-management, listing, editing, and administration
flows. It checks supported WCAG A/AA rules as well as the skip link, dialog focus
return, cropper keyboard alternatives, HTMX announcements, image loading,
320 CSS px reflow, and minimum target sizes. The same suite runs in
`.github/workflows/accessibility.yml` for pull requests and changes to `main`.

## Reporting accessibility problems

Open an issue at <https://github.com/TECH-MENTORING-EU/Textbooker/issues> with the
`accessibility` label. Include the affected page or component, reproduction
steps, and the assistive technology used. You can also contact
support@textbooker.pl.
