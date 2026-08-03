# RFCs — Request for Comments

Bigger changes — anything that touches the architecture, the plugin API, or a cross-cutting user
experience — start as an **RFC** so the design gets discussed before code is written. Small fixes and
self-contained features don't need one; just open a PR.

## When to write an RFC

- Changing or extending the [Plugin SDK](../../PLUGIN_SDK.md) contract.
- A new subsystem (e.g. the rules engine, multi-output mirroring).
- Anything that affects reliability, persistence format, or the COM firewall boundary.
- A change you expect to be contentious or hard to reverse.

## Process

1. Copy [`0000-template.md`](0000-template.md) to `NNNN-short-title.md` (next free number).
2. Open a PR adding it. Discussion happens in the PR.
3. When there's rough consensus, a maintainer marks it **Accepted** and it guides implementation.
   Rejected RFCs stay in the repo (with the reason) — they're a valuable record.

Once an RFC is accepted and the decision is settled, capture the short version as an
[ADR](../decisions/).
