# Hướng dẫn chạy test Kingdom Guard

Cập nhật 02/11/2026. Hướng dẫn dành cho phần Sound và các thao tác game cần dùng để kiểm tra Sound. Bạn có thể test trực tiếp bản Windows mà không cần biết code. Phần cuối hướng dẫn chạy bộ kiểm tra tự động bằng Unity.

**Nên làm theo thứ tự: mở game → test giao dịch tháp → âm từng tháp → nhạc và mix → volume/pause/chuyển màn → ghi kết quả.** Dành khoảng 15–25 phút cho các bước Sound cơ bản; việc chơi đến Win/Lose cần thêm thời gian.

## Chuẩn bị và mở game

1. Cắm tai nghe hoặc chọn loa bạn muốn dùng. Đặt âm lượng Windows ở mức vừa nghe và giữ ổn định trong phiên test.
2. Mở thư mục `Builds/QA` trong dự án, chạy `KingdomGuard.exe`.
3. Khi chép game sang máy khác, chép **cả thư mục `Builds/QA`**, gồm exe, thư mục dữ liệu và các DLL. Không chỉ chép riêng exe.
4. Ở MainMenu, chọn **CHƠI NGAY / PLAY** để vào Level 1. Tháp Cung/Pháo/Phép tương ứng Archer/Cannon/Magic; quái được hiển thị bằng tên các môn học.
5. Chọn **SOUND ở góc dưới bên phải**. Đặt Music khoảng 70%, Effects khoảng 80% để có mốc so sánh. Chọn CLOSE.
6. Mở [phiếu test](PHIEU_TEST_SOUND.md), ghi ngày, bản game/commit, thiết bị nghe và volume. Các tỷ lệ 70%/80% là mức bắt đầu thử, không bắt buộc là mức bạn phải thích.

Nếu chưa có `Builds/QA/KingdomGuard.exe`, dùng cách chạy trong Unity hoặc chạy bộ tự động bên dưới để tạo build.

### Cách chạy bằng Unity

1. Mở project bằng Unity **6000.6.0f1**.
2. Trong Project, mở `Assets/Scenes/MainMenu.unity` rồi nhấn Play ở thanh công cụ Unity.
3. Test như bản Windows. Có thể mở Window → General → Console để xem lỗi đỏ.
4. Kết thúc bằng cách tắt Play. Không lưu scene/prefab nếu chỉ kiểm thử.

Ưu tiên bản Windows cho test thoát rồi mở lại ứng dụng. Lưu âm lượng trong Editor và trong player Windows có thể nằm ở các vùng preference khác nhau, không dùng việc chuyển qua lại Editor/player làm phép thử lưu volume.

## Thao tác cần nhớ

| Thao tác | Cách thực hiện |
|---|---|
| Mở menu xây tháp | Click một ô đất đặt tháp cạnh đường đi |
| Chọn loại tháp | Click icon một lần để xem giá/tầm bắn |
| Xác nhận mua | Click lại đúng icon lần thứ hai |
| Nâng cấp/bán | Click tháp đã đặt, chọn Upgrade/Sell rồi click lại để xác nhận |
| Đóng menu tháp | Click ra ngoài; thao tác này không phải lỗi đặt tháp |
| Bắt đầu wave 1 | Đặt ít nhất một tháp, bấm nút bắt đầu hoặc Space |
| Bắt đầu wave tiếp sớm | Trong thời gian nghỉ, bấm nút bắt đầu sớm hoặc Space |
| Pause/resume | Esc hoặc P; có thể dùng nút pause và TIẾP TỤC |
| Tốc độ | 1 / 2 / 3 hoặc nút 1x / 2x / 3x |
| Chỉnh âm | SOUND → Music/Effects → CLOSE |
| Chơi lại từ đầu lượt | Pause → CHƠI LẠI; kiểm tra Gold trở lại 200 |

**Space dùng để bắt đầu/bỏ qua thời gian nghỉ wave, không phải pause.** Khi nghe so sánh các âm, giữ game ở 1x trước, sau đó mới thử tốc độ cao.

