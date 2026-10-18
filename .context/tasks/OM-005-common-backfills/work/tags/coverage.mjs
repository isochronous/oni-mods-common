// Tag coverage per node, two denominators: all sections, and sections that carry prose of their own.
import fs from 'node:fs';
import path from 'node:path';
import { pathToFileURL } from 'node:url';

const S = await import(pathToFileURL('c:/dev/ai/icon/lib/backfill/survey.mjs').href);
const STORE = 'C:/dev/ONI-Mods/common';
const full = S.surveyStore(STORE);
const rows = {};
for (const d of full.documents) {
  const text = fs.readFileSync(path.join(STORE, d.source_path), 'utf8');
  const r = (rows[d.node] ??= { docs: 0, docsTagged: 0, secs: 0, own: 0, eff: 0, prose: 0, proseEff: 0 });
  r.docs++;
  if (d.tags.length) r.docsTagged++;
  const byAddr = new Map(d.sections.map((s) => [s.address, s]));
  for (const s of d.sections) {
    r.secs++;
    const ancestors = [];
    const parts = (s.address ?? '').split('/');
    for (let i = 1; i < parts.length; i++) ancestors.push(byAddr.get(parts.slice(0, i).join('/')));
    const effective = s.tags.length > 0 || d.tags.length > 0 || ancestors.some((a) => a && a.tags.length);
    if (s.tags.length) r.own++;
    if (effective) r.eff++;
    // prose of its own: non-heading, non-field lines between this heading and the next heading
    const lines = text.split('\n');
    const at = lines.findIndex((l) => /^#{2,6} /.test(l) && l.replace(/^#+ /, '').trim() === s.heading);
    let has = false;
    for (let i = at + 1; at >= 0 && i < lines.length; i++) {
      const l = lines[i];
      if (/^#{1,6} /.test(l)) break;
      if (l.trim() && !/^\*\*(Id|Tags|Status):\*\*/.test(l)) { has = true; break; }
    }
    if (has) { r.prose++; if (effective) r.proseEff++; }
  }
}
const pct = (a, b) => (b ? `${Math.round((100 * a) / b)}%` : '-');
console.log('node\tdocs tagged\tsections own-tag\teffective (all)\teffective (with prose)');
const tot = { docs: 0, docsTagged: 0, secs: 0, own: 0, eff: 0, prose: 0, proseEff: 0 };
for (const [n, r] of Object.entries(rows)) {
  for (const k of Object.keys(tot)) tot[k] += r[k];
  console.log(`${n}\t${r.docsTagged}/${r.docs}\t${r.own}/${r.secs}\t${pct(r.eff, r.secs)}\t${pct(r.proseEff, r.prose)}`);
}
console.log(`TOTAL\t${tot.docsTagged}/${tot.docs}\t${tot.own}/${tot.secs}\t${pct(tot.eff, tot.secs)} (${tot.eff}/${tot.secs})\t${pct(tot.proseEff, tot.prose)} (${tot.proseEff}/${tot.prose})`);
