## What & why

<!-- What does this change do, and what problem does it solve? -->
<!-- Link the issue, e.g.: Fixes #123 -->

## How I tested it

<!-- Did you build? Run the probe (tools/Soundpost.Probe)? On which Windows version and with what devices (Bluetooth / HDMI / USB)? -->

## Checklist

- [ ] `dotnet build` passes
- [ ] `dotnet format --verify-no-changes` passes
- [ ] The COM firewall is intact — only `Soundpost.Core.Audio` touches audio COM / NAudio
- [ ] Change follows the [project principles](../CONTRIBUTING.md#project-principles)
- [ ] Docs/CHANGELOG updated if behavior changed