Giá hiện tại để test: Archer 50 Gold; mỗi lần nâng cấp Archer 30; Cannon 100, nâng cấp Cannon 60; Magic 150. Hãy đối chiếu giá trong menu giao dịch. Nếu trang Guide trong game ghi khác giá, ghi nhận lỗi nội dung hướng dẫn riêng; không dùng giá cũ đó để tính kết quả giao dịch.

## Phần 1 Âm mua nâng cấp bán và thiếu Gold

Bắt đầu một lượt Level 1 mới, chưa chạy wave. Có 200 Gold. Dùng đúng chuỗi dưới đây để dễ đối chiếu:

| ID | Các bước | Kết quả cần đạt |
|---|---|---|
| S01 | Click ô đất → click icon Archer **một lần** | Chỉ nghe tiếng chọn UI; chưa có âm xác nhận mua, chưa tạo tháp, Gold vẫn 200 |
| S02 | Click lại icon Archer | Tạo đúng một tháp; Gold còn 150; một âm xác nhận |
| S03 | Click tháp → chọn Upgrade một lần → xác nhận lần hai | Lần đầu chỉ chọn; lần xác nhận lên cấp 2, Gold còn 120 và một âm xác nhận |
| S04 | Nâng cấp thêm một lần rồi bán tháp | Lên cấp 3: Gold còn 90. Bán hoàn 55: Gold thành 145. Mỗi giao dịch thành công có một âm xác nhận |
| S05 | Dùng 145 Gold để mua Cannon | Gold còn 45; một âm xác nhận mua |
| S06 | Click Cannon, thử nâng cấp giá 60 khi chỉ có 45 Gold; click nhanh vài lần | Không lên cấp, không mất Gold. Có âm lỗi ngắn, không dồn thành tiếng kéo dài. Sau khi chờ hơn 0.3s, thử lại có thể phát một âm lỗi mới |
| S07 | Click ra ngoài để đóng menu; click nền không phải ô xây tháp | Không phát liên tục âm thiếu tiền/đặt thất bại |

Lưu ý: mua, nâng cấp và bán đang dùng chung cue xác nhận. Đây là hành vi được chọn cho Sound MVP. Đừng đánh FAIL chỉ vì ba thao tác dùng cùng một âm.

## Phần 2 Âm từng loại tháp

Chơi lại trước mỗi phép thử nếu cần tiền. Chọn ô đặt tháp gần đường để quái thực sự vào tầm; không đánh FAIL âm bắn nếu tháp chưa có mục tiêu.

| ID | Các bước | Kết quả cần đạt |
|---|---|---|
| S08 | Lượt mới: mua Archer 50 → bắt đầu wave → chờ tháp bắn | Nghe âm cung ngắn khi có phát bắn |
| S09 | Lượt mới: chỉ mua Cannon 100 → bắt đầu wave → chờ tháp bắn | Nghe âm pháo khác âm cung |
| S10 | Lượt mới: chỉ mua Magic 150 → bắt đầu wave → chờ tháp bắn | Nghe cue phép riêng, khác cung/pháo |

Hệ thống giới hạn mật độ SFX, nên ở lúc nhiều sự kiện xảy ra bạn không nhất thiết nghe từng viên đạn riêng lẻ. Để đánh giá đúng cue, test từng loại riêng trước. Nếu không nghe gì, kiểm tra Effects chưa về 0 và đã có đạn được tạo, rồi mới ghi lỗi.

## Phần 3 Nhạc loop cảnh báo và mix

### S11 Nhạc theo trạng thái

1. Ở menu: nghe track chuẩn bị.
2. Vào Level 1, chưa bắt đầu wave: vẫn là track chuẩn bị.
3. Đặt tháp rồi bắt đầu wave: chuyển sang track chiến đấu; không có hai bài chồng nhau.
4. Hạ hết wave, chờ nghỉ: trở về track chuẩn bị.
5. Bấm bắt đầu sớm: trở lại track chiến đấu.

