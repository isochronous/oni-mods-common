// OM-005 Phase D driver (common store). Usage: node tagwork.mjs <cmd> [...]
//   survey                      surveyStore over the store, split per node: survey-<slug>.json, lexicon-<slug>.json
//   input <node> <pass>         taggingInput for one node (against its current survey), written as input-<pass>.json
//   begin <pass>                open a pass (.partial) before dispatching its unit
//   complete <pass>             close a pass after the harness reports the unit finished
//   check <node> <pass>         readTaggingOutput + validateResponse against the node's expectation
import fs from 'node:fs';
import path from 'node:path';
import { pathToFileURL } from 'node:url';

const ICON = 'c:/dev/ai/icon/lib/backfill/';
const imp = (m) => import(pathToFileURL(ICON + m).href);
const STORE = 'C:/dev/ONI-Mods/common';
const DIR = path.dirname(new URL(import.meta.url).pathname.replace(/^\/([A-Za-z]:)/, '$1'));
const slug = (node) => (node === '.' ? 'root' : node.split('/').pop());
const [cmd, a1, a2] = process.argv.slice(2);
const RSUF = process.env.ROUND ? `-${process.env.ROUND}` : '';

const S = await imp('survey.mjs');
const I = await imp('intake.mjs');
const R = await imp('response.mjs');

const nodeSurvey = (full, node) => ({ ...full, documents: full.documents.filter((d) => d.node === node) });

