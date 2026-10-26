# Kiểm tra cân bằng Kingdom Guard

Ngày 26/10/2026. Đây là phân tích số liệu và kế hoạch thử nghiệm của Sound/QA/Lead. Chưa nghiệm thu độ khó qua lượt chơi tự nhiên vì placement, HUD, hai loại tháp/quái chưa tích hợp.

## Công thức và giá trị mục tiêu từ GDD

DPS đơn mục tiêu = damage / shootInterval. Số phát để giết = ceil(HP / damage). Thời gian từ phát đầu tới phát kết liễu = (số phát − 1) × shootInterval + thời gian bay đạn. HP/DPS là xấp xỉ trung bình, không thay thế công thức phát bắn rời rạc.

| Loại | Giá | Damage gốc | Nâng cấp damage | Giá nâng cấp mỗi cấp | Vai trò |
|---|---:|---:|---|---:|---|
| Archer | 50 | 10 | 10 → 13 → 16.9 | 30 | Đơn mục tiêu |
| Cannon | 100 | 30 | 30 → 39 → 50.7 | 60 | AOE, crit 15%, hệ số 1.5 |
| Mage, mở rộng | 150 | 20 | 20 → 26 → 33.8 | 90 | AOE; cấp 3 slow 30% trong 2s |

Archer bắn mỗi 0.5s là ví dụ cân bằng trong GDD, chưa phải giá trị đã được game sử dụng. Với ví dụ đó: DPS=20, Goblin 50HP cần 5 phát, khoảng 2s từ phát đầu + thời gian bay; Orc 150HP cần 15 phát, khoảng 7s + thời gian bay.

Cannon: hệ số damage kỳ vọng = 0.85 × 1 + 0.15 × 1.5 = 1.075; damage kỳ vọng mỗi mục tiêu = 32.25. Với 3 mục tiêu chịu toàn bộ vùng nổ, tổng damage kỳ vọng = 96.75/phát. Phải chốt khoảng bắn và bán kính AOE trước khi so DPS với Archer. Chưa thể coi Cannon cân bằng bằng cách so 30 và 10.

## Chênh lệch đang có trong bản triển khai

| Dữ liệu | Hiện tại | Hệ quả |
|---|---|---|
| Archer.asset | damage=1, interval=1, range=3 | Damage asset chưa được Tower truyền sang Bullet |
| Bullet | damage mặc định=20 | Chỉnh damage trong asset chưa điều khiển damage thực |
| Enemy | HP cố định=100; prefab speed=0.5 | Chưa phân biệt Goblin/Orc |
| Thưởng hạ quái | 25 mỗi con trong WaveSpawner | Chưa dùng thưởng 5/10 theo loại |
| Wave | 10 wave, mỗi wave 5 quái, nghỉ 3s | Không tăng độ khó; khác nghỉ 10–15s trong GDD |

Nếu gắn Archer.asset hiện tại vào Tower và dùng Bullet mặc định: DPS danh nghĩa đang là 20/1=20, không phải 1/1=1. Đây chỉ là cấu hình giả định để tái hiện lỗi dữ liệu; scene hiện chưa gắn tháp. Không dùng con số này làm kết quả playtest.

## Kinh tế và đề xuất baseline

Giữ startGold=100, startLives=20. Ưu tiên baseline Archer 50 Gold, Goblin 50HP/5 Gold và Orc 150HP/10 Gold sau khi programmer nối dữ liệu. Theo các mức thưởng GDD, wave 5 Goblin chỉ cho 25 Gold, chưa đủ mua một Archer mới. Điều này mâu thuẫn với mong muốn thu nhập mỗi wave đủ khoảng một tháp; cần lựa chọn thiết kế rõ ràng.

Đề xuất thử đầu tiên: giữ thưởng theo loại và thêm bonus hoàn thành wave 25 Gold. Khi đó wave 1 có thể cho 50 Gold nếu diệt đủ 5 Goblin. Đây là đề xuất cho programmer triển khai và QA đo lại, chưa áp dụng vào runtime để tránh thay đổi kinh tế khi vòng mua tháp chưa tồn tại.

MVP nên giữ 5–6 wave và chỉ Archer/Cannon + Goblin/Orc. Không thêm Mage/Bat/Troll trước khi vòng cốt lõi được nghiệm thu.

## Phiếu playtest cần điền sau khi mở phụ thuộc

Mỗi lượt ghi: mã build/commit, map, seed RNG nếu có, chiến thuật, tháp đã mua, nâng cấp, tiền trước/sau wave, số quái lọt, máu còn lại, wave cuối, thời lượng, nhận xét và lỗi. Không ghi kết quả mô phỏng/ép thắng thành lượt chơi thật.

Chạy tối thiểu 3 lượt cho từng chiến thuật: chỉ Archer, phối hợp Archer/Cannon, đặt ở vị trí kém. Tách chế độ Cannon không crit và crit cố định khi đánh giá damage nền; sau đó chạy nhiều lượt RNG thật. Mục tiêu: chiến thuật hợp lý có khả năng thắng; phối hợp tháp có lợi rõ; không bắt buộc thao tác nhanh; không vượt 15 phút. Chưa có kết quả nghiệm thu cho các mục tiêu này.
