---
doc_id: doc-d0a268a9639f
description: "Dated entries of friction with ICON's ks engine, skills, hooks and agents met while working in oni-mods-common: the invocation, what it did, what was expected, what was done instead"
---
# Tooling friction

## 2026-11-12 OM-014 unit 05: ks read on a handoff packet
**Id:** blk-d8d44bb39b16

- Invocation: `ks read C:/dev/ONI-Mods/common/.context/tasks/OM-014-onimoono-direct-launch/handoff-5.md` (the unit's prompt said to read the packet first, then `ks read` every pointer in it).
- What it did: refused — "names no record in this store".
- Expected: the packet to open through the same verb as its pointers, or the prompt to make plain that the packet is a plain file.
- Instead: the unit read the packet as a plain file, then used `ks read` for the pointers. Manager prompts should say "open the packet file directly; `ks read` is for the pointers inside it".