if (cmd === 'survey') {
  const full = S.surveyStore(STORE);
  fs.writeFileSync(path.join(DIR, 'survey-full.json'), JSON.stringify(full, null, 1));
  const nodes = [...new Set(full.documents.map((d) => d.node))];
  for (const node of nodes) {
    const sv = nodeSurvey(full, node);
    fs.writeFileSync(path.join(DIR, `survey-${slug(node)}.json`), JSON.stringify(sv, null, 1));
    fs.writeFileSync(path.join(DIR, `lexicon-${slug(node)}.json`), JSON.stringify(S.lexiconInput(sv), null, 1));
    const secs = sv.documents.reduce((n, d) => n + d.sections.length, 0);
    const tagged = sv.documents.filter((d) => d.tags.length).length + sv.documents.reduce((n, d) => n + d.sections.filter((s) => s.tags.length).length, 0);
    console.log(`${node}\tdocs=${sv.documents.length}\tsections=${secs}\talready-tagged=${tagged}`);
  }
  console.log('problems', full.problems, 'excluded', JSON.stringify({ not_carried: full.excluded.not_carried.length, destination: full.excluded.destination }));
} else if (cmd === 'chunks') {
  // chunks <node>: freeze this node's survey and write one tagging input per chunk
  const full = S.surveyStore(STORE);
  const sv = nodeSurvey(full, a1);
  const sl = slug(a1) + RSUF;
  fs.writeFileSync(path.join(DIR, `survey-${sl}.json`), JSON.stringify(sv, null, 1));
  const { chunks, problems } = S.chunkDocuments(sv);
  if (problems.length) console.log('PROBLEMS', problems);
  const meta = [];
  for (const c of chunks) {
    const id = `${sl}-c${String(c.index).padStart(2, '0')}`;
    const csv = S.chunkSurvey(sv, c);
    fs.writeFileSync(path.join(DIR, `csurvey-${id}.json`), JSON.stringify(csv, null, 1));
    const input = S.taggingInput(csv);
    fs.writeFileSync(path.join(DIR, `input-${id}.json`), JSON.stringify(input, null, 1));
    meta.push({ id, kind: c.kind, documents: c.documents.map((d) => d.id), sections: c.sections, dependsOn: c.dependsOn ?? null });
  }
  fs.writeFileSync(path.join(DIR, `chunks-${sl}.json`), JSON.stringify(meta, null, 1));
  console.log(`${a1}: ${meta.map((m) => `${m.id}[${m.kind}] docs=${m.documents.length} secs=${m.sections}`).join('  ')}`);
} else if (cmd === 'begin') {
  I.beginPass(DIR, a1);
  console.log('opened', a1);
} else if (cmd === 'complete') {
  console.log(JSON.stringify(I.completePass(DIR, a1)));
} else if (cmd === 'check') {
  // check <node> <chunk-id>: validate one finished chunk output (pass id tagout-<chunk-id>)
  const sl = slug(a1);
  const chunkSv = JSON.parse(fs.readFileSync(path.join(DIR, `csurvey-${a2}.json`), 'utf8'));
  const got = I.readTaggingOutput(DIR, `tagout-${a2}`);
  if (!got.ok) { console.log('REFUSED', got.refused); process.exit(3); }
  const v = R.validateResponse(got.raw, S.expectationFor(chunkSv));
  console.log(JSON.stringify({ chunk: a2, keys: Object.keys(v), ok: v.ok, problems: v.problems }, null, 1));
} else if (cmd === 'write' || cmd === 'dry') {
  // write|dry <node> [round-tag]: merge the node's finished chunk outputs, verify, generate, run rounds
  const G = await imp('generate.mjs');
  const RUN = await imp('run.mjs');
  const sl = slug(a1) + RSUF;
  const tag = a2 ?? 'r1';
  const frozen = JSON.parse(fs.readFileSync(path.join(DIR, `survey-${sl}.json`), 'utf8'));
  const now = nodeSurvey(S.surveyStore(STORE), a1);
  const strip = (sv) => JSON.stringify(sv.documents);
  if (strip(frozen) !== strip(now)) { console.log('REFUSED: the node changed since its survey; re-survey and re-tag'); process.exit(4); }
  const meta = JSON.parse(fs.readFileSync(path.join(DIR, `chunks-${sl}.json`), 'utf8'));
  const merged = { documents: [], sections: [] };
  for (const m of meta) {
    const got = I.readTaggingOutput(DIR, `tagout-${m.id}`);
    if (!got.ok) { console.log('REFUSED', m.id, got.refused); process.exit(3); }
    const csv = JSON.parse(fs.readFileSync(path.join(DIR, `csurvey-${m.id}.json`), 'utf8'));
    const v = R.validateResponse(got.raw, S.expectationFor(csv));
    if (!v.ok) { console.log('INVALID', m.id, v.problems); process.exit(3); }
    merged.documents.push(...v.response.documents);
    merged.sections.push(...v.response.sections);
  }
  const nodeDir = a1 === '.' ? STORE : path.join(STORE, a1);
  const dict = fs.readFileSync(path.join(nodeDir, '.context/tags.md'), 'utf8');
  const terms = new Set([...dict.matchAll(/^## (.+)$/gm)].map((x) => x[1].trim()));
  const used = [...merged.documents, ...merged.sections].flatMap((e) => e.tags ?? []);
  const unknown = [...new Set(used.filter((t) => !terms.has(t)))];
  if (unknown.length) { console.log('UNDECLARED TERMS', unknown); process.exit(3); }
  const gen = G.generateOperations({ survey: frozen, response: merged });
  if (gen.problems.length) { console.log('GEN PROBLEMS', gen.problems); process.exit(3); }
  const gaps = R.gapsIn(merged);
  fs.writeFileSync(path.join(DIR, `gaps-${sl}-${tag}.json`), JSON.stringify(gaps, null, 1));
  const docTags = merged.documents.map((d) => `${d.id}: ${(d.tags || []).join(', ') || '(' + (d.reason || '') + ')'}`);
  const dist = {}; for (const e of merged.sections) { const n = (e.tags || []).length; dist[n] = (dist[n] || 0) + 1; }
  console.log(JSON.stringify({ node: a1, operations: gen.operations.length, subtracted: gen.subtracted.length, abstentions: gen.abstentions.length, gaps: gaps.length, sectionTagCountDist: dist, docTags }, null, 1));
  if (cmd === 'write') {
    const journal = path.join(DIR, 'journal');
    fs.mkdirSync(journal, { recursive: true });
    const out = RUN.runBackfill({ survey: frozen, operations: gen.operations, storeRoot: STORE, journalDir: journal, task: 'OM-005', why: `tag backfill ${tag} for node ${a1}` });
    console.log(JSON.stringify({ status: out.status, refused: out.refused ?? null, rounds: out.rounds.length }, null, 1));
  }
} else {
  console.log('unknown command');
  process.exit(2);
}
