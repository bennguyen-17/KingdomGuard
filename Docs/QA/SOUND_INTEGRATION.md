# Bàn giao âm thanh Kingdom Guard

Cập nhật ngày 02/11/2026. Runtime dùng `Assets/Resources/Audio`, gồm 4 BGM và 11 SFX: 8 clip tổng hợp, Coin Splash CC0, Victory Fanfare của Universfield, FIASCO của olenchic, Preparation và ba đoạn Battle từ WAV người dùng cung cấp. Xem [DEFEAT_UPDATE_2026-11-02.md](DEFEAT_UPDATE_2026-11-02.md), [VICTORY_UPDATE_2026-11-02.md](VICTORY_UPDATE_2026-11-02.md) và [BATTLE_ADAPTIVE_2026-11-02.md](BATTLE_ADAPTIVE_2026-11-02.md). Thư mục `Assets/Audio` giữ các bản cũ chưa rõ nguồn và không được dùng làm nguồn runtime.

## Luồng sự kiện

| Sự kiện | Điểm gọi | Âm thanh |
|---|---|---|
| Menu | MainMenuController.Start / UIManager.ShowMainMenu | Preparation |
| Bắt đầu chuẩn bị màn / nghỉ giữa wave | GameManager.BeginGameplay / WaveSpawner | Preparation |
| Bắt đầu wave / bắt đầu sớm | WaveSpawner.StartWave | Battle Calm |
| Quái thực sự còn sống tăng/giảm | BattleMusicDirector, quét mỗi 250ms | Calm / Tense / Intense; 5/10 quái để tăng mức, 3/7 để hạ mức; Boss ưu tiên Intense |
| Chọn tháp xem trước | BuildManager.Activate, click đầu | UI Click, không phát thành công |
| Mua tháp thành công | Sau tạo tháp, Setup, trừ Gold và gán node | Tower Place, một lần |
| Nâng cấp hoặc bán thành công | Sau giao dịch tương ứng | Dùng lại cue xác nhận Tower Place |
| Thiếu Gold | Khi bấm lựa chọn bị vô hiệu do thiếu tiền | Build Failed, tối đa một lần mỗi 300ms |
| Bấm nền / đóng menu / ô không đặt được | Không có giao dịch | Không phát lỗi |
| Archer / Cannon / Magic bắn | Tower.Shoot, sau Bullet.Init | Cue riêng theo TowerData.shotSound |
| Quái mất máu / chết / lọt lâu đài | Bullet / Enemy | Hit / Enemy Death / Base Damage |
| UI, kể cả speed bar tạo runtime | UIManager / MainMenuController / GameSpeedController | UI Click |
| Win / Lose | UIManager | Dừng BGM và combat; phát Victory / Defeat |

BuildManager đã nối trực tiếp AudioManager. BuildAudioFeedback vẫn là adapter tùy chọn cho UnityEvent; không gắn thêm adapter vào cùng giao dịch để tránh gọi trùng. Không phát success từ Tower.Start vì tháp xem trước/dựng sẵn không phải giao dịch.

## Loại tháp

`TowerData.shotSound`: Arrow=0, Cannon=1, Magic=2. Cả chín asset level hiện có đã gán rõ enum. Cannon vẫn nhận diện isCannon theo gameplay mới; Magic có clip mới SFX_Magic_Shoot.wav, không dùng âm cung. Thay đổi này chỉ hoàn thiện Sound, không bổ sung slow/AOE cho Mage.

## Mix và chuyển nhạc

- Tối đa bốn voice combat, chỉ dùng voice rảnh; không chồng vô hạn bằng PlayOneShot. Toàn bộ combat giới hạn 40ms/sự kiện và 100ms/clip.
- Shoot Arrow/Cannon/Magic dùng gain 0.45/0.65/0.5; Hit/Death dùng 0.35/0.5. User Effects volume có hệ số headroom 0.65 trước AudioSource.
- Warning/confirmation/result và UI có nguồn riêng, mỗi nguồn giữ một cue đang phát; combat không chiếm nguồn của cảnh báo/UI.
- Music crossfade tuyến tính 1 giây bằng hai AudioSource, dùng unscaledDeltaTime để hoạt động khi pause; tổng gain không vượt Music của người dùng. Không khởi động lại track đang phát chỉ vì cùng state được gọi lại. Logic mức Battle dừng khi pause; xem báo cáo Battle cho ngưỡng và khoảng giữ.
- Đổi scene về Menu/Preparation dừng tiếng combat và stinger cũ. Result dừng combat/BGM; cue chiến đấu đến muộn bị bỏ qua.
- Giới hạn peak tính bảo thủ cho các cue tích hợp, volumes 100%: xem audio_mix_audit.json. Đây không phải compressor/limiter cho AudioSource hoặc hiệu ứng ngoài hệ thống.

## Bảng SOUND và cấu hình

SOUND nằm dưới bên phải để không che nút wave/pause phía trên. Panel mở phía trên nút, có Music/Effects, phần trăm, TEST SOUND và CLOSE. Vùng panel/nút SOUND chặn riêng pointer xuống BuildManager; không sửa toàn bộ chính sách input HUD/pause của nhóm.

Music/Effects được lưu khi đóng panel, pause/quit ứng dụng và đổi scene. Getter phản ánh phần trăm người chơi chọn; hệ số headroom SFX là chi tiết mix riêng. Đã có phép thử mở lại process Windows thật để xác minh lưu cấu hình; runner phục hồi preference trước kiểm tra.

## Nguồn audio và công cụ

Mười một clip được tổng hợp từ công thức trong `Tools/audio_audit.py`; SFX_Tower_Place.wav dùng Coin Splash CC0 của LordTomorrow. BGM_Preparation.wav dùng bản Tender Strength WIP do người dùng đưa; người dùng xác nhận tải từ trang chính thức nhưng chưa cung cấp URL hoặc giấy phép cụ thể. Nguồn gốc/xử lý ghi riêng theo file trong Docs/AUDIO_CREDITS.md và Tools/AudioSources. `--generate-originals` giữ cả hai file ngoài và kiểm tra hash gốc/bản xử lý. Giữ `.meta`/GUID khi thay nội dung.

Chạy `python Tools/audio_audit.py`, `python Tools/audio_review_mix.py`, rồi `powershell -ExecutionPolicy Bypass -File Tools/run_sound_qa.ps1`. Đóng Unity cùng project trước khi chạy QA. BuildReport xác minh legacy audio không nằm trong player.

Bản nghe thử là `Builds/QA/AudioReview.wav`: đầu file nghe riêng cue/nhạc, từ giây 18 có mix dày giả lập. Đây là mix PCM offline, không phải bản ghi loa hoặc output Unity. Cần nghe duyệt thực tế để đánh giá cảm giác, loop, sắc âm và độ rõ của cảnh báo.
