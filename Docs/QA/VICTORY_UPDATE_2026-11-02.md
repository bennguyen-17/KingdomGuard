# Cập nhật Victory — 02/11/2026

Đã thay `Assets/Resources/Audio/SFX/SFX_Victory.wav` bằng **Victory Fanfare — Universfield**, mẫu số 1 người dùng chọn. Nguồn: [trang Pixabay](https://pixabay.com/sound-effects/musical-victory-fanfare-152480/), [Pixabay Content License](https://pixabay.com/service/license-summary/). File WAV được người dùng tải và cung cấp; trang gốc liệt kê MP3, không có thông tin về bước chuyển định dạng trước đó.

## Xử lý

- File nguồn: `C:\Users\Admin\Downloads\universfield-victory-fanfare-152480.wav`; giữ nguyên file Downloads và lưu bản sao trong `Tools/AudioSources/Victory`.
- Đầu vào/đầu ra: stereo PCM16, 44.100 Hz, **3,813878 giây**. Giữ toàn bộ số frame và đuôi vang, không đổi tốc độ/cao độ hoặc thêm reverb.
- Peak nguồn: **0,64639**; peak đầu ra: **0,30** (khoảng −10,46 dBFS). Thêm fade-in 1ms / fade-out 20ms ở biên để giảm click.
- Giữ `.meta` và GUID `575ff98423eaa4a469d768b721c33ff1`, tiếp tục dùng luồng Victory hiện có: dừng BGM/combat và phát fanfare một lần khi thắng. Effects kiểm soát âm lượng; có thể phát khi gameplay dừng ở màn kết quả.
- Metadata, nguồn và hash: `Tools/AudioSources/Victory/source.json`, `Docs/QA/audio_provenance.json`. Importer: `Tools/import_victory.py`. Lệnh sinh lại âm tổng hợp có guard để giữ bản Victory đã chọn.
- Runtime vẫn có **15 clip**: **9 tổng hợp + 6 clip từ các bản thu bên ngoài/người dùng**. Ghi công tại `Docs/AUDIO_CREDITS.md`.

## Kiểm tra và nghe thử

Audit PCM đạt 15/15 clip có tiếng, không có mẫu full-scale. Bound mix bảo thủ khoảng **0,89449 < 0,95**. Mix nghe thử offline peak khoảng **0,432**, không chạm full-scale. Repository audit đạt 6/6.

Unity QA đạt **62/62 kiểm tra Play Mode**. Bản Windows **Succeeded; errors=0; warnings=2**; smoke **4/4 scene** đạt, lưu/đọc lại âm lượng qua process mới đạt. Hai cảnh báo build còn lại là Unity Services chưa liên kết project và tham chiếu System.Windows.Forms; không có audio legacy được đóng gói. Kết quả: `unity_verification.json`, `sound_completion_2026-11-02.json` và `player_smoke.json`. Build mới: `Builds/QA/KingdomGuard.exe`.

Nghe riêng file runtime `Assets/Resources/Audio/SFX/SFX_Victory.wav`, hoặc file `Builds/QA/AudioReview.wav` tại khoảng **12,4–16,2 giây**. Trong game, thắng màn rồi đợi ít nhất 5 giây trước khi bấm chơi lại: nghe fanfare đủ đuôi, không lặp, không còn nhạc Battle; thử Effects=0 và bật lại. Chưa nghiệm thu chất lượng cảm nhận bằng tai.

## Tái tạo

```powershell
python Tools/import_victory.py
python Tools/audio_audit.py
python Tools/audio_review_mix.py
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/run_sound_qa.ps1
```

Importer cần NumPy; game và audit PCM không cần NumPy. Nếu file Downloads không còn, importer dùng bản archive và kiểm tra hash nguồn trước khi ghi.
