# User-provided Preparation music

The user supplied Genshin-Impact-Tender-Strength-_wip_.wav from Downloads and stated it came from an official site. An exact source URL, arranger and license text were not supplied. This asset is not labelled CC0 or original synthesis.

The original stereo 44.1 kHz PCM16 WAV is archived as TenderStrength_user.wav. The Downloads file is never modified. See source.json for hashes, dimensions and processing. The runtime file retains stereo, removes a near-silent tail with 250ms decay retained, applies short endpoint fades and normalizes peak to 0.20 to fit the existing game mix.

Run python Tools/import_preparation.py to recreate the processed version using NumPy. If the original Downloads path is unavailable, it uses the archive. The standard audio audit checks both source and runtime hashes, and --generate-originals preserves the selected music.

Endpoint continuity is checked numerically; fades alone do not establish a musically seamless loop. Listen through the repeat in the game to judge phrasing. Credits must use the actual source/arranger/license if subsequently provided.
