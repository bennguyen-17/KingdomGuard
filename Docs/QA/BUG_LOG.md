# Nhật ký lỗi và phụ thuộc

Cập nhật 02/11/2026. Mô tả giữ lại vấn đề gốc; trạng thái đã đối chiếu main mới, progress audit và QA Sound. Owner là đề xuất, chưa gửi giao việc.

| ID | Mức | Vấn đề gốc | Owner đề xuất | Trạng thái mới nhất |
|---|---|---|---|---|
| KG001 | Blocker | Thiếu placement | Lan Anh + Gia Bảo | Đã có; giao dịch mua/nâng cấp/bán đạt qua controller ngày 02/11; chưa click native |
| KG002 | High | Damage asset không truyền sang Bullet | Gia Bảo | Đã nối Bullet.Init; test sát thương Archer đạt ngày 02/11 |
| KG003 | High | Một prefab quái, EnemyData rỗng | Gia Bảo | Đã có Goblin/Orc/Boss riêng; Goblin runtime đạt; chưa chơi đủ đội hình |
| KG004 | High | Thiếu HUD | Lan Anh | Đã có trong ba Level; chưa duyệt toàn bộ hiển thị/input native |
| KG005 | Medium | Targeting nearest thay First | Gia Bảo | Code đã dùng khoảng cách tới đích; chưa test nhiều mục tiêu |
| KG006 | Medium | Wave chưa có đội hình/độ khó | Gia Bảo | Level 1 đã 6 wave/12s; Level 2–3 vẫn 10 wave/3s với 6 config, cần chốt |
| KG007 | High | Build Settings trỏ scene cũ | Văn Khải | Đạt build MainMenu + ba Level |
| KG008 | Medium | Mọi tháp phát âm cung | Văn Khải | Đạt Tower.Shoot cho ba cue riêng; gán shotSound cả chín asset |
| KG009 | Medium | Chưa nối âm placement/rejected | Văn Khải | Đã nối trực tiếp; 100 lần thiếu Gold chỉ một error; giao dịch thành công đúng một cue |
| KG010 | Medium | BGM chạy sai trạng thái/kết quả | Văn Khải | Đạt wave/intermission/early start/Win/Lose/replay/menu; fade dùng unscaled time |
| KG011 | Medium | Thiếu UI volume | Văn Khải | Slider/mute/lưu qua process đạt; sửa panel dưới phải, ảnh ba độ phân giải đạt; native chưa duyệt |
| KG012 | Medium | Combat có thể quá dày | Văn Khải | Cap 4 voice, headroom 0.65; PCM/mix giả lập đạt; nghe cảm nhận chưa duyệt |
| KG013 | Medium | Audio cũ không rõ nguồn | Văn Khải | 13 clip runtime tổng hợp gốc; BuildReport loại thư mục cũ |
| KG014 | Low | Hai thư mục audio | Văn Khải | Resources là nguồn runtime; giữ bản cũ riêng, không đóng gói |
| KG015 | Medium | Editor hot reload làm mất state UI | Gia Bảo | Chưa sửa khả năng hot reload; QA dùng khởi động sạch và khôi phục Editor settings |
| KG016 | Medium | System.Windows.Forms.dll không được hỗ trợ | Gia Bảo | Còn cảnh báo build; smoke bốn scene và process volume đạt; chưa khoanh vùng nguồn |
| KG017 | Medium | SOUND đè nút wave/pause | Văn Khải | Đã chuyển xuống dưới phải; render 720p/900p/1080p đã xem; chặn riêng vùng pointer SOUND |

Quan sát code không đồng nghĩa đã tái hiện bằng người chơi. Nguy cơ effect chết/coin trùng khi hai đạn cùng frame vẫn cần test GP04; NotifyRemoved đã có guard chống thưởng Gold trùng. Không đóng các bug gameplay chỉ dựa vào bộ kiểm tra Sound.
