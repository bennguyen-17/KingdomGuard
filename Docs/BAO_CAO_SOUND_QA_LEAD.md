# Báo cáo thực hiện Sound QA Lead Kingdom Guard

Ngày 26/10/2026. Phụ trách theo vai trò và nhánh hiện tại: Văn Khải. Kế hoạch đối chiếu: Kingdom_Guard_GDD.docx, đặc biệt các mục 3.3, 8, 9, 10, 11 và 12.

Phần triển khai trong phạm vi cá nhân đã được bổ sung bằng code âm thanh, công cụ kiểm tra và hồ sơ bàn giao. Game vẫn chưa đạt MVP; các việc phụ thuộc placement, prefab Cannon, dữ liệu quái và HUD được giữ trạng thái mở. Không dùng kết quả ép thắng trong test làm bằng chứng đã hoàn thành lượt chơi hoặc cân bằng.

## Sound đã thực hiện

| Hạng mục | Trước | Sau |
|---|---|---|
| Nhạc | Chỉ Battle, phát xuyên trạng thái | Menu/Preparation dùng track mới; wave dùng Battle; kết quả dừng BGM |
| Khởi tạo | Phụ thuộc GameManager tạo khi cần | Bootstrap trước scene; singleton tồn tại qua scene và loại manager trùng |
| Phân luồng SFX | Một AudioSource chung | Nguồn riêng cho cảnh báo/UI/kết quả, tách âm chiến đấu |
| Mật độ tiếng | Không giới hạn | Âm chiến đấu tối đa một sự kiện mỗi 40ms và mỗi clip 100ms; gain shoot/hit/death giảm |
| Chỉnh âm | Chỉ có setter | Nút SOUND, hai slider Music/Effects, phần trăm, TEST SOUND và CLOSE |
| Lưu cấu hình | PlayerPrefs chưa có điểm flush rõ | Lưu khi đóng bảng, app pause/quit và đổi scene |
| Âm pháo | Tower luôn gọi âm cung | TowerData.shotSound cho Arrow/Cannon/Magic; Cannon có route riêng |
| Lâu đài mất máu | Chưa có âm riêng | Enemy đến đích gọi cảnh báo ngắn |
| Đặt tháp | Có file nhưng chưa có hợp đồng sự kiện | BuildAudioFeedback công khai success/rejected, quy định điểm gọi sau giao dịch |
| Asset và nguồn gốc | Thiếu nhạc chuẩn bị/hai cảnh báo; các clip cũ không rõ nguồn | Tạo mới toàn bộ 12 WAV runtime bằng script tổng hợp, có thể tái tạo; giữ riêng bản cũ |

File trọng tâm: `Assets/_Scripts/Audio/AudioManager.cs`, `AudioSettingsUI.cs`, `BuildAudioFeedback.cs`. Các điểm nối nhỏ nằm ở GameManager, WaveSpawner, Enemy, Tower, TowerData và UIManager.

Runtime tải từ `Assets/Resources/Audio`; toàn bộ clip tại đây được tạo lại bằng công thức gốc, giữ nguyên GUID. Bản cũ trong Assets/Audio được giữ nguyên để không xóa dữ liệu người dùng. BuildReport được kiểm tra để không đóng gói thư mục cũ.

## Giới hạn tích hợp Sound

- Placement controller chưa có: không thể gọi success/rejected từ thao tác mua tháp thật. API và hướng dẫn đã bàn giao; không giả phát success khi tháp vừa Start.
- Chưa có Cannon prefab hoạt động: routing code đã sẵn sàng, cần gán shotSound=Cannon khi programmer dựng prefab.
- Magic chưa có SFX riêng và không thuộc MVP: route Magic không mượn âm cung.
- Kiểm tra PCM không thay thế nghe nhạc, duyệt loop và mix. Không có kết luận về độ hay của nhạc hoặc mức ồn cảm nhận từ phép đo.
- Người dùng xác nhận AI lấy các clip cũ trên mạng và không biết nguồn. Bộ runtime đã thay toàn bộ bằng tổng hợp gốc; provenance và SHA256 có trong repo. Nguồn cũ trong Assets/Audio chưa xác minh, không đưa vào build.

Hướng dẫn gọi hàm, trạng thái và bàn giao chi tiết: [SOUND_INTEGRATION.md](QA/SOUND_INTEGRATION.md).

## QA đã thực hiện

Đã thêm runner Unity Play Mode thực, kiểm tra import, singleton, slider, PlayerPrefs, chuyển state, spawn quái, kết quả trận và chuyển scene. Test tạm giảm wave xuống một quái và gây damage để xác minh Win; test Lose đặt Lives=0. Dữ liệu test không được lưu vào scene và các PlayerPrefs bị thay đổi được khôi phục sau test.

Đã thêm kiểm tra âm thanh PCM với số đo duration, sample rate, channel, peak, RMS, clipping và SHA256. Cả 12 clip runtime có tín hiệu, không có sample ở ngưỡng clipping full-scale. Chưa dùng LUFS hay perceptual loudness, không kết luận master mix không thể clip khi nhiều nguồn cộng lại.

