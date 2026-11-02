"""Import the selected Universfield fanfare, preserving stereo and the full reverb tail."""
import hashlib
import json
from pathlib import Path
import shutil
import wave
import numpy as np

ROOT = Path(__file__).resolve().parents[1]
SOURCE = Path(r'C:\Users\Admin\Downloads\universfield-victory-fanfare-152480.wav')
FOLDER = ROOT / 'Tools/AudioSources/Victory'
ARCHIVE = FOLDER / SOURCE.name
TARGET = ROOT / 'Assets/Resources/Audio/SFX/SFX_Victory.wav'
EXPECTED = 'aa21f005115151a5c37b77c5891b8e06ae817dac7e156e0e1a660fc32eeffab7'

def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()

def run():
    origin = SOURCE if SOURCE.exists() else ARCHIVE
    assert sha(origin) == EXPECTED, 'Source differs from the inspected user-provided WAV'
    provenance_file = ROOT/'Docs/QA/audio_provenance.json'
    provenance = json.loads(provenance_file.read_text(encoding='utf-8-sig'))
    runtime_path = TARGET.relative_to(ROOT).as_posix()
    for path, digest in provenance['asset_sha256'].items():
        if path != runtime_path: assert sha(ROOT/path) == digest, 'Unrelated audio changed: '+path
    FOLDER.mkdir(parents=True, exist_ok=True)
    if origin != ARCHIVE: shutil.copy2(origin, ARCHIVE)
    with wave.open(str(ARCHIVE), 'rb') as wav:
        assert wav.getnchannels() == 2 and wav.getsampwidth() == 2 and wav.getframerate() == 44100
        rate = wav.getframerate()
        audio = np.frombuffer(wav.readframes(wav.getnframes()), '<i2').reshape(-1,2).astype(np.float64)/32768
    # Keep all frames: do not gate or trim the orchestral reverb tail.
    audio[:44] *= np.linspace(0,1,44)[:,None]
    audio[-882:] *= np.linspace(1,0,882)[:,None]
    audio *= .30 / np.max(abs(audio))
    with wave.open(str(TARGET),'wb') as wav:
        wav.setparams((2,2,rate,0,'NONE','not compressed'))
        wav.writeframes(np.rint(audio*32767).astype('<i2').tobytes())
    record = dict(title='Victory Fanfare', author='Universfield', license='Pixabay Content License (not CC0)',
                  source_page='https://pixabay.com/sound-effects/musical-victory-fanfare-152480/',
                  license_url='https://pixabay.com/service/license-summary/', verified_date='2026-11-02',
                  delivery='User supplied WAV downloaded from the selected Pixabay item; original site lists MP3, conversion history not supplied',
                  original_path=ARCHIVE.relative_to(ROOT).as_posix(), original_sha256=sha(ARCHIVE),
                  original_seconds=len(audio)/rate, original_rate=rate, original_channels=2,
                  runtime_path=runtime_path, runtime_sha256=sha(TARGET), runtime_seconds=len(audio)/rate,
                  processing='Keep full duration, stereo PCM16 44100 Hz; 1ms fade-in and 20ms fade-out; normalize peak to 0.30; no speed/pitch change or added reverb',
                  importer='Tools/import_victory.py', importer_sha256=sha(Path(__file__)), numpy_version=np.__version__)
    (FOLDER/'source.json').write_text(json.dumps(record,indent=2),encoding='utf-8')
    provenance['asset_sha256'][runtime_path] = record['runtime_sha256']
    provenance.setdefault('external_assets',{})[runtime_path] = record
    count = len(provenance['external_assets'])
    provenance['method'] = f'{len(provenance["runtime_assets"])-count} synthesized clips plus {count} documented external/user-provided clips; licenses recorded per asset'
    provenance['generator_sha256'] = sha(ROOT/'Tools/audio_audit.py')
    provenance_file.write_text(json.dumps(provenance,indent=2),encoding='utf-8')
    print('Victory imported:',record['runtime_seconds'],'seconds; stereo; peak 0.30; original and Unity GUID preserved')

if __name__ == '__main__': run()