Chuyển nhạc có crossfade 1 giây; nếu đang chuyển thì hoàn thành lượt hiện tại rồi nhận yêu cầu mới nhất. Không yêu cầu đổi bài ngay trong cùng một frame. Battle có ba mức theo quái sống và Boss; xem [hướng dẫn Battle](BATTLE_ADAPTIVE_2026-11-02.md) để test ngưỡng và nghe file so sánh.

### S12 Loop và mật độ cao

1. Nhạc Preparation hiện dài khoảng 79,8 giây. Để chạy ít nhất khoảng bốn phút để nghe ba vòng. Ghi lại nếu có tiếng click/pop, khoảng lặng hoặc hụt nhịp khó chịu ở điểm nối; fade đầu/cuối không tự bảo đảm vòng lặp đúng nhịp nhạc.
2. Khi chiến đấu, nghe track Battle đủ lâu để qua điểm loop. Giữ 1x khi đánh giá nhạc; sau đó thử 2x/3x để kiểm tra mật độ sự kiện.
3. Khi có đủ Gold, đặt nhiều tháp. Quan sát số tháp thực tế và ghi lại; nếu chưa đạt 10 tháp thì ghi đúng số đã test.
4. Trong lúc tháp bắn, mở SOUND và chọn TEST SOUND. Tiếng UI/cảnh báo cần phân biệt được, tổng âm không rè hoặc gây mệt tai.
5. Nếu muốn so sánh mix trước khi chơi lâu, mở `Builds/QA/AudioReview.wav` theo timeline bên dưới.

### S13 Âm lâu đài mất máu

Để một số quái lọt tới cuối đường, nhìn HUD HP giảm và nghe cue cảnh báo ngắn. Nhiều quái lọt sát nhau có thể được gộp bởi cooldown; không yêu cầu mỗi con tạo một lớp âm chồng riêng. Âm cảnh báo không nên bị tiếng bắn che hoàn toàn.

### Timeline bản nghe thử

| Thời điểm | Nội dung |
|---|---|
| 0–3s | Nhạc Preparation |
| 3.5–6.5s | Nhạc Battle |
| Khoảng 7–19.9s | Lần lượt cung, pháo, phép, hit, chết, xác nhận, lỗi, mất HP, click, thắng, thua; Victory ở khoảng 12.4–16.2s, Defeat FIASCO ở khoảng 16.6–19.9s |
| 22–32s | Mix dày giả lập với nhạc/cảnh báo/UI |

File dài 36 giây, có khoảng lặng giữa các đoạn và ở cuối. Timeline mix được tự dời sau phần nghe SFX để Defeat mới không chồng lên đoạn stress. Đây là mix PCM offline, không phải bản ghi âm game/loa; vẫn phải test trong game.

## Phần 4 Volume pause kết quả và mở lại game

| ID | Các bước | Kết quả cần đạt |
|---|---|---|
| S14 | SOUND → Music=0, Effects để lớn hơn 0 | Nhạc im; click TEST SOUND và các hiệu ứng vẫn nghe được |
| S15 | Music lớn hơn 0, Effects=0 | Nhạc còn; click TEST SOUND và hiệu ứng im |
| S16 | Cả hai về 0 | Toàn bộ âm do hệ thống Sound quản lý im |
| S17 | Bật lại âm → Esc/P → mở SOUND, chỉnh hai thanh và TEST SOUND | Trận đứng yên nhưng UI Sound vẫn dùng được. SOUND không che nút wave/pause phía trên |
| S18 | Khi bảng SOUND mở, click và kéo slider; nhìn Gold/tháp phía sau | Chỉ thay âm lượng; không mua/nâng cấp/bán tháp ngoài ý muốn. Đóng panel thì có thể thao tác map bình thường |
| S19 | Đặt Music/Effects ở hai giá trị dễ nhớ, chẳng hạn 30%/60% → CLOSE → thoát bằng menu hoặc Alt+F4 → mở lại **cùng bản exe** | Đọc lại đúng giá trị đã đặt. Không chỉ chuyển scene hoặc dừng/bật Play để kết luận test process mới đạt |
| S20 | Chơi tới Lose, nghe kết quả rồi CHƠI LẠI hoặc VỀ MENU | Một cue thua; BGM chiến đấu dừng. Chơi lại/menu khởi động track phù hợp; không còn âm trận cũ kéo dài |
| S21 | Chơi tới Win → thử Next Level, Replay và Menu trong các lượt phù hợp | Một cue thắng, BGM chiến đấu dừng; màn tiếp/Replay về nhạc chuẩn bị, menu về nhạc menu; volume giữ nguyên |

