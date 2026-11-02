# Đánh giá tiến độ Kingdom Guard ngày 2 tháng 11 năm 2026

Đối tượng kiểm tra: nhánh van-khai tại merge commit 303595d, đã tích hợp origin/main e93506c. Đối chiếu Kingdom_Guard_GDD.docx. Không chỉnh gameplay hoặc tự commit trong phiên đánh giá này.

Game đã chuyển từ prototype thiếu vòng mua tháp sang bản có vòng gameplay MVP được triển khai và nhiều thao tác cốt lõi đã chạy qua kiểm tra Play Mode. Chưa thể nghiệm thu game hoàn chỉnh vì chưa kiểm thử thắng tự nhiên, cân bằng cả lượt chơi, build Windows mới và các trường hợp UI/âm thanh còn thiếu tích hợp.

## Bằng chứng kiểm tra mới

Đã chạy Unity 6000.6.0f1 trong Play Mode với một probe tạm, sau đó gỡ probe khỏi Assets. Kết quả: **35 assertion đạt, 0 assertion thất bại, không ghi nhận Error/Exception runtime trong kịch bản đã chạy**. Kết quả máy: [progress_runtime_2026-11-02.json](progress_runtime_2026-11-02.json). Log chi tiết ở Logs/ProgressAudit20261102.log.

Kịch bản đã chạy: mở MainMenu; tải lần lượt ba Level; khởi tạo node placement; kiểm tra prefab tháp/quái/HUD; mua Archer/Cannon bằng controller thật; nâng cấp Archer đến cấp 3; bán hoàn 50% tổng tiền; thử nâng cấp khi thiếu Gold; bắt đầu wave; spawn Goblin; áp sát thương đạn từ TowerData; nhận thưởng 5 Gold; pause/resume; kích hoạt Win có kiểm soát; replay; đặt HP=0 để kiểm tra Lose; trở về menu.

Các thao tác mua/nâng cấp/bán gọi method của controller bằng reflection, không phải click chuột native. Phép thử sát thương tắt tự bắn của tháp test để đo damage một đạn riêng biệt. Win được gọi trực tiếp; Lose do test đặt HP=0. Không dùng các kết quả này để khẳng định người chơi có thể thắng tự nhiên, độ khó đã cân bằng hoặc giao diện đã được duyệt bằng thao tác thực tế.

## So với MVP trong GDD

| Tiêu chí MVP | Hiện tại | Mức xác minh |
|---|---|---|
| Một map | Có ba Level, tương ứng 1/2/4 đường waypoint; có MainMenu riêng | Cả ba scene khởi tạo được trong Play Mode |
| Hai loại tháp Archer/Cannon | Có TowerDefinition, ba cấp mỗi loại, mua/nâng cấp/bán, range preview và targeting First | Mua hai loại và chu kỳ nâng cấp/bán Archer đã đạt; chiến đấu Cannon AOE/crit mới kiểm tra code/config |
| Hai loại quái Goblin/Orc | Có prefab riêng, HP/tốc độ/thưởng/sát thương lâu đài riêng; có thêm Boss | Prefab có trong ba Level; Goblin spawn 50HP, thưởng 5 Gold đã đạt; chưa chạy hết đội hình Orc/Boss |
| Gold | Có 200 Gold đầu lượt; trừ mua/nâng cấp, cộng thưởng và hoàn 50% khi bán | Các giao dịch đã kiểm tra bằng controller thật |
| Wave 5–6 cho MVP | Level 1 có 6 wave; bắt đầu sau khi đặt tháp, nghỉ và bắt đầu sớm | Spawn đầu wave và trạng thái nhạc đã đạt; chưa chơi hết 6 wave |
| Win/Lose | Có panel, thống kê, retry/replay, next level/menu | Điều kiện Win/Lose có kiểm soát và replay đã đạt |
| HUD cơ bản | Gold, HP, wave, số quái còn lại, nút wave; pause và speed 1x/2x/3x | HUD được khởi tạo, prefab có reference; pause/resume đã đạt; chưa duyệt toàn bộ hiển thị và input native |

Như vậy cả **7 nhóm tính năng MVP đã hiện diện ở mức triển khai**. Đây không phải chứng nhận 7/7 nhóm đã hoàn tất toàn bộ test chấp nhận. Giai đoạn hiện tại phù hợp nhất là **kiểm thử tích hợp, cân bằng và hoàn thiện trước nghiệm thu MVP**.

