# Refactor playbook

Behavior-preserving, one module at a time. Bugs found on the way are reported, not fixed in the same change.

- **Layer 0 - Safety net.** `./check.ps1` green. Add characterization tests for the pure part of the module you are about to touch. For window/input code, write down the manual steps you will re-run.
- **Layer 1 - Mechanical.** Formatting, names, dead code, magic values. No structural change.
- **Layer 2 - Locality.** Move knowledge to where it is used: P/Invoke into its only user, constants next to their meaning, duplicated logic into one place.
- **Layer 3 - Depth.** Apply the deletion test to each type and method. Fold shallow ones into their caller or into the deep module they belong to. Pull pure decisions out of forms.
- **Layer 4 - Seams.** Only where a second adapter exists or a test needs one.

After each layer: `./check.ps1`, re-run the manual steps, update `architecture.md` and `deepening-log.md`.
