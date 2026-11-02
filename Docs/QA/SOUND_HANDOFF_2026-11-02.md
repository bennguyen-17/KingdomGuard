# Bàn giao phần âm thanh — 02/11/2026

Phạm vi commit: code Sound, tích hợp sự kiện âm thanh, cấu hình shotSound cho 9 cấp tháp, asset runtime/metadata, bản nguồn và công cụ import, tài liệu nguồn/QA và báo cáo tiến độ đã thực hiện trong phiên. Nhánh bàn giao: `van-khai`.

## Đã hoàn thiện kỹ thuật

- Preparation dùng bản WAV người dùng chọn; Battle có Calm/Tense/Intense theo số Enemy sống và Boss, crossfade hai nguồn, khoảng giữ và hysteresis.
- Giao dịch mua/nâng cấp/bán dùng Coin Splash; Victory dùng Victory Fanfare — Universfield; Defeat dùng FIASCO — olenchic.
- Archer/Cannon/Magic có cue riêng; âm cảnh báo/UI dùng nguồn riêng; SFX chiến đấu giới hạn bốn voice và chống spam.
- Music/Effects độc lập, lưu âm lượng, hoạt động khi pause; bảng Sound không truyền click xuống đặt tháp. Win/Lose dừng BGM/combat; replay/menu khôi phục nhạc đúng state.
- 15 clip runtime, metadata Unity và hash nguồn/đầu ra đầy đủ. 8 clip tổng hợp và 7 clip dẫn xuất từ bản thu bên ngoài/người dùng; nguồn/giấy phép ghi riêng tại `Docs/AUDIO_CREDITS.md`, không gán CC0 cho toàn bộ asset.
- Các importer giữ bản gốc ngoài Assets. Đã sửa importer Preparation để số lượng nguồn trong provenance vẫn đúng khi nhập lại sau các bản thu mới.

## Xác nhận trước khi push

- Chạy lại `Tools/import_preparation.py`: đầu ra vẫn khớp hash đã được QA, bảo toàn các clip khác; bảng provenance tính đúng 8/7.
- Chạy `Tools/audio_audit.py`: 15/15 clip có tiếng, không có mẫu full-scale, kiểm tra hash/loop seam đạt; bound mix bảo thủ 0,89449.
- Chạy `Tools/repository_audit.py`: 6/6 kiểm tra scene/GUID/metadata đạt.
- Xác minh kết quả Unity của bản Defeat cuối: **62/62 Play Mode**, **Windows build Succeeded; errors=0; warnings=2**, **4/4 scene smoke**, **fresh_process_volume_reload=true**. Hash các file build thực tế khớp `player_smoke.json`; không đóng gói audio legacy. Không sửa code C# hoặc nội dung PCM kể từ lượt Unity đó.
- Hai cảnh báo build còn lại: Unity Services chưa liên kết project và tham chiếu System.Windows.Forms. Xem log build khi xử lý hạ tầng Unity riêng.
- Các file WAV/OGG và ảnh sử dụng Git LFS theo `.gitattributes`. Build, Logs và Library không nằm trong commit. Thành viên cần Git LFS để nhận đầy đủ asset.

## Phần còn cần nghiệm thu

Kỹ thuật và tự động đã đạt; nghe nghiệm thu bằng loa/tai nghe vẫn chưa được đánh dấu PASS. Cần nghe điểm nối loop/crossfade, mức lớn cảm nhận của Victory/Defeat, và SFX trong trận đông quái. Hướng dẫn: `HUONG_DAN_CHAY_TEST_GAME.md`, phiếu trống `PHIEU_TEST_SOUND.md`, báo cáo từng asset trong cùng thư mục.

Build kiểm thử local: `Builds/QA/KingdomGuard.exe` (phân phối cả thư mục, không chỉ file exe). File nghe thử tổng hợp `Builds/QA/AudioReview.wav` dài 36 giây; so sánh Battle tại `Builds/QA/BattleAdaptiveReview.wav`.

Không đưa các thay đổi local ngoài phạm vi Sound vào commit: cấu hình render pipeline, UnityConnectSettings, cấu hình Learn, file solution và tài liệu GDD người dùng cung cấp. Giữ nguyên chúng trong workspace.
