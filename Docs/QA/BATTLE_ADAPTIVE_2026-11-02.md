# Nhạc Battle thích ứng — 02/11/2026

Đã thay Battle tổng hợp bằng ba đoạn từ `3-01.-All-Conquering-Tide.wav` và thêm cơ chế chuyển theo lượng quái thực sự còn sống. File trong Downloads được giữ nguyên. Đây là cách dựng các đoạn từ một bản nhạc stereo hoàn chỉnh; không phải tách từng nhạc cụ thành các lớp độc lập.

## Phân tích và lựa chọn đoạn

Nguồn: stereo PCM16, 48.000 Hz, dài **267,278 giây (4:27,278)**. Phân tích dùng RMS hai kênh và spectral flux trên tín hiệu mono để so sánh mức năng lượng và biến động phổ. Các số đo không thay thế việc nghe nhạc. Metadata chi tiết, hash nguồn và đầu ra: [battle_analysis.json](battle_analysis.json).

| Mức | Đoạn trong bản gốc | RMS nguồn | Spectral flux trung bình | Loop xuất ra |
|---|---|---:|---:|---:|
| Calm — bình thường | 2:08–2:32 | 0,11687 | 0,10293 | 23,2 giây |
| Tense — căng hơn | 0:56–1:20 | 0,16968 | 0,22589 | 23,2 giây |
| Intense — cao trào | 3:20–3:52 | 0,27202 | 0,43744 | 31,2 giây |

Đoạn cao trào có RMS khoảng 2,33 lần và spectral flux khoảng 4,25 lần đoạn bình thường. Vì vậy lựa chọn này dựa trên sự khác nhau trong nội dung bản thu, thay vì chỉ tăng âm lượng hay tăng tốc phát. Nhạc giữ nguyên cao độ/tốc độ và stereo; áp dụng cùng hệ số gain cho cả ba đoạn để giữ quan hệ động lực của bản gốc.

Bản nguồn có 28 mẫu chạm ngưỡng full-scale PCM16. Các file xuất không có mẫu full-scale; giảm gain tạo khoảng trống khi trộn nhưng không tái tạo được chi tiết nếu bản nguồn đã bị cắt đỉnh. Không kết luận có méo nghe thấy chỉ từ con số này.

![Phân tích năng lượng và biến động phổ](Visual/battle-analysis.png)

## Xử lý file và chuyển nhạc

- Lưu bản gốc tại `Tools/AudioSources/Battle/All-Conquering-Tide_user.wav`, ngoài thư mục Assets.
- Resample polyphase 48 kHz → 44,1 kHz; xuất stereo PCM16. Gain chung 0,20 so với biên độ nguồn.
- Trộn chồng đuôi/đầu mỗi đoạn 800ms để tạo vòng lặp; thêm ramp 5ms ở hai biên để giảm tiếng click. Chồng 800ms khiến loop ngắn hơn đoạn cắt 0,8 giây.
- Game dùng hai AudioSource phát chồng **1 giây**, gain tuyến tính có tổng không vượt âm lượng Music của người dùng. Không cộng hai bài ở toàn bộ âm lượng.
- Nếu có yêu cầu nhạc mới trong lúc chuyển, hoàn thành lượt crossfade đang chạy rồi xử lý yêu cầu mới nhất. Result hủy ngay và dừng cả hai nguồn.
- Điểm nối được xử lý kỹ thuật nhưng chưa căn theo phách/ô nhịp hoặc xác nhận bằng tai. Có thể còn cảm giác đổi câu nhạc; cần nghe thử để chốt các mốc cắt.

## Quy tắc trong game

| Tình huống | Hành vi |
|---|---|
| Vừa vào Battle | Khởi đầu Calm, đặt lại bộ đếm giữ mức |
| 5–9 quái sống | Đề nghị Tense |
| Từ 10 quái sống | Đề nghị Intense |
| Có Boss sống | Đề nghị Intense ngay ở lần quét kế tiếp, bỏ qua thời gian giữ thông thường |
| Tense giảm xuống 4 quái | Giữ Tense; phải còn tối đa 3 mới đề nghị Calm |
| Intense giảm xuống 8–9 quái | Giữ Intense; tối đa 7 mới đề nghị hạ mức |
| Thăng mức thông thường | Điều kiện duy trì 2 giây và mức hiện tại đã giữ ít nhất 5 giây |
| Hạ mức | Điều kiện duy trì 4 giây và mức hiện tại đã giữ ít nhất 5 giây |
| Pause | Đóng băng logic giữ mức; crossfade đang chạy vẫn hoàn thành |
| Chơi 2x | Khoảng giữ vẫn tính bằng giây thực khi đang chơi |
| Hết wave / menu | Quay về Preparation |
| Win / Lose | Dừng cả hai BGM, phát cue kết quả hiện có |

Quét mỗi 250ms, chỉ đếm Enemy đang active/enabled có HP > 0; Boss được nhận qua EnemyKey. Không dùng `WaveSpawner.AliveEnemiesCount` vì giá trị đó gồm cả quái chưa spawn. Nếu tháp giết quái nhanh, nhạc có thể giữ Calm dù tổng lượng quái của wave lớn: đây là hành vi theo mật độ quái đang hiện diện.

