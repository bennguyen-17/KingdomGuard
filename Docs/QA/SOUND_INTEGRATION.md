# Bàn giao âm thanh

Ngày 26/10/2026. Runtime dùng duy nhất `Assets/Resources/Audio`. Thư mục `Assets/Audio` cũ được giữ để tránh xóa dữ liệu của người dùng, không được dùng làm nguồn thứ hai khi phát nhạc.

## Các trạng thái

| Trạng thái | Điểm gọi | Kết quả |
|---|---|---|
| Menu | UIManager.ShowMainMenu | Nhạc Preparation, dừng hiệu ứng còn lại |
| Battle | WaveSpawner.StartWave | Nhạc Battle, không tạo thêm AudioManager |
| Preparation | Hết wave, còn wave tiếp theo | Nhạc Preparation |
| Result | ShowWinScreen / ShowLoseScreen | Dừng BGM và âm chiến đấu, phát Victory hoặc Defeat |

Âm chiến đấu, âm cảnh báo/kết quả và âm UI dùng các AudioSource tách nhau. Nhờ đó thao tác về menu có thể dừng tiếng kết quả mà không cắt mất tiếng click. Âm chiến đấu giảm gain, giới hạn một lần mỗi 40 ms toàn hệ thống và 100 ms mỗi clip để giảm chồng tiếng. Đây là giới hạn tần suất, không phải limiter/master compressor; vẫn cần nghe thử mật độ cao.

## Nối placement khi hệ thống được bổ sung

Gắn `BuildAudioFeedback` vào controller đặt tháp. Chỉ gọi `PlacementSucceeded()` sau khi kiểm tra ô, đủ tiền, tạo tháp và trừ Gold thành công. Gọi `PlacementRejected()` khi thiếu tiền hoặc ô không hợp lệ. Các public method có thể nối trực tiếp từ UnityEvent. Không gọi success từ `Tower.Start`, vì tháp dựng sẵn và tháp preview không phải lượt mua.

Placement controller hiện chưa tồn tại trong dự án. Vì vậy đây là hợp đồng tích hợp đã chuẩn bị, chưa phải xác nhận âm đặt tháp đã phát qua thao tác người chơi. Owner tích hợp đề xuất: Lan Anh; QA: Văn Khải.

## Nối loại tháp

`TowerData.shotSound`: Arrow=0 (giữ tương thích Archer hiện tại), Cannon=1, Magic=2. Cannon phát âm pháo; Magic không mượn âm cung. Khi làm prefab Cannon, gán TowerData có `shotSound=Cannon`. Chưa có prefab Cannon hoạt động để nghiệm thu đầu cuối.

## Điều chỉnh và lưu âm lượng

Nút SOUND góc phải mở Music/Effects, phần trăm, TEST SOUND và CLOSE. Slider hỗ trợ chuột và điều hướng UI bàn phím. Các giá trị được lưu khi đóng bảng, tạm ngừng ứng dụng, thoát ứng dụng và hủy UI khi chuyển scene. Bảng không tự pause trận đấu. AudioSource là 2D và vẫn phát khi gameplay có timeScale=0.

## Nguồn và quyền sử dụng

Người dùng xác nhận các clip cũ do AI lấy từ Internet và không biết nguồn cụ thể. Vì vậy toàn bộ 12 clip runtime trong Resources/Audio đã được thay bằng âm thanh tổng hợp gốc từ công thức trong `Tools/audio_audit.py`, không dùng mẫu nhạc bên ngoài. Có thể tái tạo bằng `python Tools/audio_audit.py --generate-originals`. BGM Preparation/Battle dài 16s; toàn bộ 10 SFX ngắn hơn 1s. Nguồn và SHA256 được ghi trong `audio_provenance.json` và `audio_audit.json`.

Các file cũ trong Assets/Audio được giữ nguyên để người dùng tham khảo, có nguồn chưa xác minh. Không dùng hoặc phân phối thư mục đó như bộ asset đã được duyệt. Runner kiểm tra BuildReport để bảo đảm build không đóng gói asset từ Assets/Audio. Thay file âm thanh trong Resources giữ nguyên `.meta` để bảo toàn GUID.

Chạy `python Tools/audio_audit.py` chỉ đọc âm thanh và cập nhật số đo. Số đo PCM không xác minh chất lượng nghe, điểm loop cảm nhận hay license.
