// Checks what Unity would otherwise only report when the package is imported:
// package.json fields, one .meta per asset, no orphaned .meta, no duplicate GUIDs.
// Usage: node "Tools~/check-package.mjs" [package root]
import { readFileSync, readdirSync, existsSync, statSync } from 'node:fs';
import { join, relative } from 'node:path';

const root = process.argv[2] ?? '.';
const problems = [];

const pkg = JSON.parse(readFileSync(join(root, 'package.json'), 'utf8'));
if (!/^com\.[a-z0-9-]+(\.[a-z0-9-]+)+$/.test(pkg.name ?? '')) problems.push(`package.json: bad name "${pkg.name}"`);
if (!/^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?$/.test(pkg.version ?? '')) problems.push(`package.json: bad version "${pkg.version}"`);
if (!/^\d{4}\.\d+$/.test(pkg.unity ?? '')) problems.push(`package.json: bad unity "${pkg.unity}"`);
for (const field of ['displayName', 'description', 'license']) {
  if (!pkg[field]) problems.push(`package.json: missing ${field}`);
}
for (const sample of pkg.samples ?? []) {
  if (!existsSync(join(root, sample.path))) problems.push(`package.json: sample path ${sample.path} does not exist`);
}

// Unity skips hidden entries and folders ending in "~". Samples~ is copied into Assets on import,
// so its contents need .meta files too.
const guids = new Map();

function walk(dir) {
  const names = readdirSync(dir);
  for (const name of names) {
    if (name.startsWith('.')) continue;
    const path = join(dir, name);
    const rel = relative(root, path);
    if (name.endsWith('.meta')) {
      const asset = name.slice(0, -5);
      if (!names.includes(asset)) problems.push(`${rel}: no matching asset`);
      const guid = /^guid: ([0-9a-f]{32})$/m.exec(readFileSync(path, 'utf8'))?.[1];
      if (!guid) problems.push(`${rel}: no guid`);
      else if (guids.has(guid)) problems.push(`${rel}: guid also used by ${guids.get(guid)}`);
      else guids.set(guid, rel);
      continue;
    }
    const isDir = statSync(path).isDirectory();
    if (name.endsWith('~')) {
      if (isDir && rel === 'Samples~') walk(path);
      continue;
    }
    if (!names.includes(`${name}.meta`)) problems.push(`${rel}: missing .meta`);
    if (isDir) walk(path);
  }
}

walk(root);

if (problems.length) {
  console.error(problems.join('\n'));
  process.exit(1);
}
console.log(`${pkg.name} ${pkg.version}: package.json valid, ${guids.size} .meta files paired, GUIDs unique`);
