"""Check Unity handoff integrity without changing project assets."""
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
checks = []
settings = (ROOT / 'ProjectSettings/EditorBuildSettings.asset').read_text(encoding='utf-8-sig')
for scene, guid in re.findall(r'path: (.+)\n\s+guid: (\w+)', settings):
    scene = scene.strip()
    assert (ROOT / scene).is_file(), f'Missing build scene: {scene}'
    assert f'guid: {guid}' in (ROOT / (scene + '.meta')).read_text(), f'Scene GUID mismatch: {scene}'
    checks.append(f'Build scene exists and GUID matches: {scene}')

guids = {}
duplicates = []
for meta in (ROOT / 'Assets').rglob('*.meta'):
    match = re.search(r'^guid: (\w+)', meta.read_text(encoding='utf-8-sig'), re.M)
    if match:
        guid = match[1]
        if guid in guids: duplicates.append([str(guids[guid]), str(meta)])
        guids[guid] = meta.relative_to(ROOT).as_posix()
assert not duplicates, f'Duplicate GUIDs: {duplicates}'
checks.append(f'No duplicate GUIDs among {len(guids)} asset metadata files')
for folder in ['Assets/_Scripts/Audio', 'Assets/Resources/Audio']:
    for asset in (ROOT / folder).rglob('*'):
        if asset.is_file() and asset.suffix != '.meta':
            assert Path(str(asset) + '.meta').is_file(), f'Missing metadata: {asset}'
checks.append('All runtime audio scripts and clips have metadata')
report = dict(passed=checks, duplicate_guids=duplicates)
(ROOT / 'Docs/QA/repository_audit.json').write_text(json.dumps(report, indent=2), encoding='utf-8')
print(f'PASS: {len(checks)} repository integrity checks')