## Tiến bộ từ bản ngày 26 tháng 10

- Đã có BuildManager thực sự tạo node từ tilemap và đặt tháp, thay cho khoảng trống placement trước đây.
- Đã có TowerDefinition/TowerData cho Archer/Cannon/Magic và ba cấp; damage được truyền vào Bullet.Init. Lỗi damage asset không điều khiển đạn trước đây đã được xử lý ở code mới và xác minh bằng một phép thử đạn Archer.
- Đã có Goblin/Orc riêng và Boss, thưởng theo loại, máu tăng theo wave; EnemyData không còn rỗng.
- Targeting dùng quãng đường còn lại đến đích, thay cho quái gần tháp nhất.
- Đã có HUD, Pause, Guide trong menu, chuyển ngôn ngữ, speed và tooltip.
- Có effect chết, coin drop, projectile impact và crit; có sprite/animation bổ sung.

## Các điểm còn thiếu hoặc cần chốt

| Ưu tiên | Phát hiện | Ý nghĩa / hành động |
|---|---|---|
| Cao | Chưa có lượt chơi tự nhiên được ghi nhận qua đủ wave | Chơi ít nhất ba lượt/chiến thuật, ghi thời gian, Gold, HP, wave và lý do thất bại; chưa gắn nhãn cân bằng đạt |
| Cao | Level 2–3 dùng 10 wave/3s nghỉ nhưng chỉ có 6 waveConfigs | Code clamp index nên wave 7–10 lặp cấu hình thứ 6; chốt đây là thiết kế mong muốn hay đồng bộ về 6 wave/12s |
| Cao, cá nhân Sound | BuildManager không gọi PlayTowerPlace/PlayBuildFailed; BuildAudioFeedback không được nối trong scene/prefab | Placement đã có nhưng âm thành công/thất bại vẫn chưa đi theo thao tác mua thật; giờ có thể nối trực tiếp |
| Vừa | BuildManager đọc Pointer trực tiếp, không kiểm tra IsPaused hoặc EventSystem.IsPointerOverGameObject | Có nguy cơ thao tác dưới pause/settings/HUD tác động menu tháp; quan sát từ code, chưa tái hiện bằng click native |
| Vừa | Mage/Magic có ba cấp nhưng splashRadius=0; không có logic slow | Đang là tháp damage đơn mục tiêu, chưa đạt AOE và slow 30%/2s ở cấp 3 như GDD; có thể bỏ khỏi MVP |
| Vừa | Asset Bat và isFlying có nhưng không có tuyến spawn Bat trong wave | Chưa tính Bat là loại quái gameplay hoàn thành |
| Vừa | Có sprite shooting nhưng Tower chưa có logic chuyển frame/animation khi bắn | Phần asset và phần animation chiến đấu tích hợp cần nghiệm thu riêng |
| Vừa | QA runner cũ còn kỳ vọng kiến trúc menu/wave ngày 26/10 | Cập nhật trước khi dùng để chấm bản mới; 35 phép thử của phiên này là bộ riêng, không ghi lại kết quả 22 test cũ thành kết quả mới |
| Thấp / mở rộng | Chưa thấy phép hỗ trợ, xếp sao, shake/viền đỏ khi mất máu | Không chặn MVP theo mục 9; chỉ mở rộng sau khi lõi ổn định |
| Thấp | Tooltip tháp chỉ có tên/cấp/tầm; Guide mở thủ công; Space bắt đầu/skip wave | Khác một số mô tả GDD: tooltip có damage, tutorial lần đầu và Space pause. Cập nhật GDD hoặc thống nhất hành vi |

Không thấy guard ở đầu TakeDamage/Die ngăn gọi chết lặp trong cùng frame; NotifyRemoved có guard nên chống thưởng Gold trùng. Vẫn cần test hai đạn trúng cùng lúc để xác minh hiệu ứng chết/coin không nhân đôi. Đây là rủi ro cần test, không ghi là lỗi đã tái hiện.

## Cấu hình thực đọc được trong Play Mode

