# Kế hoạch kiểm thử Sound và MVP

Cập nhật 02/11/2026. QA: Văn Khải. Unity 6000.6.0f1, Windows x64. Kết quả Sound mới: sound_completion_2026-11-02.json, 62 assertion Play Mode sau bổ sung Battle thích ứng; player_smoke.json ghi xác minh bốn scene và lưu âm lượng qua process mới. Xem BATTLE_ADAPTIVE_2026-11-02.md cho kết quả đầy đủ của lần cập nhật. Không suy ra PASS nghe hoặc input native từ kết quả tự động.

## Chạy lại

Đóng Editor cùng project, chạy `powershell -ExecutionPolicy Bypass -File Tools/run_sound_qa.ps1`. Runner khôi phục preference/tùy chọn Editor sau test, build Windows và chạy các process player. Log ở Logs; executable ở Builds/QA/KingdomGuard.exe.

`python Tools/audio_audit.py` kiểm tra 15 clip, hash, loop seam và bound mix. `python Tools/audio_review_mix.py` tạo Builds/QA/AudioReview.wav và số đo. `python Tools/import_battle.py` tạo ba loop và Builds/QA/BattleAdaptiveReview.wav từ bản nhạc đã lưu. Chỉ dùng `--generate-originals` khi chủ động muốn sinh lại audio.

## Ma trận kiểm thử

| ID | Phép thử | Mong đợi | Trạng thái |
|---|---|---|---|
| AU01 | MainMenu / Level 1 khởi động | Một AudioManager, đủ 15 clip | Đạt Play Mode |
| AU02 | Tạo manager thứ hai | Loại bản trùng | Đạt Play Mode |
| AU03 | Music/Effects | Điều chỉnh độc lập, đúng phần trăm | Callback đạt; chuột native chưa duyệt |
| AU04 | Volume -1 và 2 | Clamp 0–1 | Đạt Play Mode |
| AU05 | Chuyển qua ba Level | Giữ manager và volume | Đạt Play Mode và player |
| AU06 | Bắt đầu wave | Nhạc Battle | Đạt bằng WaveSpawner thật |
| AU07 | Hết wave, còn wave tiếp | Nhạc Preparation | Đạt state/track trong intermission |
| AU08 | Diệt hết wave test cuối | Win, dừng combat/BGM, một Victory | Đạt test có kiểm soát, không phải thắng tự nhiên |
| AU09 | HP=0 | Lose, dừng BGM, một Defeat | Đạt test có kiểm soát |
| AU10 | Về menu | Nhạc Menu và timeScale bình thường | Đạt Play Mode |
| AU11 | PCM và provenance | Có tín hiệu, không full-scale clipping, hash khớp | Đạt Python |
| AU12 | Executable smoke | MainMenu + ba Level có audio/UI đúng state | Đạt cả bốn scene, exit 0 |
| AU13 | Slider / TEST SOUND bằng chuột | Nghe thay đổi gain, không tác động mua tháp | Callback/mute/consume vùng pointer đạt; native và nghe chưa duyệt |
| AU14 | Bàn phím trên slider | Chọn và đổi giá trị đúng | Chưa chạy lại native; giữ navigation Unity hiện có |
| AU15 | Đóng rồi mở process mới | Đọc lại Music=0.23, Effects=0.41 | Đạt hai process Windows; phục hồi cấu hình sau test |
| AU16 | Loop nhạc ba vòng | Không click/gãy nhịp khó chịu | Seam PCM đạt; nghe cảm nhận chưa duyệt |
| AU17 | Combat dày | Không chồng vô hạn, UI/cảnh báo vẫn có nguồn | Đạt spam 5.000 request, cap 4 voice và mix giả lập; nghe chưa duyệt |
| AU18 | Chọn/xác nhận mua | Preview không success; sau giao dịch đúng một success | Đạt BuildManager thật qua script |
| AU19 | Thiếu Gold / click nền | Một error có cooldown, không mất tiền; nền không báo lỗi | Đạt rejection 100 click; không thêm cue vào click nền |
| AU20 | Archer/Cannon/Magic bắn | Mỗi tháp dùng cue riêng | Đạt Tower.Shoot thật, event sau PlayOneShot |
| AU21 | Lâu đài mất HP | Cảnh báo ngắn | Hook hiện có; priority dưới spam đạt; nghe chưa duyệt |
| AU22 | Win/Lose/replay/menu | Không có BGM/combat cũ chạy tiếp | Đạt state, voice stop, fade và replay |
| AU23 | SOUND ở 720p/900p/1080p | Không tràn, không che wave/pause | Đã xem ba ảnh Unity render; chưa desktop screenshot |
| GP01 | Mua/nâng cấp/bán | Gold và tháp đúng giao dịch | Đạt controller trong progress audit 02/11; không thay cho click native |
| GP02 | Goblin/Orc | Chỉ số/thưởng theo loại | Đã có data/prefab; test Goblin đạt; chưa chơi đủ đội hình |
| GP03 | First targeting | Ưu tiên quái gần đích | Code đã triển khai; chưa test lựa chọn nhiều mục tiêu |
| GP04 | Hai đạn cùng giết quái | Thưởng/effect chết đúng một lần | Chưa nghiệm thu edge case |
| GP05 | Chơi thật đủ wave | Có chiến thuật thắng được | Chưa nghiệm thu lượt tự nhiên |
| GP06 | Ba chiến thuật, mỗi loại ba lượt | Có phiếu Gold/HP/thời gian | Chưa chạy playtest cân bằng |

## Tiêu chuẩn nghiệm thu

Battle thích ứng bổ sung 10 kiểm tra chính sách và 12 kiểm tra tích hợp vào runner Sound: đếm sống thay vì hàng chờ, ngưỡng 5/10 và 3/7, khoảng giữ, Boss, Pause, hai nguồn crossfade, gain/mute, Result và khôi phục Preparation. Kết quả cảm nhận của ba loop vẫn cần nghe theo BATTLE_ADAPTIVE_2026-11-02.md.

Sound kỹ thuật cần import, compile, routing/giao dịch, volume, state, build và player đạt. Sound cảm nhận cần nghe thật loop/mix/cảnh báo; không ghi PASS manual từ số đo PCM. Nghiệm thu MVP và cân bằng là hạng mục riêng, không dùng test ép Win để thay thế.

Phiên nghe cần ghi ngày, người nghe, loa/tai nghe, âm lượng hệ thống, Music/Effects, tình huống, cảm nhận và thay đổi đề xuất. Mỗi bug cần bước tái hiện, expected/actual, build/commit, log/ảnh, owner và kết quả retest.