**Cách tạo tình huống Lose dễ hơn:** chơi lại, đặt một Archer để mở khóa Start Wave, bắt đầu wave rồi bán tháp. Không xây thêm, chờ quái lọt vào lâu đài; có thể dùng 3x để giảm thời gian chờ. Nghe cảnh báo và kết quả ở 1x nếu có thể.

**Nếu chưa chơi thắng được:** ghi S21 là BLOCKED/CHƯA TEST, kèm wave bạn dừng. Không đánh PASS Sound Win dựa trên việc bộ tự động đã ép tình huống thắng. Không cần sửa Gold, HP hoặc scene để hoàn thành lượt manual này.

## Ghi PASS FAIL và báo lỗi

- **PASS:** chính bạn đã thực hiện bước và thấy/nghe đúng kết quả mong đợi.
- **FAIL:** đã thực hiện nhưng thấy/nghe sai. Ghi thao tác, giá trị Gold/volume, scene/wave và kết quả thực tế.
- **BLOCKED:** chưa tạo được điều kiện để thử, ví dụ chưa tới Win. Không tính là PASS.
- **CHƯA CHẠY:** chưa thực hiện; giữ nguyên cho tới khi có bằng chứng.

Dùng [PHIEU_TEST_SOUND.md](PHIEU_TEST_SOUND.md). Khi gặp lỗi âm thanh, ưu tiên ghi màn hình **có tiếng hệ thống/game**, hoặc ghi rõ mốc thời gian trong video. Ảnh chụp đơn lẻ chỉ chứng minh UI, không chứng minh âm thanh.

Mẫu mô tả bug:

```text
ID test:
Bản game hoặc commit:
Scene / wave:
Music / Effects / âm lượng Windows:
Loa hoặc tai nghe:
Các bước tái hiện:
Mong đợi:
Thực tế:
Tần suất, ví dụ 3/3 lần:
Log / ảnh / video và mốc thời gian:
```

## Chạy bộ kiểm tra tự động

Phần này dành cho máy đã cài Unity 6000.6.0f1 với Windows Build Support. **Đóng game và Unity Editor đang mở cùng project** trước khi chạy.

Mở PowerShell tại thư mục dự án, chạy:

```powershell
Set-Location -LiteralPath 'C:\FA26 KY5\FGU301_TriPT9\KingdomGuard'
powershell -ExecutionPolicy Bypass -File .\Tools\run_sound_qa.ps1
```

