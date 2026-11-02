# Coin Splash source

- Title: Coin Splash
- Author: LordTomorrow
- Source page: https://opengameart.org/content/coin-splash
- Original file: https://opengameart.org/sites/default/files/coinsplash_0.ogg
- License shown by the source: CC0 1.0, https://creativecommons.org/publicdomain/zero/1.0/
- Verified: 2026-11-02; selected by the user as reference number 3.

The original download is kept here, outside Assets, for traceability. The processed runtime file is Assets/Resources/Audio/SFX/SFX_Tower_Place.wav. Its existing Unity GUID is preserved. It is used for successful purchase, upgrade and sale confirmations.

Processing preserves the complete short coin clatter, converts to mono PCM16 / 44.1 kHz, fades only the endpoints and normalizes peak amplitude to 0.25 to retain the existing mix headroom. See source.json for original/processed hashes and processing versions.

To recreate the runtime file, install the optional packages in Tools/requirements-audio-import.txt and run python Tools/import_coin_splash.py. The standard audio audit and game do not require these packages. Regenerating synthesized cues with audio_audit.py preserves this selected external clip.