Boss gửi yêu cầu trong tối đa khoảng 250ms khi game đang chạy; âm cao trào tăng dần trong crossfade 1 giây. Nếu một lượt crossfade khác đang chạy, thời gian nghe rõ cao trào có thể thêm tối đa khoảng 1 giây.

## Nghe thử và test thủ công

Mở `Builds/QA/BattleAdaptiveReview.wav`: **45 giây**, lần lượt Calm → Tense → Intense → Calm, ba lần chuyển chồng 1 giây. Đây là bản nghe so sánh, không phải bản ghi từ gameplay và không chứa SFX. Các khoảng chuyển bắt đầu tại giây 11, 22 và 33. Build game ở `Builds/QA/KingdomGuard.exe`.

1. Trong trận, quan sát số quái hiện diện thay vì chỉ số wave. Nghe Calm khi ít quái; giữ 5 quái đủ thời gian để nghe Tense; 10 quái để nghe Intense.
2. Nghe lúc Boss xuất hiện, lúc quái chết dần, và khi hết wave quay về Preparation. Kiểm tra có hụt tiếng, lặp khó chịu hoặc đổi câu nhạc đột ngột không.
3. Trong lúc đổi nhạc, thử Pause, Music = 0 rồi tăng lại, tốc độ 2x và Win/Lose. SFX vẫn phải theo thanh Effects riêng.
4. Nghe từng loop ít nhất ba vòng: Calm/Tense khoảng 70 giây, Intense khoảng 94 giây. Nếu nối câu chưa tự nhiên, ghi mức nhạc và thời điểm nghe thấy để chỉnh lại mốc cắt.

Chỉ có bản stereo đã trộn nên không thể bảo đảm duy trì chính xác cùng câu nhạc khi đổi mức như hệ thống có stem đồng bộ. Nghe nghiệm thu còn chờ người dùng; không đánh dấu chất lượng cảm nhận là đã đạt chỉ từ số đo PCM.

## Tái tạo

```powershell
python Tools/import_battle.py
python Tools/audio_audit.py
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/run_sound_qa.ps1
```

Importer cần NumPy, SciPy và Matplotlib theo `Tools/requirements-audio-import.txt`. Game và bộ audit PCM không cần các thư viện này. Importer dùng file Downloads nếu còn, nếu không dùng bản archive; phát hiện hash nguồn khác bản đã lưu và từ chối ghi đè âm thầm.

Nguồn và quyền được ghi đúng mức thông tin được cung cấp trong `Docs/AUDIO_CREDITS.md`; không gán nhãn CC0 cho bản nhạc người dùng cung cấp.

## Kết quả kiểm thử

Bản cuối đã đạt **62/62 kiểm tra Play Mode**, build Windows **Succeeded; errors=0; warnings=3**, **4/4 scene smoke** và **lưu/đọc âm lượng qua process mới đạt**. Hai cảnh báo API phát sinh từ code mới đã được sửa và xác nhận lại. Ba cảnh báo build còn lại: Unity Services chưa liên kết project, pragma shader TextMeshPro cũ, và tham chiếu System.Windows.Forms không được nền tảng hỗ trợ. Báo cáo JSON cuối: `unity_verification.json`, `sound_completion_2026-11-02.json`, `player_smoke.json`.

Các kiểm tra bổ sung bao gồm:

- 10 tình huống chính sách: ngưỡng tăng/hạ, vùng giữ mức, thời gian chờ, Boss và việc hủy đợt tăng khi số quái giảm.
- 12 kiểm tra tích hợp: 100 quái trong wave nhưng chỉ 1 đã spawn; bỏ qua inactive/HP bằng 0; 5 và 10 quái thật chọn đúng clip; Pause đóng băng thời gian giữ; giảm xuống 1 quái trở lại Calm; Boss thật chọn High; hai nguồn thực sự cùng phát khi crossfade; tổng gain; mute cả hai; Result hủy crossfade; Preparation khởi động lại.
- 40 kiểm tra Sound hiện có tiếp tục đạt: mua/nâng cấp/bán, SFX theo tháp, chống spam, UI, âm lượng, wave/intermission, Win/Lose/replay/menu và các scene.
- Audit PCM: **15/15 clip** có tiếng, không có mẫu full-scale; bước biên loop BGM bằng 0 sau ramp; bound mix bảo thủ **0,89449 < 0,95** ở âm lượng người dùng 100%. Bound này dùng điều kiện tổng gain hai BGM không vượt 1.
- Preview mix có SFX `AudioReview.wav`: peak mô phỏng khoảng **0,432**, không chạm full-scale. File này dùng Calm cho đoạn Battle; dùng `BattleAdaptiveReview.wav` để so sánh đủ ba mức.
- Repository audit: **6/6** kiểm tra toàn vẹn đạt. Bản gốc ngoài Assets; kiểm tra build không đóng gói audio legacy.

Chưa nghiệm thu bằng tai điểm nối câu nhạc, độ lặp và cân bằng cảm nhận giữa các mức. Không dùng kết quả tự động để điền PASS cho các mục nghe thủ công.