Đã kiểm tra Build Settings trỏ tới ba scene tồn tại và đúng GUID; toàn bộ metadata không trùng GUID; script/audio runtime đều có `.meta`, số lượng chính xác trong repository_audit.json. Sửa Build Settings Level_1–3 đã tồn tại trong working tree trước phiên này và được giữ lại, không nhận là code mới do phiên này tạo.

Đã tạo test plan với 29 trường hợp gồm 23 audio/UI và 6 gameplay, phân biệt tự động, manual chưa chạy và blocked. Bug log giữ nguyên các blocker của nhóm, bổ sung vấn đề license và hiện tượng Editor hot reload mất state. Lượt runner cuối vượt qua 22 assertion Play Mode, bao gồm sự kiện con trỏ và điều hướng slider; kết quả build/player/kiểm tra UI được ghi trong [EXECUTION_RESULTS.md](QA/EXECUTION_RESULTS.md).

Các test case và tiêu chuẩn nghiệm thu: [TEST_PLAN.md](QA/TEST_PLAN.md). Các lỗi và owner đề xuất: [BUG_LOG.md](QA/BUG_LOG.md).

## Cân bằng đã thực hiện

Đã ghi công thức DPS, số phát để giết và thời gian từ phát đầu tới phát kết liễu; tách công thức liên tục HP/DPS khỏi thời điểm damage rời rạc. Đã tính hệ số crit kỳ vọng Cannon là 1.075, tương ứng 32.25 damage kỳ vọng mỗi mục tiêu khi damage gốc là 30.

Đã chỉ rõ lỗi dữ liệu đang chặn cân bằng: Archer.asset damage=1 nhưng Bullet mặc định damage=20; Tower chưa truyền damage; Enemy có 100HP chung và thưởng 25 Gold chung; wave không tăng độ khó. Không chỉnh asset để tạo cảm giác đã cân bằng khi dữ liệu chưa được runtime sử dụng.

Đã đề xuất baseline theo GDD và phương án thử bonus hoàn thành wave 25 Gold để wave đầu 5 Goblin có tổng 50 Gold. Đây là đề xuất có lý do, chưa áp dụng kinh tế vào game khi placement còn thiếu. Có mẫu ghi lượt chơi và ma trận ba chiến thuật × ba lượt để QA thực hiện sau tích hợp.

Chi tiết: [BALANCE_REVIEW.md](Balance/BALANCE_REVIEW.md).

## Lead và báo cáo đã thực hiện

Đã chuyển phạm vi GDD thành bảng nhiệm vụ có ID, owner đề xuất, trạng thái, phụ thuộc và điều kiện nghiệm thu. Đã ghi thứ tự tích hợp một map MVP trước khi mở rộng, checklist metadata/Git/build và điều kiện chấp nhận bản Windows.

Các owner được suy từ lịch sử Git để Lead dùng khi trao đổi. Phiên này không gửi thông báo giao việc, không xác nhận người khác đã nhận việc và không ghi nhận commit của người khác là đóng góp cá nhân.

Chi tiết: [PROGRESS_AND_HANDOFF.md](Lead/PROGRESS_AND_HANDOFF.md).

## Phần cần tiếp tục sau bàn giao

| Phần | Vì sao chưa đóng | Người phối hợp đề xuất |
|---|---|---|
| Âm đặt tháp qua thao tác thật | Thiếu placement và giao dịch Gold | Lan Anh + Gia Bảo |
| Âm Cannon trong trận | Thiếu prefab Cannon sử dụng dữ liệu loại | Gia Bảo + Trung Tín |
| Nghiệm thu độ khó và kinh tế | Chưa có vòng mua tháp, hai quái và HUD | Toàn nhóm; Văn Khải ghi playtest |
| Mix/loop qua nghe | Cần duyệt cảm nhận bằng tai nghe/loa | Văn Khải |
| Asset cũ để tham khảo | Nguồn chưa xác minh; runtime đã được thay toàn bộ | Chỉ cần xác minh nếu muốn dùng lại bản cũ |
| Tích hợp remote main | Chưa push/merge trong phiên này | Lead theo quy trình nhóm |

## Cách sử dụng và chạy lại

Mở một trong Level_1–3 bằng Unity 6000.6.0f1, Play, chọn SOUND góc phải. Dùng slider Music/Effects, TEST SOUND và CLOSE. Bảng âm thanh không dừng trận đấu.

Chạy `python Tools/audio_audit.py` và `python Tools/repository_audit.py` để kiểm tra offline. Chạy `powershell -ExecutionPolicy Bypass -File Tools/run_sound_qa.ps1` sau khi đóng Editor cùng project để chạy Play Mode, build và smoke trên Windows executable.

Build QA đặt ở `Builds/QA/KingdomGuard.exe`, cùng thư mục dữ liệu và DLL của Unity. Khi chuyển máy phải mang cả thư mục Builds/QA, không chỉ file exe. Build này dùng để kiểm tra kỹ thuật; chưa phải bản phát hành MVP.

## Tình trạng Git

Thay đổi được chuẩn bị tại working tree nhánh van-khai. GDD và settings tutorial có sẵn trên máy được giữ nguyên. Không tự commit, push hoặc merge remote để tránh đưa lẫn các thay đổi có sẵn vào lịch sử nhóm. Báo cáo máy và công cụ có thể được review trước khi Lead tạo commit riêng.
