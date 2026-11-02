# Hoàn thiện Sound Kingdom Guard ngày 2 tháng 11 năm 2026

**Cập nhật sau lượt nghiệm thu dưới đây:** người dùng đã chọn Coin Splash và đưa WAV mới cho Preparation. Xem PREPARATION_UPDATE_2026-11-02.md để biết bản audio/build hiện tại và Docs/AUDIO_CREDITS.md để biết nguồn theo từng file. Nội dung dưới đây là báo cáo của lượt trước; bộ QA đã được chạy lại trong phiên thay Preparation.

Thực hiện trên nhánh van-khai sau merge 303595d. Phạm vi là Sound và bằng chứng QA liên quan; không thay đổi cân bằng, wave, Mage slow/AOE, quái hoặc nội dung map của nhóm.

## Kết quả theo thứ tự công việc

### Bước 1 Nối âm giao dịch tháp

BuildManager đã phát cue xác nhận sau khi mua, nâng cấp hoặc bán thành công. Click đầu để chọn/xem range chỉ có tiếng UI, không phát success và không trừ Gold. Khi thiếu Gold, không đổi tiền hoặc cấp tháp và chỉ phát tối đa một lỗi mỗi 300ms. Click nền/đóng menu không bị biến thành báo lỗi.

Mua/nâng cấp/bán dùng lại SFX_Tower_Place làm cue xác nhận, không thêm asset ngoài. BuildAudioFeedback được giữ làm adapter UnityEvent tùy chọn, không gắn trùng lên giao dịch đã nối trực tiếp.

Đã kiểm tra controller thật: click chọn không success; xác nhận mua đúng một success và trừ đúng giá; nâng cấp/bán có một confirmation; 100 lần thử nâng cấp khi Gold=0 chỉ phát một error và không đổi trạng thái.

### Bước 2 Âm từng loại tháp

Đã gán shotSound rõ cho chín asset cấp tháp. Archer, Cannon và Magic gọi ba cue riêng sau khi tạo đạn. Magic có clip gốc mới dài 0.24s; không còn dùng âm cung. Bộ runtime hiện có 13 clip, gồm 2 BGM và 11 SFX.

Đã gọi Tower.Shoot thật cho từng TowerDefinition và quan sát event phát ra sau khi AudioSource nhận PlayOneShot. Mỗi loại phát chính xác cue của nó một lần. Đây là kiểm tra routing/khởi tạo projectile có kiểm soát, không phải lượt chơi tự nhiên.

### Bước 3 Nhạc và mix

- Bốn voice chiến đấu tối đa; voice bận không nhận thêm PlayOneShot. Giới hạn 40ms toàn combat, 100ms mỗi clip; các cue combat không phát khi đang chuẩn bị hoặc ở kết quả.
- Warning/confirmation/result có nguồn riêng; UI có nguồn riêng. Hai nguồn này không chồng vô hạn các cue của chính chúng; cue mới thay cue cũ cùng nguồn.
- Gain shoot/hit/death được giữ thấp; toàn bộ Effects có headroom 0.65. Getter và slider vẫn phản ánh phần trăm người chơi chọn.
- Chuyển nhạc fade out 120ms, fade in 160ms bằng unscaledDeltaTime. Fade vẫn hoàn tất khi timeScale=0.
- Win/Lose dừng BGM và tiếng chiến đấu; replay/menu/preparation dừng stinger cũ. Cùng track đang chạy không bị khởi động lại bởi lời gọi state trùng.
- Đã nối UI click cho thanh tốc độ được dựng trong runtime.

Kiểm tra PCM: 13 clip có tín hiệu, không có sample full-scale clipping; hai BGM có sai khác sample đầu/cuối bằng 0. Bound cộng peak bảo thủ với volumes 100%, cue/gain tích hợp và số voice giới hạn là **0.8922915**, dưới ngưỡng 1.0. Bound này dựa trên PCM nguồn, không bao gồm gain/DSP hoặc AudioSource ngoài hệ thống và không là chứng nhận perceived loudness hay output loa.

Mix dày giả lập offline có peak **0.473538, khoảng -6.49 dBFS**, không clipping. Lịch giả lập thử 250 request theo thời gian: 143 được trộn, 107 bị bỏ theo giới hạn voice/cooldown. Đây là mô phỏng từ clip thật, không phải bản ghi output Unity.

### Bước 4 Volume và chuyển scene

