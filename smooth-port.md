# SmoothUI → Flowery.NET porting plan

This document tracks which SmoothUI components are (a) already covered in Flowery.NET, (b) worth enhancing, (c) should become new reusable controls, or (d) should stay as Gallery-only “eye candy”.

## Scope & constraints

- **Source**: `smoothui/packages/smoothui/components/*` (React + motion).
- **Target**: `Flowery.NET` (Avalonia controls/themes) + `Flowery.NET.Gallery` examples.
- **Control/theme conventions**:
  - Controls: `Flowery.NET/Controls/Daisy*.cs`
  - Themes: `Flowery.NET/Themes/Daisy*.axaml`
- **Docs/screenshots mapping must stay in sync**:
  - `Flowery.NET.Gallery/Examples/SectionHeader.axaml.cs` (`SectionIdToControlName`)
  - `Utils/generate_docs.py` (`_section_to_control()`)

## Component inventory (SmoothUI)

Folders found in `smoothui/packages/smoothui/components/`:
- `ai-branch`, `ai-input`, `animated-input`, `animated-o-t-p-input`, `animated-progress-bar`, `animated-tags`, `app-download-stack`, `apple-invites`, `basic-accordion`, `basic-dropdown`, `basic-modal`, `basic-toast`, `button-copy`, `clip-corners-button`, `contribution-graph`, `cursor-follow`, `dot-morph-button`, `dynamic-island`, `expandable-cards`, `figma-comment`, `github-stars-animation`, `image-metadata-preview`, `interactive-image-selector`, `job-listing-component`, `matrix-card`, `number-flow`, `phototab`, `power-off-slide`, `price-flow`, `reveal-text`, `rich-popover`, `scramble-hover`, `scroll-reveal-paragraph`, `scrollable-card-stack`, `siri-orb`, `social-selector`, `typewriter-text`, `user-account-avatar`, `wave-text`.

## Triage matrix (final)

Legend:

- **New**: build as reusable Flowery control
- **Enhance**: already exists; consider small opt-in enhancements
- **Covered**: already equivalent in Flowery
- **Recipe**: Gallery-only composition using existing controls
- **Eye candy**: Gallery-only demo (optional)

| SmoothUI component | Triage | Flowery equivalent / notes |
|---|---:|---|
| `basic-accordion` | Covered | `DaisyAccordion`, `DaisyCollapse` |
| `basic-modal` | Covered | `DaisyModal` |
| `basic-toast` | Covered | `DaisyToast` |
| `animated-input` | Covered/Enhance | `DaisyInput` supports `LabelPosition=Floating`; optionally add smoother transitions in theme |
| `animated-progress-bar` | Covered/Enhance | `DaisyProgress`; optionally add animated transitions on value updates |
| `user-account-avatar` | Covered | `DaisyAvatar`, `DaisyAvatarGroup` |
| `cursor-follow` | Covered | `Effects/CursorFollowBehavior.cs` |
| `scramble-hover` | Covered | `Effects/ScrambleHoverBehavior.cs` (supports scramble-on-hover mode) |
| `reveal-text` | Covered | `Effects/RevealBehavior.cs` (note: not viewport-aware) |
| `wave-text` | Covered/Enhance | `Effects/WaveTextBehavior.cs` (currently whole-block wave; per-character would be eye candy) |
| `animated-o-t-p-input` | New | Add `DaisyOtpInput` |
| `contribution-graph` | New | Add `DaisyContributionGraph` |
| `rich-popover` | New | Add `DaisyPopover` base + `DaisyRichPopover` (rich tooltip/popover with actions) |
| `basic-dropdown` | New | Add `DaisyDropdown` (menu-style dropdown). `DaisySelect` remains for ComboBox-style selection |
| `animated-tags` | New | Add `DaisyTagPicker` (selectable “chip” list) |
| `button-copy` | New | Add `DaisyCopyButton` (copy-to-clipboard with success state) |
| `number-flow` | New | Add `DaisyAnimatedNumber` (rolling digits display); keep `DaisyNumericUpDown` as input |
| `price-flow` | New (fold into AnimatedNumber) | Use same `DaisyAnimatedNumber` for 2+ digit formats |
| `phototab` | Recipe | Build in Gallery using `DaisyTabs` + custom header templates + animated indicator |
| `typewriter-text` | Eye candy | Optional: add `TypewriterBehavior` or Gallery-only demo |
| `scroll-reveal-paragraph` | Eye candy | Optional: implement viewport/scroll-aware reveal later |
| `power-off-slide` | Eye candy | Optional: “SlideToConfirm” control (destructive confirm pattern) |
| `expandable-cards` | Eye candy | Gallery demo using `DaisyCard` + animations |
| `scrollable-card-stack` | Eye candy | Gallery demo (complex interaction) |
| `ai-branch` | Eye candy / niche | Could be a chat UI helper control; not core |
| `ai-input` | Eye candy / niche | Highly app-specific (morphing input surface) |
| `app-download-stack` | Eye candy | Marketing layout |
| `apple-invites` | Eye candy | Marketing layout |
| `clip-corners-button` | Eye candy | Styling variant demo |
| `dot-morph-button` | Eye candy | Styling/animation demo |
| `dynamic-island` | Eye candy | Platform-specific aesthetic |
| `figma-comment` | Eye candy | App-specific UI |
| `github-stars-animation` | Eye candy | Animation demo |
| `image-metadata-preview` | Eye candy | App-specific UI |
| `interactive-image-selector` | Eye candy | App-specific UI |
| `job-listing-component` | Eye candy | App-specific UI |
| `matrix-card` | Eye candy | Visual demo |
| `siri-orb` | Eye candy | Visual demo |
| `social-selector` | Eye candy | App-specific UI |

