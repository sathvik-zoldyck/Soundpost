# Scripts

Developer and build helper scripts. Keep them cross-shell-friendly where possible (PowerShell for
Windows-specific tasks) and documented at the top of each file.

Ideas that belong here: a clean/rebuild helper, a packaging script (MSIX / portable zip), a release
helper, a format+lint pre-push check.

Nothing here should be required to *build* the project — `dotnet build` from the root must always
just work.