SOUND chuyển xuống dưới bên phải để không che nút wave/pause. Panel không chồng lên nút điều khiển chính ở các ảnh đã kiểm tra; tay nắm slider đã được sửa để không đè nhãn. Pointer trên panel/nút SOUND được BuildManager bỏ qua; vùng panel đóng được trả lại. Không thay toàn bộ xử lý input pause/HUD của thành viên khác.

Đã kiểm tra độc lập Music/Effects, mute toàn bộ nguồn được quản lý, clamp 0–1, lưu khi đóng bảng, dùng UI khi pause, giữ volume khi replay và qua Level 2/3. Đã chạy hai process Windows riêng: process đầu lưu Music=0.23/Effects=0.41, process thứ hai đọc lại đúng rồi phục hồi cấu hình trước test. Runner có bước khôi phục dự phòng trong finally.

Ảnh Unity render 1280×720, 1600×900 và 1920×1080 đã được xem: SOUND/panel nằm trong màn hình, không che nút wave/pause, nhãn không chồng tay nắm slider. Đây là render từ game và UI thực, không phải desktop screenshot.

## Bằng chứng mới

| Kiểm tra | Kết quả | File |
|---|---|---|
| Unity Play Mode | 40 assertion đạt, failure rỗng | sound_completion_2026-11-02.json / unity_verification.json |
| Compile và Windows development build | Succeeded, 0 lỗi, 3 cảnh báo | JSON trên và Logs/SoundQaVerification.log |
| Smoke Windows | MainMenu + 3 Level đạt, mã thoát 0 | player_smoke.json |
| Mở process mới | fresh_process_volume_reload=true | player_smoke.json; Logs/AudioPrefsSave.log và AudioPrefsReopen.log |
| Audio source/provenance | 13 clip gốc, hash khớp | audio_audit.json / audio_provenance.json |
| Loop seam và bound mix | Đạt kiểm tra số liệu | audio_mix_audit.json |
| Mix nghe thử | WAV 32 giây, timeline/peak có ghi lại | Builds/QA/AudioReview.wav / audio_review_mix.json |
| Legacy audio | Không đóng gói Assets/Audio | legacyAudioIncluded=false trong JSON Unity |
| Metadata/scene | Kiểm tra riêng bằng repository_audit.py | repository_audit.json |

Các log trong Logs và build trong Builds được Git ignore. Hash exe/resources.assets/Assembly-CSharp.dll trong player_smoke.json nhận diện đúng bộ build đã kiểm tra; exe riêng lẻ không đủ để phân biệt thay đổi audio/script.

Ba cảnh báo build còn lại: Unity Services chưa link project ID; pragma deprecated trong TextMeshPro shader; System.Windows.Forms.dll không được hỗ trợ. Không sửa package hoặc cấu hình dịch vụ ngoài phạm vi Sound. Không ghi build là sạch warning.

## Phần cần nghe duyệt

Đã thử kết nối Computer Use, retry và reset; native pipe không có nên chưa kiểm tra chuột/bàn phím desktop trực tiếp. Phiên này không có bằng chứng nghe output Unity qua tai nghe/loa. Vì vậy phần sắc âm, nhịp loop cảm nhận và mức dễ chịu ở mật độ cao **chưa được ký duyệt cảm nhận**, dù số liệu và kiểm thử kỹ thuật đạt.

Mở `Builds/QA/AudioReview.wav` để nghe nhanh: 0–3s Preparation, 3.5–6.5s Battle, khoảng 7–15s các SFX riêng, 18–28s mix dày. Giữa các đoạn có khoảng lặng. Sau đó mở game, thử SOUND ở menu, pause và Win/Lose; đặt nhiều tháp để nghe mix trong trận. Chỉ ghi PASS manual sau khi nghe thật.

## Chạy lại và bàn giao

1. `python Tools/audio_audit.py` kiểm tra clip, hash, loop seam và bound.
2. `python Tools/audio_review_mix.py` tái tạo WAV nghe thử và số đo.
3. Đóng Unity cùng project rồi chạy `powershell -ExecutionPolicy Bypass -File Tools/run_sound_qa.ps1` để chạy Play Mode, build, smoke bốn scene và test process-volume.

Khi chuyển máy, chép cả thư mục Builds/QA thay vì chỉ exe. Kịch bản QA thay wave/sinh đối tượng tạm trong Play Mode, không lưu vào scene; các preference liên quan và tùy chọn Play Mode được khôi phục.

Chưa commit hoặc push thay đổi trong phiên Sound này. Báo cáo progress/GDD đã có trước phiên được giữ nguyên. Mức bàn giao: **triển khai và kiểm thử kỹ thuật Sound hoàn thành; nghe duyệt cảm nhận và input native còn chờ**.