## Phase 1 — New reusable controls (deliverables)

### 1) `DaisyContributionGraph` (from `contribution-graph`)

- **Why**: Flowery has no contribution-calendar heatmap.
- **Deliverables**:
  - `Flowery.NET/Controls/DaisyContributionGraph.cs`
  - `Flowery.NET/Themes/DaisyContributionGraph.axaml`
  - Gallery section: `Flowery.NET.Gallery/Examples/DataDisplayExamples.axaml`

### 2) `DaisyOtpInput` (from `animated-o-t-p-input`)

- **Why**: common verification-code UX; not covered by existing controls.
- **Deliverables**:
  - `Flowery.NET/Controls/DaisyOtpInput.cs`
  - `Flowery.NET/Themes/DaisyOtpInput.axaml`
  - Gallery section: `Flowery.NET.Gallery/Examples/DataInputExamples.axaml`

## Phase 2 — Similar/existing components: detailed analysis & options

### `basic-accordion` → `DaisyAccordion` / `DaisyCollapse`

- **Current**: `Flowery.NET/Controls/DaisyAccordion.cs`, `DaisyCollapse.cs`.
- **Option A (preferred)**: keep defaults; add Gallery example showing how to tune transitions.
- **Option B**: tweak `Flowery.NET/Themes/DaisyAccordion.axaml` (risk: global visual change).

### `basic-modal` → `DaisyModal`

- **Current**: `Flowery.NET/Controls/DaisyModal.cs`.
- **Option A (preferred)**: treat as equivalent.
- **Option B**: add entry/exit transitions (theme-only).

### `basic-toast` → `DaisyToast`

- **Current**: `Flowery.NET/Controls/DaisyToast.cs`.
- **Option A (preferred)**: treat as equivalent.
- **Option B**: add stack/auto-dismiss helpers later if demanded.

### `animated-input` → `DaisyInput`

- **Current**: `Flowery.NET/Controls/DaisyInput.cs` supports `DaisyLabelPosition.Floating`.
- **Option A (preferred)**: keep as-is; optionally add subtle transitions in `Flowery.NET/Themes/DaisyInput.axaml`.
- **Option B**: add an opt-in class (e.g. `Classes="animated"`) to avoid changing defaults.

### `animated-progress-bar` → `DaisyProgress`

- **Current**: `Flowery.NET/Controls/DaisyProgress.cs`.
- **Option A (preferred)**: animate in theme via `Transitions`.
- **Option B**: add explicit animation properties (more API surface).

### `cursor-follow` / `scramble-hover` / `reveal-text` / `wave-text`

- **Current**:
  - `Flowery.NET/Effects/CursorFollowBehavior.cs`
  - `Flowery.NET/Effects/ScrambleHoverBehavior.cs`
  - `Flowery.NET/Effects/RevealBehavior.cs`
  - `Flowery.NET/Effects/WaveTextBehavior.cs`
- **Notes**:
  - Viewport-aware reveal (SmoothUI `triggerOnView`) could become a separate `ScrollRevealBehavior` later.
  - Per-character wave is possible but better treated as eye candy.

### Dropdown/Popover gap (`basic-dropdown`, `rich-popover`)

- **Current primitives**:
  - ComboBox-based: `DaisySelect`, `DaisyThemeDropdown`, `DaisySizeDropdown`.
  - Tooltip styling: `Flowery.NET/Themes/DaisyToolTip.axaml`.
- **Decision**:
  - Implement a reusable **`DaisyPopover`** (Popup/light-dismiss) and build `DaisyDropdown` + `DaisyRichPopover` on top.

### Tags/copy/animated-number gap (`animated-tags`, `button-copy`, `number-flow`)

- **Decision**:
  - Implement reusable controls:
    - `DaisyTagPicker`
    - `DaisyCopyButton`
    - `DaisyAnimatedNumber`

## Phase 3 — Eye candy (Gallery-only)

Goal: keep Flowery.NET lean; ship these only as Gallery demos if they prove valuable.

Recommended Gallery-only candidates:

- `phototab` (as a `DaisyTabs` recipe)
- `expandable-cards`, `scrollable-card-stack`
- `power-off-slide`
- `scroll-reveal-paragraph`
- `typewriter-text`
- Marketing/layout-heavy components: `app-download-stack`, `apple-invites`, `figma-comment`, `job-listing-component`, `dynamic-island`, `siri-orb`, `github-stars-animation`, `matrix-card`, `social-selector`, `interactive-image-selector`, `image-metadata-preview`, `clip-corners-button`, `dot-morph-button`.

## Integration checklist (per new control)

- Add control class in `Flowery.NET/Controls/`.
- Add theme file in `Flowery.NET/Themes/` and ensure it is included by the app theme.
- Add Gallery section with `SectionHeader` and `DaisyDivider` separation.
- Update mappings:
  - `Flowery.NET.Gallery/Examples/SectionHeader.axaml.cs`
  - `Utils/generate_docs.py`
- Run `python Utils/generate_docs.py`.
