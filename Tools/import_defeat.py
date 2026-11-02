"""Import the user's FIASCO WAV as Defeat; preserve the complete recording and source."""
import hashlib
import json
from pathlib import Path
import shutil
import wave
import numpy as np

ROOT = Path(__file__).resolve().parents[1]
SOURCE = Path(r'C:\Users\Admin\Downloads\olenchic-fiasco-154915.wav')
FOLDER = ROOT/'Tools/AudioSources/Defeat'
ARCHIVE = FOLDER/SOURCE.name
TARGET = ROOT/'Assets/Resources/Audio/SFX/SFX_Defeat.wav'
EXPECTED = '7c137af17b2539f6c8eaa802008eda985eb0187ac66e173d063bf6dce327ef9d'

def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()

def run():
    origin = SOURCE if SOURCE.exists() else ARCHIVE
    assert sha(origin) == EXPECTED, 'Source differs from the inspected WAV'
    p = ROOT/'Docs/QA/audio_provenance.json'
    provenance = json.loads(p.read_text(encoding='utf-8-sig'))
    runtime_path = TARGET.relative_to(ROOT).as_posix()
    for path, digest in provenance['asset_sha256'].items():
        if path != runtime_path: assert sha(ROOT/path) == digest, 'Unrelated audio changed: '+path
    FOLDER.mkdir(parents=True,exist_ok=True)
    if origin != ARCHIVE: shutil.copy2(origin,ARCHIVE)
    with wave.open(str(ARCHIVE),'rb') as wav:
        assert wav.getnchannels()==2 and wav.getsampwidth()==2 and wav.getframerate()==44100
        rate = wav.getframerate()
        audio = np.frombuffer(wav.readframes(wav.getnframes()),'<i2').reshape(-1,2).astype(np.float64)/32768
    audio[:44] *= np.linspace(0,1,44)[:,None]
    audio[-882:] *= np.linspace(1,0,882)[:,None]
    audio *= .30/np.max(abs(audio))
    with wave.open(str(TARGET),'wb') as wav:
        wav.setparams((2,2,rate,0,'NONE','not compressed'))
        wav.writeframes(np.rint(audio*32767).astype('<i2').tobytes())
    record = dict(title='FIASCO',author='olenchic',license='Pixabay Content License (not CC0)',
                  source_page='https://pixabay.com/sound-effects/film-special-effects-fiasco-154915/',
                  license_url='https://pixabay.com/service/license-summary/',verified_date='2026-11-02',
                  delivery='User supplied WAV of selected Pixabay item; site lists MP3, conversion history not supplied',
                  original_path=ARCHIVE.relative_to(ROOT).as_posix(),original_sha256=sha(ARCHIVE),
                  original_seconds=len(audio)/rate,original_rate=rate,original_channels=2,
                  runtime_path=runtime_path,runtime_sha256=sha(TARGET),runtime_seconds=len(audio)/rate,
                  processing='Full duration, stereo PCM16 44100 Hz; 1ms fade-in / 20ms fade-out; peak normalized to 0.30; unchanged speed and pitch',
                  importer='Tools/import_defeat.py',importer_sha256=sha(Path(__file__)),numpy_version=np.__version__)
    (FOLDER/'source.json').write_text(json.dumps(record,indent=2),encoding='utf-8')
    provenance['asset_sha256'][runtime_path]=record['runtime_sha256']
    provenance.setdefault('external_assets',{})[runtime_path]=record
    count=len(provenance['external_assets'])
    provenance['method']=f'{len(provenance["runtime_assets"])-count} synthesized clips plus {count} documented external/user-provided clips; licenses recorded per asset'
    provenance['generator_sha256']=sha(ROOT/'Tools/audio_audit.py')
    p.write_text(json.dumps(provenance,indent=2),encoding='utf-8')
    print('Defeat imported:',record['runtime_seconds'],'seconds; stereo; peak 0.30; original and GUID preserved')

if __name__=='__main__': run()
