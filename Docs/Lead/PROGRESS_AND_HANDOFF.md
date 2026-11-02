# Tiến độ và bàn giao nhóm

Ngày 26/10/2026. Phạm vi nguồn: GDD, nhánh van-khai, lịch sử Git local và kiểm tra tự động đi kèm. Vai trò gắn tên theo lịch sử đóng góp, owner dưới đây là đề xuất để Lead xác nhận với nhóm; chưa gửi giao việc cho bất kỳ ai.

## Phạm vi và điều kiện hoàn thành

MVP: một map, Archer/Cannon, Goblin/Orc, Gold, 5–6 wave, thắng/thua và HUD. Một task chỉ Done khi code/asset đã tích hợp vào scene, test chấp nhận đạt và có bằng chứng. Sprite, hàm API hay màn debug riêng không đủ để đóng task gameplay đầu cuối.

## Bảng công việc

| ID | Hạng mục | Owner đề xuất | Trạng thái | Điều kiện nghiệm thu |
|---|---|---|---|---|
| S01 | AudioManager, trạng thái nhạc, volume UI | Văn Khải | Implemented, xem kết quả QA | Đủ clip, chuyển state, volume và scene persistence đạt |
| S02 | Âm mua tháp thành công/thất bại | Văn Khải + Lan Anh | API ready; phụ thuộc placement | Một lần phát sau giao dịch; thất bại không mất Gold |
| S03 | Âm Cannon | Văn Khải + Gia Bảo | Routing ready; phụ thuộc prefab | Cannon dùng shotSound=Cannon; Archer dùng Arrow |
| S04 | Nghe và duyệt mix cuối | Văn Khải | Chưa nghiệm thu cảm nhận | Nghe loop, 10+ tháp, cảnh báo, Win/Lose ở tai nghe/loa |
| Q01 | Test plan, bug log, smoke automation | Văn Khải | Đã có tài liệu và runner | Có kết quả máy riêng, không giả định manual PASS |
| Q02 | Cân bằng số liệu | Văn Khải + Gia Bảo | Phân tích xong; playtest bị chặn | Chỉ số nối runtime, đủ phiếu lượt chơi và chỉnh lại |
| G01 | Placement và mua tháp | Lan Anh + Gia Bảo | Open, chặn MVP | Ô hợp lệ, đủ tiền, trừ đúng một lần; placement không đi xuyên UI |
| G02 | TowerData/EnemyData và 2 loại tháp/quái | Gia Bảo | Open, chặn MVP | Damage/tốc độ/thưởng đúng loại; Cannon AOE |
| U01 | HUD Gold/HP/Wave | Lan Anh | Open, chặn MVP | Cập nhật theo sự kiện, không hiển thị sai khi replay |
| A01 | Prefab/animation chiến đấu | Trung Tín | Asset có; tích hợp còn thiếu | Asset đúng loại trong scene, clip không bị mất reference |
| G03 | Wave tăng độ khó, First targeting | Gia Bảo | Open | Chốt 5–6 wave có đội hình; chọn quái tiến xa nhất |
| L01 | Báo cáo và release checklist | Văn Khải | Đã soạn; chưa duyệt phát hành | Build và test thật đạt, không còn blocker |

## Trình tự tích hợp

1. Chốt placement, prefab và dữ liệu cho một map; các map còn lại để sau.
2. Nối HUD và chạy 5–6 wave. Giữ âm thanh như thư viện chung, không copy AudioManager vào từng scene.
3. Nối các hook placement/Cannon theo `Docs/QA/SOUND_INTEGRATION.md`.
4. Xử lý bug mức blocker/high, chạy lại automation, playtest cân bằng và duyệt âm thanh.
5. Kiểm tra bản Windows trên máy khác rồi mới nghiệm thu MVP.

## Checklist bàn giao Git

- Bản working tree đã có Build Settings trỏ đúng Level_1–3; giữ sửa đổi có sẵn này.
- Thêm toàn bộ `.meta` cùng script/audio mới; kiểm tra không trùng GUID.
- Không đưa Library/Temp/Logs/Builds vào Git; build QA ở Builds/QA được ignore.
- GDD và settings tutorial có sẵn trên máy được giữ nguyên. Không tự gộp chúng vào commit code.
- Commit riêng thay đổi Sound/QA/Lead rồi tích hợp vào main qua quy trình nhóm. Không force push hoặc công bố bản release khi blocker chưa đóng.
- Phiên làm việc này chuẩn bị thay đổi local; không khẳng định đã push, mở PR, gửi giao việc hoặc merge remote.
