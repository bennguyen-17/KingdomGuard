"""Import the user-selected CC0 Coin Splash; optional requirements in requirements-audio-import.txt."""
import hashlib
import json
import math
from pathlib import Path
import wave

import numpy as np
import scipy
from scipy.signal import resample_poly
import soundfile as sf

ROOT = Path(__file__).resolve().parents[1]
SOURCE_DIR = ROOT / 'Tools/AudioSources/CoinSplash'
SOURCE = SOURCE_DIR / 'coinsplash_0.ogg'
TARGET = ROOT / 'Assets/Resources/Audio/SFX/SFX_Tower_Place.wav'
EXPECTED_SOURCE = '1918a8e2eb94697f4710d1d47568e5194f4587f26ff0305c73cf8f8e28e15cc7'
def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()

def run():
    assert sha(SOURCE) == EXPECTED_SOURCE, 'Source download differs from the reviewed Coin Splash'
    provenance_file = ROOT / 'Docs/QA/audio_provenance.json'
    provenance = json.loads(provenance_file.read_text(encoding='utf-8-sig'))
    target_path = TARGET.relative_to(ROOT).as_posix()
    for path, digest in provenance['asset_sha256'].items():
        if path != target_path:
            assert sha(ROOT / path) == digest, 'Unexpected unrelated audio change: ' + path
    original, rate = sf.read(str(SOURCE), dtype='float64', always_2d=True)
    mono = original.mean(axis=1)
    divisor = math.gcd(rate, 44100)
    converted = resample_poly(mono, 44100 // divisor, rate // divisor)
    # Preserve the complete 0.33s coin clatter; soften only the endpoint samples.
    attack, release = min(44, len(converted)), min(265, len(converted))
    converted[:attack] *= np.linspace(0, 1, attack)
    converted[-release:] *= np.linspace(1, 0, release)
    converted *= .25 / np.max(np.abs(converted))
    pcm = np.rint(converted * 32767).astype('<i2')
    with wave.open(str(TARGET), 'wb') as output:
        output.setparams((1, 2, 44100, 0, 'NONE', 'not compressed'))
        output.writeframes(pcm.tobytes())
    manifest = dict(
        title='Coin Splash', author='LordTomorrow', license='CC0 1.0',
        source_page='https://opengameart.org/content/coin-splash',
        download_url='https://opengameart.org/sites/default/files/coinsplash_0.ogg',
        license_url='https://creativecommons.org/publicdomain/zero/1.0/', verified_date='2026-11-02',
        original_path=SOURCE.relative_to(ROOT).as_posix(), original_sha256=sha(SOURCE),
        original_seconds=len(original)/rate, original_rate=rate, original_channels=original.shape[1],
        runtime_path=target_path, runtime_sha256=sha(TARGET), runtime_seconds=len(pcm)/44100,
        processing='Mono, resample to 44100 Hz, 1ms fade-in / 6ms fade-out, normalize peak to 0.25, PCM16 WAV; original clatter retained',
        importer='Tools/import_coin_splash.py', importer_sha256=sha(Path(__file__)),
        library_versions=dict(numpy=np.__version__, scipy=scipy.__version__, soundfile=sf.__version__))
    (SOURCE_DIR / 'source.json').write_text(json.dumps(manifest, indent=2), encoding='utf-8')
    provenance['asset_sha256'][target_path] = manifest['runtime_sha256']
    provenance.setdefault('external_assets', {})[target_path] = manifest
    count = len(provenance['external_assets'])
    provenance['method'] = f'{len(provenance["runtime_assets"])-count} synthesized clips plus {count} documented external/user-provided clips; licenses are recorded per asset'
    provenance['generator_sha256'] = sha(ROOT / 'Tools/audio_audit.py')
    provenance_file.write_text(json.dumps(provenance, indent=2), encoding='utf-8')
    print('Imported Coin Splash:', manifest['runtime_seconds'], 'seconds; peak 0.25; .meta/GUID unchanged')

if __name__ == '__main__': run()
