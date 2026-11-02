# Kingdom Guard audio credits

Updated 2026-11-02.

| Runtime file | Work and author | Source | License and changes |
|---|---|---|---|
| Assets/Resources/Audio/SFX/SFX_Tower_Place.wav | Coin Splash by LordTomorrow | https://opengameart.org/content/coin-splash | Source lists CC0 1.0. Converted from OGG to mono PCM16 WAV, resampled 48 kHz to 44.1 kHz, endpoint fades and peak normalization to 0.25. Used for successful purchase, upgrade and sale confirmations. |
| Assets/Resources/Audio/BGM/BGM_Preparation.wav | Tender Strength, user-provided wip WAV; author/arranger not supplied | User states the file came from an official site; exact URL not supplied | Specific license not supplied. Not CC0 or original synthesis. Original archived; stereo preserved, near-silent tail trimmed, endpoint fades and peak normalization to 0.20. |
| Assets/Resources/Audio/BGM/BGM_Battle.wav, BGM_Battle_Medium.wav, BGM_Battle_High.wav | All-Conquering Tide; title inferred from user filename, author not supplied | User-provided local WAV: 3-01.-All-Conquering-Tide.wav | Specific license and source URL not supplied. Stereo, unchanged speed/pitch, selected sections resampled 48 kHz to 44.1 kHz, common gain 0.20, cyclic overlap and endpoint fades. Not CC0 or original synthesis. |
| Assets/Resources/Audio/SFX/SFX_Victory.wav | Victory Fanfare by Universfield | https://pixabay.com/sound-effects/musical-victory-fanfare-152480/ | Pixabay Content License: https://pixabay.com/service/license-summary/ . User supplied WAV of selected item (site lists MP3; conversion history not supplied). Full duration and stereo retained; PCM16 44.1 kHz, 1ms/20ms endpoint fades, peak normalized to 0.30. Not CC0. |
| Assets/Resources/Audio/SFX/SFX_Defeat.wav | FIASCO by olenchic | https://pixabay.com/sound-effects/film-special-effects-fiasco-154915/ | Pixabay Content License: https://pixabay.com/service/license-summary/ . User supplied WAV of selected item (site lists MP3; conversion history not supplied). Full duration and stereo retained; PCM16 44.1 kHz, 1ms/20ms endpoint fades, peak normalized to 0.30. Not CC0. |
| Other 8 clips under Assets/Resources/Audio | Original mathematical synthesis for Kingdom Guard | Tools/audio_audit.py | No external recorded samples used. |

Coin Splash original download: https://opengameart.org/sites/default/files/coinsplash_0.ogg

CC0 source license link: https://creativecommons.org/publicdomain/zero/1.0/

The original OGG and processing metadata are in Tools/AudioSources/CoinSplash. User-provided recordings and processing records are in Tools/AudioSources/Preparation, Tools/AudioSources/Battle, Tools/AudioSources/Victory and Tools/AudioSources/Defeat. Processed runtime hashes and per-asset source records are in Docs/QA/audio_provenance.json. Regenerating synthesized cues preserves all selected external files. Do not apply the Coin Splash CC0 label to the other selected recordings.

Assets/Audio contains legacy files with unverified provenance. That folder is not the runtime audio source and must not be treated as a reviewed asset pack.
