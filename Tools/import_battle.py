"""Analyze and derive adaptive battle loops from the user's WAV; keep the original."""
import hashlib
import json
from pathlib import Path
import shutil
import uuid
import wave
import numpy as np
from scipy import signal

ROOT = Path(__file__).resolve().parents[1]
SOURCE = Path(r'C:\Users\Admin\Downloads\3-01.-All-Conquering-Tide.wav')
FOLDER = ROOT / 'Tools/AudioSources/Battle'
ARCHIVE = FOLDER / 'All-Conquering-Tide_user.wav'
RATE = 44100
SECTIONS = [('Calm', 128, 152, 'BGM_Battle'),
            ('Tense', 56, 80, 'BGM_Battle_Medium'),
            ('Intense', 200, 232, 'BGM_Battle_High')]

def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()

def write(path, audio):
    path.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(path), 'wb') as out:
        out.setparams((2, 2, RATE, 0, 'NONE', 'not compressed'))
        out.writeframes(np.rint(audio * 32767).astype('<i2').tobytes())

def run():
    origin = SOURCE if SOURCE.exists() else ARCHIVE
    FOLDER.mkdir(parents=True, exist_ok=True)
    if ARCHIVE.exists():
        assert sha(origin) == sha(ARCHIVE), 'Source changed since archive; review before replacing'
    if origin != ARCHIVE: shutil.copy2(origin, ARCHIVE)
    with wave.open(str(ARCHIVE), 'rb') as wav:
        assert wav.getsampwidth() == 2 and wav.getnchannels() == 2 and wav.getframerate() == 48000
        audio = np.frombuffer(wav.readframes(wav.getnframes()), '<i2').reshape(-1, 2).astype(np.float64)/32768
    # Spectral flux is positive spectral change, an acoustic proxy for activity,
    # not a claim to identify instruments or perceived musical intensity.
    mono = audio.mean(axis=1)
    freq, times, spectrum = signal.stft(mono[::6], 8000, nperseg=1024, noverlap=768)
    mag = abs(spectrum)
    flux = np.maximum(np.diff(mag, axis=1), 0).sum(axis=0)
    rows = []
    outputs = []
    previews = []
    gain = .20 / np.max(abs(audio))  # Same gain for all tiers; preserve source dynamics.
    for tier, start, end, name in SECTIONS:
        clip = audio[start*48000:end*48000]
        rows.append(dict(tier=tier, source_start=start, source_end=end,
                         source_rms=float(np.sqrt(np.mean(clip**2))),
                         mean_spectral_flux=float(flux[(times[1:]>=start)&(times[1:]<end)].mean())))
        clip = signal.resample_poly(clip, 147, 160, axis=0)*gain
        overlap = int(.8*RATE)
        ramp = np.linspace(0, 1, overlap)[:, None]
        # Wrap tail into head using a bounded linear crossfade.
        loop = np.concatenate([clip[overlap:-overlap], clip[-overlap:]*(1-ramp)+clip[:overlap]*ramp])
        # Very short endpoint ramps prevent clicks even if a source phrase is imperfect.
        edge = int(.005*RATE)
        loop[:edge] *= np.linspace(0,1,edge)[:,None]
        loop[-edge:] *= np.linspace(1,0,edge)[:,None]
        path = ROOT / f'Assets/Resources/Audio/BGM/{name}.wav'
        write(path, loop)
        meta = Path(str(path)+'.meta')
        if not meta.exists():
            template = (ROOT/'Assets/Resources/Audio/BGM/BGM_Battle.wav.meta').read_text()
            import re
            meta.write_text(re.sub(r'guid: [a-f0-9]+', 'guid: '+uuid.uuid4().hex, template))
        outputs.append(dict(tier=tier, runtime_path=path.relative_to(ROOT).as_posix(),
                            runtime_sha256=sha(path), runtime_seconds=len(loop)/RATE,
                            source_start=start, source_end=end))
        previews.append(loop[:RATE*12])
    # Standalone stereo audition: 12s calm -> tense -> intense -> calm, with 1s crossfades.
    demo = previews[0].copy()
    for part in [previews[1], previews[2], previews[0]]:
        n = RATE
        ramp = np.linspace(0,1,n)[:,None]
        demo = np.concatenate([demo[:-n], demo[-n:]*(1-ramp)+part[:n]*ramp, part[n:]])
    write(ROOT/'Builds/QA/BattleAdaptiveReview.wav', demo)
    import matplotlib
    matplotlib.use('Agg')
    import matplotlib.pyplot as plt
    fig, axes = plt.subplots(2, 1, figsize=(12, 6), sharex=True)
    blocks = len(audio)//48000
    rms = np.sqrt(np.mean(audio[:blocks*48000].reshape(blocks,48000,2)**2,axis=(1,2)))
    axes[0].plot(np.arange(blocks)+.5, rms, color='#34495e', lw=1)
    axes[0].set_ylabel('Stereo RMS (1s)')
    axes[1].plot(times[1:], flux, color='#697b8d', lw=.4, alpha=.8)
    axes[1].set_ylabel('Positive spectral flux')
    axes[1].set_xlabel('Source time (seconds)')
    for (tier,start,end,_), color in zip(SECTIONS, ['#42b883','#e8ac31','#e35c5c']):
        for ax in axes: ax.axvspan(start,end,color=color,alpha=.25,label=f'{tier}: {start}-{end}s')
    axes[0].legend(loc='upper left', ncol=3)
    axes[0].set_title('All-Conquering Tide: acoustic analysis and selected regions')
    for ax in axes: ax.grid(alpha=.15)
    fig.tight_layout()
    fig.savefig(ROOT/'Docs/QA/Visual/battle-analysis.png', dpi=150)
    plt.close(fig)
    record = dict(title='All-Conquering Tide (title inferred from user filename)',
                  author='Not supplied', license='Specific license and source URL not supplied',
                  source_page='WAV supplied locally by user', verified_date='2026-11-02',
                  original_path=ARCHIVE.relative_to(ROOT).as_posix(), original_sha256=sha(ARCHIVE),
                  original_seconds=len(audio)/48000, original_rate=48000, original_channels=2,
                  original_full_scale_samples=int(np.count_nonzero(abs(audio)>=32767/32768)),
                  processing='Stereo PCM16 44100 Hz; polyphase resampling 147/160; common gain 0.20; 800ms cyclic linear overlap; 5ms endpoint fades; unchanged pitch and speed. Section selection uses RMS and spectral flux; musical listening review pending.',
                  importer='Tools/import_battle.py', importer_sha256=sha(Path(__file__)),
                  runtime_assets=outputs, analysis=rows)
    (FOLDER/'source.json').write_text(json.dumps(record,indent=2),encoding='utf-8')
    (ROOT/'Docs/QA/battle_analysis.json').write_text(json.dumps(record,indent=2),encoding='utf-8')
    # Keep provenance truthful without regenerating or overwriting unrelated cues.
    p = ROOT/'Docs/QA/audio_provenance.json'
    provenance = json.loads(p.read_text(encoding='utf-8-sig'))
    for item in outputs:
        path = item['runtime_path']
        provenance['asset_sha256'][path] = item['runtime_sha256']
        provenance.setdefault('external_assets',{})[path] = {**record, **item}
    provenance['runtime_assets'] = sorted(provenance['asset_sha256'])
    external = len(provenance['external_assets'])
    provenance['method'] = f'{len(provenance["runtime_assets"])-external} synthesized clips plus {external} documented external/user-provided clips; licenses recorded per asset'
    provenance['generator_sha256'] = sha(ROOT/'Tools/audio_audit.py')
    p.write_text(json.dumps(provenance,indent=2),encoding='utf-8')
    print(json.dumps(dict(seconds=record['original_seconds'], sections=rows, outputs=outputs),indent=2))

if __name__ == '__main__': run()
