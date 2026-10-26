# Nhật ký lỗi và phụ thuộc

Ngày 26/10/2026. Open = chưa sửa. Ready for retest = đã có thay đổi, xem kết quả tự động riêng. Blocked = phụ thuộc tính năng chưa có. Owner là đề xuất từ Lead, chưa gửi thông báo giao việc.

| ID | Mức | Hiện trạng / cách tái hiện | Mong đợi | Owner đề xuất | Trạng thái |
|---|---|---|---|---|---|
| KG001 | Blocker | Mở scene, không có Tower component hoặc controller đặt tháp | Người chơi mua và đặt Archer/Cannon | Lan Anh + Gia Bảo | Open |
| KG002 | High | Sửa TowerData.damage, bắn bằng Bullet mặc định; Tower không truyền damage | Damage thực lấy từ dữ liệu tháp | Gia Bảo | Open, kiểm tra tĩnh |
| KG003 | High | Cả ba WaveSpawner dùng cùng Enemy prefab; EnemyData rỗng | Goblin/Orc có chỉ số và thưởng riêng | Gia Bảo | Open |
| KG004 | High | Chơi trận không có HUD Gold/HP/Wave | Theo dõi tài nguyên và tiến độ trận | Lan Anh | Open |
| KG005 | Medium | Đọc Tower.UpdateTarget: dùng khoảng cách tới tháp | First theo tiến độ trên đường | Gia Bảo | Open |
| KG006 | Medium | Tất cả wave dùng 5 quái, một prefab, nghỉ 3s | Cấu hình đội hình và nghỉ 10–15s, MVP 5–6 wave | Gia Bảo | Open |
| KG007 | High | Build Settings ở commit nền trỏ Game.unity đã không tồn tại | Level_1–3 có trong build | Văn Khải | Working tree đã đúng từ trước; xác minh bằng build mới |
| KG008 | Medium | Audio cũ luôn phát Arrow cho mọi Tower | Chọn âm theo loại tháp | Văn Khải | Routing đã sửa; gameplay Cannon bị chặn KG001/KG003 |
| KG009 | Medium | Chưa có âm base damage và placement rejected | Có cue rõ, không spam | Văn Khải | Hook base đã nối; hook placement ready, bị chặn KG001 |
| KG010 | Medium | Nhạc một track, vẫn chạy trên bảng kết quả | Phân trạng thái, dừng combat ở kết quả | Văn Khải | Đã sửa; retest tự động trong JSON |
| KG011 | Medium | Không có UI volume | Có Music/Effects, phần trăm, lưu | Văn Khải | Đã thêm; manual layout/chuột chưa nghiệm thu |
| KG012 | Medium | Nhiều shoot/hit có thể phát cùng lúc | Hạn chế tần suất, ưu tiên cảnh báo | Văn Khải | Đã giới hạn; cần nghe stress test |
| KG013 | Medium | Người dùng xác nhận WAV cũ do AI lấy trên Internet, không biết nguồn | Bộ runtime có nguồn tái tạo được | Văn Khải | Đã thay toàn bộ 12 clip runtime bằng tổng hợp gốc; nguồn cũ chỉ giữ trong Assets/Audio, kiểm tra không đóng gói bằng BuildReport |
| KG014 | Low | Runtime audio có bản sao ở Assets/Audio và Resources/Audio | Một nguồn runtime rõ ràng | Văn Khải | Đã quy định Resources là runtime; giữ bản cũ an toàn |
| KG015 | Medium | Editor nạp lại assembly sau khi đã vào Play Mode có thể làm mất state UI/runtime và gây NullReference tại UIManager.Show | Hot reload không hỏng state hoặc yêu cầu khởi động lại Play Mode rõ ràng | Gia Bảo | Quan sát trong lượt runner đầu; lần khởi động sạch đạt. Runner dùng domain reload và khôi phục tùy chọn Editor sau kiểm tra; chưa sửa khả năng hot reload của game |
| KG016 | Medium | Build cảnh báo System.Windows.Forms.dll không được hỗ trợ trên platform hiện tại | Không kéo assembly không hỗ trợ vào player | Gia Bảo | Open; chưa khoanh vùng nguồn tham chiếu, smoke 3 scene vẫn đạt |

Các quan sát tĩnh không được ghi là đã tái hiện qua thao tác người chơi. Rủi ro hai đạn cùng giết một quái được giữ thành test GP04, chưa gán là lỗi đã tái hiện.
