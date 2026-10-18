"""Write one tagging unit spec per chunk id: python mkspec.py <glosses-file> <chunk-id> [<chunk-id> ...]

The brief is copied verbatim from the ICON tag-backfill skill's tagging-brief.md (the fenced block);
only its three <path> placeholders are filled.
"""
import re
import sys
from pathlib import Path

ROOT = Path('C:/dev/ONI-Mods/common')
TASK = ROOT / '.context/tasks/OM-005-common-backfills'
TAGS = TASK / 'work/tags'
brief_md = Path('c:/dev/ai/icon/skills/tag-backfill/tagging-brief.md').read_text(encoding='utf-8')
brief = re.search(r'```\n(.*?)\n```', brief_md, re.S).group(1)

glosses = sys.argv[1]
for chunk in sys.argv[2:]:
    text = brief.replace('<path to the definitions file>', f'`{(TAGS / glosses).as_posix()}`')
    text = text.replace('<path to the corpus>', f'`{(TAGS / f"input-{chunk}.json").as_posix()}`')
    text = text.replace('Write to <path>,', f'Write to `{(TAGS / f"tagout-{chunk}.partial").as_posix()}`,')
    assert '<path' not in text, chunk
    spec = f"""# Tagging pass tagout-{chunk} (model tier: default)

The brief below is the exact wording the ICON tag-backfill skill sends to a tagging pass. Follow it.

Notes from the manager, outside the brief:
- The output file already exists, empty: the manager opened it as this pass's live mark. Write into it and nothing else. After each batch of about 10 sections, rewrite the whole JSON object into that file so it always holds everything you have so far. Do not rename it, and do not create any other file there.
- The vocabulary file is the dictionary listing printed by `ks tags --glosses`: each term with its gloss and its current count. Use only those terms, spelled exactly as listed.
- Put the brief's closing report (coverage, 1/2/3/4 distribution, anything WRONG) in your final reply, not in the file.

---

{text}
"""
    (TASK / 'units' / f'tag-{chunk}.md').write_text(spec, encoding='utf-8', newline='\n')
    print('wrote', f'units/tag-{chunk}.md')
