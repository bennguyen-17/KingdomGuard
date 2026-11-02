# Cập nhật nhạc Preparation từ WAV người dùng

Ngày 02/11/2026. Người dùng cung cấp Genshin-Impact-Tender-Strength-_wip_.wav và xác nhận tải từ trang chính thức. Chưa có URL cụ thể, tên người làm bản phối hoặc văn bản giấy phép; hồ sơ ghi đúng thông tin được cung cấp, không gán CC0 hay nhận là sáng tác của dự án.

## File và xử lý

- Bản gốc: stereo PCM16, 44.1kHz, 83.5657 giây, không có sample chạm ngưỡng clipping.
- Bản gốc ở Downloads không đổi; lưu một bản đối chiếu trong Tools/AudioSources/Preparation/TenderStrength_user.wav.
- Bản runtime: Assets/Resources/Audio/BGM/BGM_Preparation.wav, giữ nguyên Unity metadata/GUID và stereo 44.1kHz.
- Cắt phần đuôi rất nhỏ tiếng, giữ thêm 250ms decay; độ dài runtime khoảng 79.7665 giây. Không cắt đoạn giữa bài.
- Fade đầu 20ms/cuối 400ms để làm mềm mép file; đưa peak về 0.20 để khớp mức mix của game.
- Source hash, processed hash, thao tác và script được ghi trong Tools/AudioSources/Preparation/source.json.

Track Preparation đang được dùng chung ở MainMenu, giai đoạn chuẩn bị màn và thời gian nghỉ giữa wave. Nhạc Battle và các SFX không đổi. Coin Splash tiếp tục là âm xác nhận mua/nâng cấp/bán.

## Kiểm tra

Audit PCM/provenance đạt cho cả 13 clip. Kiểm tra mép loop thực hiện theo từng channel; sai khác mẫu đầu/cuối bằng 0. Bound mix bảo thủ từ PCM là khoảng 0.894, không clipping trong mix giả lập. Các kiểm tra số liệu không thay thế việc nghe điểm lặp theo nhịp/phách.

Đã chạy lại bộ QA trên nhạc mới: 40 assertion Play Mode đạt, failure rỗng. Windows development build thành công, 0 lỗi và 2 cảnh báo ở lượt này. Player smoke và kiểm tra lưu âm lượng qua process mới được ghi trong player_smoke.json. Các con số cuối phải đối chiếu JSON của lượt chạy mới nhất.

Build mới bao gồm cả bản Coin Splash được chọn ở lượt trước. Kết quả đang cập nhật tại unity_verification.json, sound_completion_2026-11-02.json và player_smoke.json; log trong Logs.

## Nghe duyệt và chạy lại

Nghe trực tiếp bản runtime hoặc mở game từ Builds/QA/KingdomGuard.exe. Để menu/chuẩn bị chạy khoảng bốn phút để nghe ba vòng; đánh giá xem cuối bài quay về đầu có tự nhiên hay không. Fade chỉ làm mềm mép âm, không chứng minh vòng lặp đúng nhịp nhạc. Chưa có ký duyệt cảm nhận bằng tai nghe/loa trong phiên này.

python Tools/import_preparation.py tái tạo file runtime từ Downloads hoặc bản archive; cần NumPy. python Tools/audio_audit.py kiểm tra file/hash/loop/mix. Lệnh --generate-originals giữ cả nhạc người dùng và Coin Splash, chỉ sinh lại các cue tổng hợp.

Không commit hoặc push thay đổi. File gốc của người dùng được giữ nguyên.