| Scene | Gold đầu | Wave | Số cấu hình | Nghỉ | Đường đi |
|---|---:|---:|---:|---:|---:|
| Level 1 | 200 | 6 | 6 | 12s | 1 |
| Level 2 | 200 | 10 | 6 | 3s | 2 |
| Level 3 | 200 | 10 | 6 | 3s | 4 |

Archer cấp 1: giá 50, damage 10, khoảng bắn 0.5s; DPS danh nghĩa 20. Cannon cấp 1: giá 100, damage 30, khoảng bắn 1.5s, AOE radius 1, crit 15% ×1.5; DPS kỳ vọng trên một mục tiêu khoảng 21.5, giá trị tăng khi trúng nhiều mục tiêu. Chưa tính đường đi, đạn bay, overkill và vị trí đặt nên các số này không chứng minh độ khó đã cân bằng.

Máu quái hiện tăng tuyến tính 10% mỗi wave; GDD gợi ý cấp số nhân 15–20%. Gold đầu là 200 thay vì ví dụ 100. Đây là lựa chọn cân bằng cần xác nhận bằng playtest, không tự coi mọi sai khác số tham khảo là bug.

## Tiến độ từng thành viên

Vai trò và tên đối chiếu theo lịch sử Git, không dựa trên số commit để chấm đóng góp. Những commit merge do người tích hợp tạo không làm thay đổi tác giả asset/code gốc.

| Thành viên | Kết quả hiện có | Cần hoàn tất |
|---|---|---|
| Gia Bảo, Gameplay/integration | GameManager, wave, chỉ số quái, First targeting, damage/crit/AOE, HUD/UX và tích hợp scene đã tiến nhiều | Đồng nhất cấu hình Level 2–3, test chiến đấu và điều kiện cuối wave, xử lý edge case, phối hợp cân bằng |
| Lan Anh, Level/UI | Placement, nâng cấp/bán, map/waypoint; cập nhật hiển thị và speed | Kiểm tra click xuyên UI/pause, bố cục đa độ phân giải, hỗ trợ nối âm placement |
| Trung Tín, Art/Animation | Prefab/asset tháp, projectile, effects; animator Goblin/Orc/Bat/Boss và sprite shooting | Nghiệm thu asset khi chạy trận; nối animation bắn; Bat chưa được spawn; kiểm tra consistency |
| Văn Khải, Sound/QA/Lead | 12 audio có nguồn tái tạo, state music/volume, report/bug log/balance review; phiên này thêm 35 kiểm tra runtime | Nối hai hook placement, nghe mix/loop, cập nhật suite QA cho main mới, playtest cân bằng, build Windows mới và cập nhật các ticket đã được main xử lý |

Các cập nhật chính mới nhất theo Git: Gia Bảo có HUD/core/UI ngày 26–31/10; Lan Anh có placement ngày 25/10 và speed/name ngày 29/10; Trung Tín có animator ngày 22/10 và art ngày 26/10. Phần Sound/QA/Lead lưu ở 4d714af và được giữ khi merge 303595d.

## Thứ tự hoàn thiện đề xuất

1. Chốt một Level 1 làm màn nghiệm thu MVP, kiểm tra thắng/thua tự nhiên và toàn bộ 6 wave.
2. Nối âm placement và chặn input thế giới khi thao tác UI/pause; retest giao dịch Gold.
3. Chốt cấu hình Level 2–3, tạm loại Mage/Bat khỏi tiêu chí bắt buộc nếu chưa đúng chức năng.
4. Cập nhật suite tự động, chạy các edge case, duyệt input/UI và nghe âm thanh.
5. Build Windows mới từ commit hiện tại, smoke cả bốn scene và thử trên máy khác.

## Phạm vi và kết luận

Phiên này không build Windows, không chạy trên máy khác, không duyệt âm thanh bằng nghe và không chơi thắng tự nhiên. Các ảnh/build/báo cáo ngày 26/10 không được dùng làm bằng chứng bản hiện tại đã vượt qua các bước đó. Các phép thử không lưu cấu hình thử vào scene; PlayerPrefs về pending level, unlock và completion được khôi phục.

Kết luận: **phần lõi đã tiến rõ và đủ để bắt đầu nghiệm thu MVP có hệ thống; phần còn lại tập trung vào tích hợp cuối, sửa sai lệch cấu hình, kiểm thử và cân bằng, thay vì viết lại các hệ thống nền.**
