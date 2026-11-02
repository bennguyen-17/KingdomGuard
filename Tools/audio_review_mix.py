"""Render an audition file and dense mix from the real runtime PCM clips.

Does not replace a perceptual listening review. Output WAV is in ignored Builds/QA.
"""
from array import array
import json
import math
from pathlib import Path
import struct
import wave

ROOT = Path(__file__).resolve().parents[1]
RATE = 44100
clips = {}
for path in (ROOT / 'Assets/Resources/Audio').rglob('*.wav'):
    with wave.open(str(path), 'rb') as source:
        assert source.getnchannels() in (1,2) and source.getframerate() == RATE and source.getsampwidth() == 2
        pcm = array('h')
        pcm.frombytes(source.readframes(source.getnframes()))
        if source.getnchannels() == 2:
            pcm = array('h', (round((pcm[i]+pcm[i+1])/2) for i in range(0,len(pcm),2)))
        clips[path.stem] = pcm

isolated_names = ['SFX_Arrow_Shoot','SFX_Cannon_Shoot','SFX_Magic_Shoot','SFX_Hit','SFX_Enemy_Death','SFX_Tower_Place','SFX_Build_Failed','SFX_Base_Damage','SFX_UI_Click','SFX_Victory','SFX_Defeat']
isolated_end = 7 + sum(len(clips[name])/RATE + .35 for name in isolated_names)
stress_start = max(18, math.ceil(isolated_end + 1))
review_seconds = stress_start + 14
mix = array('f', [0]) * (RATE * review_seconds)
timeline = []
def add(name, start, gain=1, duration=None, loop=False):
    data = clips[name]
    count = len(data) if duration is None else round(duration * RATE)
    offset = round(start * RATE)
    for i in range(count):
        mix[offset+i] += data[i % len(data)] / 32768 * gain
    timeline.append(dict(clip=name, start_seconds=round(start, 3), duration=round(count/RATE,3), gain=gain))

# Isolated cue audition at default user settings; stereo music is downmixed only for this preview.
add('BGM_Preparation', 0, .7, 3)
add('BGM_Battle', 3.5, .7, 3)
cursor = 7.0
for name in isolated_names:
    add(name, cursor, .8 * .65)
    cursor += len(clips[name])/RATE + .35

# Dense hypothetical schedule at 100% user settings. Four combat voices maximum.
add('BGM_Battle', stress_start, 1, 10)
active_until = [stress_start] * 4
events = [('SFX_Arrow_Shoot',.45),('SFX_Cannon_Shoot',.65),('SFX_Magic_Shoot',.5),('SFX_Hit',.35),('SFX_Enemy_Death',.5)]
last = {}
accepted = dropped = 0
for tick in range(250):
    now = stress_start + tick * .04
    name, gain = events[tick % len(events)]
    free = next((i for i, until in enumerate(active_until) if until <= now), None)
    if free is None or now - last.get(name, -100) < .1:
        dropped += 1
        continue
    add(name, now, .65 * gain)
    active_until[free] = now + len(clips[name])/RATE
    last[name] = now
    accepted += 1
for start in [stress_start + offset for offset in [1, 3, 5, 7]]:
    add('SFX_Base_Damage', start, .65)
    add('SFX_UI_Click', start + .1, .65)
peak = max(abs(sample) for sample in mix)
assert peak < 1, f'Simulated mix clips: {peak}'
pcm = array('h', (round(sample * 32767) for sample in mix))
output = ROOT / 'Builds/QA/AudioReview.wav'
output.parent.mkdir(parents=True, exist_ok=True)
with wave.open(str(output), 'wb') as dest:
    dest.setparams((1,2,RATE,0,'NONE','not compressed'))
    dest.writeframes(pcm.tobytes())
report = dict(output=str(output.relative_to(ROOT)), seconds=review_seconds, peak=round(peak,6),
              peak_dbfs=round(20*math.log10(peak),2), clipped_samples=0,
              accepted_stress_events=accepted, dropped_stress_events=dropped,
              warning='Offline hypothetical mix, not a recording of Unity output and not a listening signoff', timeline=timeline)
(ROOT / 'Docs/QA/audio_review_mix.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(f'PASS: review WAV {output}; simulated peak={peak:.3f}; no full-scale clipping')
