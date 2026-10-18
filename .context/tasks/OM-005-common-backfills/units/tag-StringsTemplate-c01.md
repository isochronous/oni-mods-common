# Tagging pass tagout-StringsTemplate-c01 (model tier: default)

The brief below is the exact wording the ICON tag-backfill skill sends to a tagging pass. Follow it.

Notes from the manager, outside the brief:
- The output file already exists, empty: the manager opened it as this pass's live mark. Write into it and nothing else. After each batch of about 10 sections, rewrite the whole JSON object into that file so it always holds everything you have so far. Do not rename it, and do not create any other file there.
- The vocabulary file is the dictionary listing printed by `ks tags --glosses`: each term with its gloss and its current count. Use only those terms, spelled exactly as listed.
- Put the brief's closing report (coverage, 1/2/3/4 distribution, anything WRONG) in your final reply, not in the file.

---

Tag sections in a documentation store.

## 1. The vocabulary

`C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/work/tags/glosses-StringsTemplate.txt`

## 2. The sections

`C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/work/tags/input-StringsTemplate-c01.json`

Each document carries `document_tags` and a list of `sections` needing tags. Each section in that
list carries its own `text`, its heading `level`, and `enclosing_tags` — the terms carried by the
headings enclosing it, outermost first, not counting the document's own tags.

If a document's `document_tags` is empty, **choose 1 to 5 terms for the document itself** — what the
whole document is about — and return them. If it already has tags, keep them and tag only sections.

**A document marked `document_tags_declined` carries no document tags by decision.** Give it no document answer.
Tag its sections as sections of a document with no tags: a section about a subject the document would
otherwise have carried declares that term itself.

**A document abstains the same way a section does, with one difference: `inherits` is not available
to it.** A document has nothing above it whose tags could already cover it, so an empty answer from a
document is never "already covered" — it is always a gap. Its only reason is `"no-good-match"`, and
it takes a `suggestion` naming the concept the whole document IS about that no term covers — as a
short plain noun phrase, not a term name.

## 3. A section may carry several tags, and that is normal — not a compromise

Tags are not mutually exclusive. When two terms both genuinely fit, assign BOTH. Do not treat it as
a choice to be settled, do not agonise over which fits better, and do not drop one to look decisive.
A term that genuinely describes the section earns its place whether or not a neighbour also does.
The only thing to avoid is a term that does not genuinely fit.

Assign 1 to 4 terms — every term that genuinely applies.

## 4. Never assign a term the section's document, or a heading above the section, already carries

A section inheriting a term its document or an enclosing heading has is invisible to search: the query
returns every section under it as identical pointers. Assign only terms neither the document nor any
heading above the section carries — a heading's own `**Tags:**` line in the text is what it carries.
`enclosing_tags` on the section already names every term an enclosing heading carries, so check there
before assigning.

**This applies to the document tags you choose in this same call**, not only to ones it arrived with.

    { "id": "domains/write-path.md#the-command-and-its-shape", "tags": ["base-digest"] }

    ^ this document is tagged `write-path`. The section does NOT repeat it.

## 5. Every section should earn a tag its document does not have — a blank is a failure

**A section carrying only what it inherits adds a pointer and no information.** A document with ten
such sections returns eleven pointers that are identical in tag terms; a query cannot choose among
them, so it returns the whole document and spends the budget. **So look for the distinguishing facet
rather than for a reason to abstain.**

**The one exception:** a section that merely **introduces** the document, or **restates its rule**,
with no subject of its own. That is `inherits`, and it is correct — but it is narrow. **A section
about a specific thing is not an introduction merely because that thing sits within its document's
subject.**

⚠ **When a section genuinely has nothing left to declare, the conclusion is NOT that the section is
fine. It is that the vocabulary is too coarse to name what it is about** — which is a gap, and gaps
are how the vocabulary grows.

So an empty `tags` array is valid, and it must say which of the two it is:

- `"reason": "inherits"` — **the document's tags, or a heading above this section, cover this section.** Nothing is missing. Do not
  invent a suggestion for these. **This includes the case where the term you WANTED to assign is one
  the document already carries** — the right term exists and already reaches this section, so nothing
  is missing from the vocabulary. That is `inherits`, not a gap.
- `"reason": "no-good-match"` — **this section IS about something distinct, and no term fits it.**
  This one takes a `suggestion` naming the missing concept — what sets the section apart from its
  siblings — as a short plain noun phrase, not a term name.

An empty array with no `reason` is not a valid answer, and a `no-good-match` with no `suggestion` is
not either.

## Output

Write to `C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/work/tags/tagout-StringsTemplate-c01.partial`, appending in batches of about 10 sections. Do not hold the whole result to write
once at the end.

{ "documents": [ { "id": "domains/write-path.md", "tags": ["write-path"] },
                 { "id": "...", "tags": [], "reason": "no-good-match", "suggestion": "..." } ],
  "sections":  [ { "id": "...", "tags": ["term-a", "term-b"] },
                 { "id": "...", "tags": [], "reason": "inherits" },
                 { "id": "...", "tags": [], "reason": "no-good-match", "suggestion": "..." } ] }

Every section id from the input, exactly once. Every document whose `document_tags` was empty,
exactly once — omit the ones that already had tags, and the ones marked `document_tags_declined`. Terms spelled as the vocabulary spells them.

Report: coverage, your 1/2/3/4 tag distribution, and anything in this brief you found WRONG.
