# Cập nhật Defeat — 02/11/2026

Đã thay `Assets/Resources/Audio/SFX/SFX_Defeat.wav` bằng **FIASCO — olenchic** theo lựa chọn người dùng. Nguồn được đối chiếu tại [Pixabay](https://pixabay.com/sound-effects/film-special-effects-fiasco-154915/), có nhãn Defeat/Comical và [Pixabay Content License](https://pixabay.com/service/license-summary/). WAV do người dùng cung cấp; trang gốc liệt kê MP3, lịch sử chuyển định dạng không được cung cấp.

## Xử lý

- File nguồn: `C:\Users\Admin\Downloads\olenchic-fiasco-154915.wav`; giữ nguyên Downloads, lưu bản sao tại `Tools/AudioSources/Defeat`.
- Stereo PCM16, 44.100 Hz, **3,343673 giây**; giữ toàn bộ số frame, không đổi tốc độ/cao độ.
- Peak nguồn **0,39175**, RMS **0,04027**. Chuẩn hóa peak đầu ra về **0,30**, cùng mức đỉnh mục tiêu với Victory; mức đỉnh bằng nhau không bảo đảm độ lớn cảm nhận bằng nhau.
- Fade-in 1ms / fade-out 20ms để làm mềm biên, không cắt ngắn phần âm chính.
- Giữ `.meta`/GUID hiện có để tiếp tục dùng routing Defeat: dừng BGM/combat, phát một lần khi thua; âm lượng theo Effects và phát được lúc gameplay dừng ở kết quả.
- Importer: `Tools/import_defeat.py`; metadata và hash: `Tools/AudioSources/Defeat/source.json`, `Docs/QA/audio_provenance.json`. Có guard để sinh âm tổng hợp không ghi đè Defeat đã chọn.
- Tổng runtime vẫn **15 clip**: **8 clip tổng hợp + 7 clip từ các bản thu bên ngoài/người dùng**. Đã cập nhật `Docs/AUDIO_CREDITS.md`.

## Kiểm tra

Audit PCM đạt 15/15 clip có tiếng và không có mẫu full-scale; bound mix bảo thủ **0,89449 < 0,95**. Mix offline peak khoảng **0,432**, không chạm full-scale. Repository audit đạt 6/6.

Unity đạt **62/62 kiểm tra Play Mode**, gồm Defeat phát một lần và dừng BGM, Result chặn SFX chiến đấu, quay về menu khôi phục nhạc và các kiểm tra Battle/Victory hiện có. Build Windows **Succeeded; errors=0; warnings=2**. Smoke **4/4 scene** đạt và lưu/đọc âm lượng qua process mới đạt. Hai cảnh báo build còn lại thuộc Unity Services chưa liên kết project và tham chiếu System.Windows.Forms. Audio legacy không được đóng gói; hash các file build khớp báo cáo smoke. Kết quả cuối: `unity_verification.json`, `sound_completion_2026-11-02.json`, `player_smoke.json`. Game mới: `Builds/QA/KingdomGuard.exe`.

## Nghe thử

- Nghe riêng `Assets/Resources/Audio/SFX/SFX_Defeat.wav`.
- File `Builds/QA/AudioReview.wav` mới dài **36 giây**: Victory ở khoảng **12,4–16,2s**, FIASCO ở **16,6–19,9s**. Đoạn stress đã dời sang **22–32s** để tránh chồng lên Defeat.
- Trong game: để HP lâu đài về 0, đợi ít nhất 5 giây trước khi chơi lại/menu. Kiểm tra Defeat phát một lần, nghe đủ đuôi, không còn Battle, và thanh Effects điều khiển được âm này. So sánh cảm nhận âm lượng với Victory qua một lượt thắng.

Chưa nghiệm thu bằng tai độ lớn cảm nhận và chất lượng chuyển sang màn thua. Không ghi PASS nghe từ số đo PCM.

## Tái tạo

```powershell
python Tools/import_defeat.py
python Tools/audio_audit.py
python Tools/audio_review_mix.py
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/run_sound_qa.ps1
```

Importer cần NumPy, kiểm tra hash nguồn trước khi ghi và dùng bản archive nếu Downloads không còn. Chạy game và audit PCM không cần NumPy.
