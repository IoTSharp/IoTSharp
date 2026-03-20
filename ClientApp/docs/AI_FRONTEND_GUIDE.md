# IoTSharp Frontend Guide

This guide defines the baseline rules for future IoTSharp frontend work, especially when pages are generated or assisted by AI.

## Design Direction

IoTSharp should feel like an industrial command console:

- clean, data-dense, and modern
- calm instead of playful
- technical without looking like a generic admin template
- suitable for dashboards, telemetry, alarms, automation, and operations workflows

## Core Principles

1. Product first

Every screen should read as an IoTSharp product screen, not as a reused admin template.

2. Signal over noise

Use strong hierarchy, clear cards, and meaningful contrast. Avoid decorative clutter that does not help operations work.

3. AI-friendly structure

Pages should be easy to extend by following stable patterns, tokens, and layout zones.

4. Progressive richness

Use gradients, glass surfaces, and ambient backgrounds carefully. Keep interactions fast and readable.

## Required Tokens

Use variables from [`src/theme/iotsharp-tokens.scss`](d:/GitHub/IoTSharp/ClientApp/src/theme/iotsharp-tokens.scss) before introducing new hard-coded colors, radii, or shadows.

Preferred categories:

- brand colors
- surface colors
- border colors
- shell and card shadows
- shell and card radii
- standard page gutter

## Layout Rules

Default page anatomy:

1. shell
2. top context area
3. primary content zone
4. secondary insight zone when needed

For new pages:

- keep page spacing aligned with `--iotsharp-page-gutter`
- use one dominant headline region
- group controls close to the data they affect
- avoid floating unrelated widgets into the same row

## Page Patterns

### Dashboard

- one summary hero area
- one quick health/status strip
- one or two dense operational panels below

### List Pages

- title + context summary
- action row
- filter row
- table/card view
- empty/loading/error states designed explicitly

### Detail Pages

- top identity block
- status and important metrics first
- tabs or stacked sections for telemetry, properties, rules, and history
- destructive actions separated visually

### Setup / Installer / Login

- onboarding pages should share one visual family
- left side for brand and context
- right side for action
- avoid plain template forms without product framing

## Component Rules

- Prefer a small set of strong reusable wrappers over many one-off style hacks.
- Reuse cards, pills, metrics, empty states, and section headers consistently.
- Avoid introducing a new visual style for every feature page.
- Keep form labels visible and concise.
- Prefer descriptive button copy over generic words like `Submit`.

## Motion Rules

- Use short entrance transitions for shells and cards.
- Use hover only where it improves clarity.
- Avoid excessive motion on frequently refreshed telemetry views.

## AI Coding Checklist

Before adding a new page or redesigning an existing one:

1. Identify whether it is dashboard, list, detail, or onboarding.
2. Reuse the shell and token system first.
3. Define title, context, actions, and main data blocks before styling.
4. Keep visual emphasis on the most operationally important data.
5. Verify mobile and desktop layouts.
6. Avoid template branding or template-era naming.

## Avoid

- generic purple-on-white template styling
- random one-off gradients without product meaning
- deeply nested card-inside-card-inside-card layouts
- oversized empty whitespace on operational screens
- copying template comments, names, or demo content into IoTSharp pages

## Near-Term Cleanup Targets

- continue replacing legacy template comments and issue links in older modules
- remove template-oriented README and changelog references outside rewritten docs
- keep mock/menu resources inside the IoTSharp repo
- normalize page scaffolds across dashboard, list, and detail screens
