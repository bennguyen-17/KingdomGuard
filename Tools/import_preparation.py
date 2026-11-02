"""Prepare the user-provided WAV as stereo Preparation music; never modifies Downloads."""
import hashlib
import json
from pathlib import Path
import shutil
import wave
import numpy as np

ROOT = Path(__file__).resolve().parents[1]
SOURCE = Path(r'C:\Users\Admin\Downloads\Genshin-Impact-Tender-Strength-_wip_.wav')
SOURCE_DIR = ROOT / 'Tools/AudioSources/Preparation'
ARCHIVE = SOURCE_DIR / 'TenderStrength_user.wav'
TARGET = ROOT / 'Assets/Resources/Audio/BGM/BGM_Preparation.wav'
EXPECTED = 'a6797f4175e937d34ff33e664a3a5275c91f629ee7bfdb6dd22456d548920aa6'
def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()

def run():
    origin = SOURCE if SOURCE.exists() else ARCHIVE
    assert sha(origin) == EXPECTED, 'The supplied music file differs from the inspected WAV'
    SOURCE_DIR.mkdir(parents=True, exist_ok=True)
    if origin != ARCHIVE: shutil.copy2(origin, ARCHIVE)
    with wave.open(str(ARCHIVE), 'rb') as source:
        rate, channels, width = source.getframerate(), source.getnchannels(), source.getsampwidth()
        assert rate == 44100 and channels == 2 and width == 2
        audio = np.frombuffer(source.readframes(source.getnframes()), dtype='<i2').astype(np.float64).reshape(-1, channels)/32768
    original_frames = len(audio)
    active = np.flatnonzero(np.max(np.abs(audio), axis=1) > .001)
    assert len(active), 'Source is silent'
    end = min(len(audio), int(active[-1]) + 1 + rate//4)
    audio = audio[:end].copy()
    attack, release = min(882, len(audio)), min(17640, len(audio))
    audio[:attack] *= np.linspace(0, 1, attack)[:,None]
    audio[-release:] *= np.linspace(1, 0, release)[:,None]
    audio *= .20 / np.max(np.abs(audio))
    pcm = np.rint(audio*32767).astype('<i2')
    provenance_file = ROOT / 'Docs/QA/audio_provenance.json'
    provenance = json.loads(provenance_file.read_text(encoding='utf-8-sig'))
    runtime_path = TARGET.relative_to(ROOT).as_posix()
    for path, digest in provenance['asset_sha256'].items():
        if path != runtime_path: assert sha(ROOT/path) == digest, 'Unrelated audio changed: '+path
    with wave.open(str(TARGET), 'wb') as output:
        output.setparams((channels,2,rate,0,'NONE','not compressed')); output.writeframes(pcm.tobytes())
    record = dict(title='Tender Strength (user-provided wip WAV; title inferred from filename)',
                  author='Not supplied', license='Specific license not supplied; not identified as CC0',
                  source_page='User states the download came from an official site; URL not supplied',
                  verified_date='2026-11-02', original_path=ARCHIVE.relative_to(ROOT).as_posix(), original_sha256=sha(ARCHIVE),
                  original_seconds=original_frames/rate, original_rate=rate, original_channels=channels,
                  runtime_path=runtime_path, runtime_sha256=sha(TARGET), runtime_seconds=len(pcm)/rate,
                  processing='Preserve stereo PCM16 44100 Hz; trim near-silent tail at -60dB threshold plus 250ms decay; 20ms fade-in and 400ms fade-out; normalize peak to 0.20',
                  importer='Tools/import_preparation.py', importer_sha256=sha(Path(__file__)), numpy_version=np.__version__)
    (SOURCE_DIR/'source.json').write_text(json.dumps(record,indent=2),encoding='utf-8')
    provenance['asset_sha256'][runtime_path] = record['runtime_sha256']
    provenance.setdefault('external_assets',{})[runtime_path] = record
    count = len(provenance['external_assets'])
    provenance['method'] = f'{len(provenance["runtime_assets"])-count} synthesized clips plus {count} documented external/user-provided clips; licenses are recorded per asset'
    provenance['generator_sha256'] = sha(ROOT/'Tools/audio_audit.py')
    provenance_file.write_text(json.dumps(provenance,indent=2),encoding='utf-8')
    print('Preparation imported:',record['runtime_seconds'],'seconds; stereo; peak 0.20; original and Unity GUID preserved')

if __name__ == '__main__': run()
