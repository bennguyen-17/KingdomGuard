"""Offline PCM audit; no claim about perceived loudness or musical quality."""
import hashlib
import json
import math
import random
from pathlib import Path
import struct
import wave

ROOT = Path(__file__).resolve().parents[1]
AUDIO = ROOT / 'Assets/Resources/Audio'

def write_tone(path, seconds, sample):
    rate = 44100
    values = [max(-32767, min(32767, round(sample(i / rate) * 32767))) for i in range(round(seconds * rate))]
    path.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(path), 'wb') as out:
        out.setparams((1, 2, rate, 0, 'NONE', 'not compressed'))
        out.writeframes(struct.pack('<' + 'h' * len(values), *values))

def generate():
    # Original synthesized cues. Regeneration is explicit; audit alone never overwrites assets.
    notes = [261.6256, 329.6276, 391.9954, 523.2511, 220, 261.6256, 329.6276, 440,
             174.6141, 220, 261.6256, 349.2282, 195.9977, 246.9417, 293.6648, 391.9954]
    def music(t):
        local = t % 1
        envelope = math.sin(math.pi * local) ** 2
        f = notes[int(t) % len(notes)]
        return .18 * envelope * (math.sin(2 * math.pi * f * local) + .25 * math.sin(4 * math.pi * f * local))
    write_tone(AUDIO / 'BGM/BGM_Preparation.wav', 16, music)
    write_tone(AUDIO / 'SFX/SFX_Build_Failed.wav', .25,
               lambda t: .24 * math.sin(math.pi * t / .25) ** 2 * math.sin(2 * math.pi * (240 * t - 120 * t*t)))
    write_tone(AUDIO / 'SFX/SFX_Base_Damage.wav', .35,
               lambda t: .3 * math.sin(math.pi * t / .35) ** 2 * math.sin(2 * math.pi * (150 * t - 100 * t*t)))
    rng = random.Random(301)
    def env(t, duration):
        return math.sin(math.pi * t / duration) ** 2
    def battle(t):
        beat = int(t / .25)
        local = t % .25
        root = [130.8128, 110, 87.3071, 97.9989][int(t / 4) % 4]
        bass = .12 * env(local, .25) * math.sin(2 * math.pi * root * local)
        lead = .07 * env(local, .25) * math.sin(2 * math.pi * notes[beat % 16] * 2 * local)
        # Deterministic percussion, without recorded samples.
        kick = .12 * math.exp(-35 * local) * math.sin(2 * math.pi * (85 * local - 70 * local * local)) if beat % 2 == 0 else 0
        return bass + lead + kick
    write_tone(AUDIO / 'BGM/BGM_Battle.wav', 16, battle)
    write_tone(AUDIO / 'SFX/SFX_Arrow_Shoot.wav', .15,
               lambda t: env(t, .15) * (.20 * rng.uniform(-1, 1) + .08 * math.sin(2 * math.pi * (1900*t-4200*t*t))))
    write_tone(AUDIO / 'SFX/SFX_Cannon_Shoot.wav', .45,
               lambda t: env(t, .45) * math.exp(-5*t) * (.28 * math.sin(2*math.pi*(110*t-80*t*t)) + .18*rng.uniform(-1,1)))
    write_tone(AUDIO / 'SFX/SFX_Hit.wav', .12,
               lambda t: env(t,.12) * (.17*rng.uniform(-1,1) + .12*math.sin(2*math.pi*720*t)))
    write_tone(AUDIO / 'SFX/SFX_Enemy_Death.wav', .30,
               lambda t: .28*env(t,.30)*math.sin(2*math.pi*(360*t-380*t*t)))
    write_tone(AUDIO / 'SFX/SFX_Tower_Place.wav', .18,
               lambda t: .25*env(t,.18)*math.sin(2*math.pi*(520*t+800*t*t)))
    write_tone(AUDIO / 'SFX/SFX_UI_Click.wav', .06,
               lambda t: .20*env(t,.06)*math.sin(2*math.pi*900*t))
    def phrase(t, pitches):
        local = t % .225
        pitch = pitches[min(3, int(t/.225))]
        return .26*env(local,.225)*(math.sin(2*math.pi*pitch*local)+.15*math.sin(4*math.pi*pitch*local))
    write_tone(AUDIO / 'SFX/SFX_Victory.wav', .9,
               lambda t: phrase(t, [261.6256,329.6276,391.9954,523.2511]))
    write_tone(AUDIO / 'SFX/SFX_Defeat.wav', .9,
               lambda t: phrase(t, [391.9954,329.6276,261.6256,130.8128]))

def audit(generated=False):
    rows = []
    for path in sorted(AUDIO.rglob('*.wav')):
        with wave.open(str(path), 'rb') as audio:
            assert audio.getsampwidth() == 2, f'Unsupported PCM width: {path}'
            raw = audio.readframes(audio.getnframes())
            values = struct.unpack('<' + 'h' * (len(raw)//2), raw)
            peak = max(abs(v) for v in values) / 32768
            rms = math.sqrt(sum((v/32768)**2 for v in values) / len(values))
            rows.append(dict(path=path.relative_to(ROOT).as_posix(), seconds=round(audio.getnframes()/audio.getframerate(), 3),
                             channels=audio.getnchannels(), rate=audio.getframerate(), peak=round(peak, 5),
                             rms=round(rms, 5), clipped_samples=sum(abs(v) >= 32767 for v in values),
                             sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
    out = ROOT / 'Docs/QA/audio_audit.json'
    out.write_text(json.dumps(rows, indent=2), encoding='utf-8')
    assert len(rows) == 12, f'Expected 12 runtime clips, found {len(rows)}'
    assert all(r['peak'] > 0 and r['clipped_samples'] == 0 for r in rows), 'Silent or clipped asset'
    provenance_file = ROOT / 'Docs/QA/audio_provenance.json'
    hashes = {r['path']: r['sha256'] for r in rows}
    if generated:
        provenance = dict(generator='Tools/audio_audit.py', generator_sha256=hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),
                          method='Original mathematical synthesis; deterministic random seed 301; no external audio samples',
                          runtime_assets=[r['path'] for r in rows], asset_sha256=hashes,
                          legacy_folder='Assets/Audio: unknown external sources per user; preserved, not approved for redistribution')
        provenance_file.write_text(json.dumps(provenance, indent=2), encoding='utf-8')
    else:
        assert provenance_file.is_file(), 'Missing provenance; generate originals explicitly before claiming their source'
        provenance = json.loads(provenance_file.read_text(encoding='utf-8'))
        assert provenance['asset_sha256'] == hashes, 'Audio differs from recorded originals; review changes and source before accepting'
    print(f'PASS: {len(rows)} non-silent PCM clips, no full-scale clipped samples; {out}')

if __name__ == '__main__':
    import sys
    generated = '--generate-originals' in sys.argv
    if generated: generate()
    audit(generated)
