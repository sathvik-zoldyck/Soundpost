# 0002 — License: GNU GPLv3

- **Status:** Accepted
- **Date:** 2026-08
- **Supersedes:** the project's initial MIT license.

## Context

Soundpost is meant to be a lasting, community-owned open-source project — a "statement" repo people
install, star, and build on. Early on it used MIT for maximum adoption. As the plugin/visualizer/theme
ecosystem took shape, the priority shifted toward **keeping the whole thing — and everything built on
it — open**.

## Decision

License Soundpost under the **GNU General Public License v3.0**.

## Consequences

- **Strong copyleft:** anyone who distributes a modified version must also release their source under
  GPLv3. Soundpost can't be quietly absorbed into a closed product (the exact fate the project was
  created to avoid).
- Contributions are accepted under GPLv3 (see [CONTRIBUTING](../../CONTRIBUTING.md)).
- Plugins are a nuance worth watching: a plugin that links Soundpost's code is generally expected to be
  GPL-compatible. This will be clarified in the Plugin SDK as it firms up.
- Trade-off accepted: some corporate/closed-source adoption is discouraged. That's aligned with the
  project's values — see [VISION](../../VISION.md).
- The name and logo are handled separately — see [TRADEMARK](../../TRADEMARK.md).