Nếu Unity ở thư mục khác, truyền đúng đường dẫn tới `Unity.exe`:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\run_sound_qa.ps1 -Unity 'D:\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe'
```

Script sẽ tự:

1. Vào Play Mode và kiểm tra giao dịch/cue từng tháp, giới hạn SFX, state nhạc, volume, replay và scene.
2. Build Windows mới vào `Builds/QA`; build này thay thế bản QA trước đó.
3. Mở player kiểm tra MainMenu và ba Level.
4. Chạy các process riêng để lưu và đọc lại volume, sau đó phục hồi preference trước test.
5. Ghi kết quả máy và kết thúc.

Thường mất vài phút; script đặt giới hạn 15 phút cho Unity và 2 phút mỗi process player. Trong lúc chạy, không mở thêm Editor cùng project hoặc chỉnh code/asset. Nếu bị ngắt giữa chừng, đọc log và chạy lại; script có bước khôi phục preference dự phòng.

Khi đạt, cuối PowerShell sẽ in:

```text
PASS: Unity Play Mode, Windows build, all scene smoke checks and settings reload across player processes.
```

Kết quả mới nhất sau cập nhật Defeat FIASCO: **62 assertion Play Mode đạt, build 0 lỗi/2 cảnh báo, bốn scene smoke đạt và fresh_process_volume_reload=true**. Sau khi sửa code, dùng kết quả lần chạy mới, không mặc định vẫn giữ các con số này.

### Đọc kết quả và tìm lỗi

| File | Cần xem |
|---|---|
| `Docs/QA/unity_verification.json` | `failure` rỗng; danh sách `passed`; `build` có Succeeded và errors=0 |
| `Docs/QA/sound_completion_2026-11-02.json` | Bản kết quả cùng bộ runner hiện tại; file có thể được ghi lại khi chạy lại |
| `Docs/QA/player_smoke.json` | `exit_code=0`, `scene_passes=4`, `complete=true`, `fresh_process_volume_reload=true` |
| `Logs/SoundQaVerification.log` | Chi tiết Play Mode, compiler và build; tìm assertion thất bại hoặc exception đầu tiên |
| `Logs/AudioPlayerSmoke.log` | Có PASS scene=0,1,2,3 và COMPLETE |
| `Logs/AudioPrefsSave.log`, `AudioPrefsReopen.log` | Marker SAVED và REOPEN_PASS |

`legacyAudioIncluded` cần là false. Script không kết luận game cân bằng hoặc người chơi thắng được; các wave/Win trong suite được điều khiển để test Sound.

### Kiểm tra file audio riêng

Nếu máy có Python, chạy tại thư mục dự án:

```powershell
python .\Tools\audio_audit.py
python .\Tools\audio_review_mix.py
python .\Tools\repository_audit.py
```

Lệnh đầu kiểm tra 15 clip, hash nguồn, loop seam và bound mix. Lệnh thứ hai tạo lại WAV nghe thử SFX/nhạc tổng quát; file so sánh ba mức Battle là Builds/QA/BattleAdaptiveReview.wav. Lệnh cuối kiểm tra scene/GUID/metadata. Không cần chạy các lệnh Python để chơi và nghe test thủ công.

## Xử lý các tình huống thường gặp

| Hiện tượng | Cách kiểm tra |
|---|---|
| Không nghe toàn bộ game | Kiểm tra thiết bị phát Windows, Volume Mixer của game và hai slider trong SOUND |
| Không nghe tiếng bắn | Chọn 1x, để Effects lớn hơn 0, chờ quái vào tầm và xác nhận có đạn |
| Không bắt đầu wave được | Đặt ít nhất một tháp trước, sau đó dùng nút wave hoặc Space |
| Không đủ Gold để test tháp khác | Pause → CHƠI LẠI để lấy lại 200 Gold; không cần sửa dữ liệu |
| Chạy exe báo thiếu DLL/dữ liệu | Chép lại toàn bộ thư mục Builds/QA |
| Script báo project đang được sử dụng | Đóng Editor cùng project rồi chạy lại |
| Script báo Unity not found | Truyền tham số -Unity với đường dẫn thật |
| Build có cảnh báo | Đọc nội dung; baseline còn cảnh báo Services/TMP shader/Windows.Forms. Không coi mọi cảnh báo mới đều đã được chấp nhận |
| Thay code nhưng exe chưa đổi hành vi | Chạy lại bộ QA để build bản mới; sửa code không tự cập nhật exe đã có |

Cuối phiên, lưu phiếu test và các lỗi còn mở. Phần nghe Sound chỉ hoàn tất nghiệm thu khi đã nghe thật và ghi kết quả; kết quả tự động và số đo PCM là bằng chứng bổ sung.
