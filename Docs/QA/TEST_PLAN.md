# Kế hoạch kiểm thử Sound và MVP

Ngày 26/10/2026. QA phụ trách: Văn Khải. Môi trường mục tiêu: Unity 6000.6.0f1, Windows x64. Kết quả chạy máy nằm trong `unity_verification.json` và `audio_audit.json`; không suy ra PASS manual từ việc có test case.

## Cách chạy lại

Đóng Unity đang mở cùng project rồi chạy `powershell -ExecutionPolicy Bypass -File Tools/run_sound_qa.ps1`. Runner vào Play Mode, kiểm tra runtime, khôi phục các PlayerPrefs mà phép thử thay đổi, build Windows development và chạy smoke trên executable. Quá trình build mất vài phút. Log chi tiết ở `Logs/SoundQaVerification.log` và `Logs/AudioPlayerSmoke.log`; executable ở `Builds/QA/KingdomGuard.exe`.

Chạy `python Tools/audio_audit.py` để kiểm tra PCM. Không truyền `--generate-originals` khi chỉ muốn kiểm tra.

## Ma trận kiểm thử

| ID | Phép thử và bước chính | Kết quả mong đợi | Cách xác minh / trạng thái |
|---|---|---|---|
| AU01 | Mở Level_1 từ trạng thái sạch | Một AudioManager, tải 12 clip | Play Mode tự động, xem JSON |
| AU02 | Tạo manager thứ hai, chờ một frame | Manager trùng bị xóa | Play Mode tự động |
| AU03 | Mở SOUND, kéo Music/Effects | Thay đổi riêng, đúng phần trăm | Callback tự động; thao tác chuột chưa nghiệm thu |
| AU04 | Truyền volume -1 và 2 | Clamp về 0 và 1, lưu PlayerPrefs | Play Mode tự động |
| AU05 | Chuyển Level_1 → 2 → 3 | Một manager, giữ volume | Play Mode và player smoke |
| AU06 | Bắt đầu wave | State Battle | Play Mode tự động |
| AU07 | Chuyển state Preparation | State đúng và dùng track yên hơn | Tự động kiểm tra state; nghe chưa nghiệm thu |
| AU08 | Diệt quái cuối bằng test harness | Win, timeScale=0, state Result | Play Mode tự động, không phải lượt thắng tự nhiên |
| AU09 | Đặt Lives=0 trong test harness | Lose và state Result | Play Mode tự động |
| AU10 | Từ kết quả về menu | timeScale=1, Menu | Play Mode tự động |
| AU11 | Đọc tất cả WAV bằng PCM parser | Không im lặng toàn clip, không clipping full-scale | Python tự động |
| AU12 | Mở bản Windows qua flag smoke | Ba scene có manager, UI và đủ clip | Player smoke, log PASS từng scene |
| AU13 | Chuột kéo thanh volume, click TEST SOUND | Nghe rõ thay đổi, Music không đổi gain SFX | Có test PointerEventData mapping; chuột native và nghe chưa nghiệm thu |
| AU14 | Điều hướng slider bằng bàn phím | Chọn và đổi giá trị đúng; focus thấy rõ | Có test AxisEventData; bàn phím native chưa nghiệm thu |
| AU15 | Thoát mở lại ứng dụng | Volume giữ đúng sau process mới | Manual chưa chạy; automation chỉ kiểm tra lưu và giữ qua scene |
| AU16 | Nghe Preparation/Battle loop 3 vòng | Không click, không hụt nhịp khó chịu | Manual chưa chạy |
| AU17 | 10+ tháp bắn cùng lúc | Cảnh báo/UI nghe được, không quá ồn | Bị chặn bởi placement/prefab; limiter cần nghe duyệt |
| AU18 | Đặt tháp hợp lệ | Thành công đúng một SFX sau giao dịch | Bị chặn bởi placement |
| AU19 | Đặt ở ô sai/thiếu Gold | Âm lỗi; không tạo tháp, không trừ Gold | Bị chặn bởi placement |
| AU20 | Archer và Cannon bắn xen kẽ | Đúng âm từng loại | Routing đã có; bị chặn prefab Cannon trong gameplay |
| AU21 | Quái lọt vào lâu đài | Giảm HP đúng; nghe cảnh báo ngắn | Hook đã có; nghe manual chưa chạy |
| AU22 | Thua/thắng rồi retry nhiều lần | Không nhạc chồng, không SFX trận cũ | State tự động một chu kỳ; manual nhiều chu kỳ chưa chạy |
| AU23 | Mở SOUND ở 1280×720, 1600×900, 1920×1080 | Không tràn, không che nút quan trọng khi đóng | Đã xem 3 ảnh render Unity: panel không tràn, nút PLAY không bị che; chưa native desktop |
| GP01 | Mua hai loại tháp bằng Gold | Đủ tiền mới mua, trừ chính xác một lần | Bị chặn bởi placement |
| GP02 | Goblin/Orc cùng wave | HP/tốc độ/thưởng khác nhau đúng dữ liệu | Bị chặn bởi EnemyData và wave config |
| GP03 | Chọn mục tiêu First | Ưu tiên quái tiến xa nhất | Chưa đạt khi kiểm tra code: đang nearest |
| GP04 | Hai đạn trúng một quái cùng frame | Thưởng và âm chết đúng một lần | Chưa chạy; cần guard health/death nếu tái hiện |
| GP05 | Chơi thật 5–6 wave | Có chiến thuật thắng được, HUD đúng | Bị chặn MVP |
| GP06 | Kiểm tra 3 chiến thuật × 3 lượt | Có phiếu Gold/HP/thời gian thực | Bị chặn MVP |

## Tiêu chuẩn nghiệm thu

Sound code được chấp nhận kỹ thuật khi import, compile, Play Mode và player smoke đạt. Sound cảm nhận được chấp nhận khi AU13–AU23 đạt hoặc có ngoại lệ được nhóm ghi rõ. MVP chỉ chấp nhận khi GP01–GP06 đạt, không còn blocker/high, và một người khác chạy bản Windows thành công. Không dùng test ép Win làm bằng chứng cân bằng.

## Mẫu bug và phiên kiểm thử

Bug cần: ID, severity, build/commit, scene, điều kiện, bước tái hiện, expected, actual, log/ảnh, owner, trạng thái và kết quả retest. Phiên kiểm thử cần: ngày, người kiểm tra, thiết bị, độ phân giải, âm lượng hệ thống, kết quả từng ID và các bug mới. Lưu phiên mới thay vì ghi đè kết quả lượt cũ khi cần so sánh.
