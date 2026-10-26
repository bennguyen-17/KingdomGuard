# Kết quả kiểm thử ngày 26 tháng 10 năm 2026

Môi trường: Windows, Unity 6000.6.0f1. Nhánh van-khai, commit nền 768d638 cộng thay đổi local Sound/QA/Lead. Bằng chứng máy trong các JSON đi kèm; log đầy đủ ở thư mục Logs được Git ignore.

| Nhóm kiểm tra | Kết quả cuối | Bằng chứng |
|---|---|---|
| Unity C# compile | Thành công | Logs/SoundQaVerification.log |
| Unity Play Mode | 22 assertion đạt, failure rỗng | [unity_verification.json](unity_verification.json) |
| Windows x64 development build | Succeeded, 0 lỗi, 2 cảnh báo ở lượt cuối | Cùng JSON; Builds/QA/KingdomGuard.exe |
| Executable smoke | Mã thoát 0, cả 3 scene đạt | [player_smoke.json](player_smoke.json) |
| PCM audio | 12 clip có tín hiệu, 0 sample chạm ngưỡng clipping | [audio_audit.json](audio_audit.json) |
| Nguồn audio runtime | 12 clip tổng hợp gốc bằng công thức, không dùng mẫu ngoài | [audio_provenance.json](audio_provenance.json) |
| Loại trừ clip cũ khỏi build | legacyAudioIncluded=false qua BuildReport.packedAssets | [unity_verification.json](unity_verification.json) |
| Tái tạo audio | Sinh lại cả 12 clip cho đúng cùng SHA256 với bộ đã build | Tools/audio_audit.py và audio_provenance.json |
| Tính toàn vẹn repo | 5 kiểm tra đạt: 3 scene/GUID, GUID không trùng, metadata đầy đủ | [repository_audit.json](repository_audit.json) |
| Bố cục bảng SOUND | Đã xem ảnh 1280×720, 1600×900, 1920×1080 | [Visual](Visual/) |
| Chuột/bàn phím native | Chưa chạy trực tiếp | Computer Use không kết nối được native pipe sau retry và reset |
| Nghe loop, mix mật độ cao | Chưa nghiệm thu | Không suy ra từ PCM hoặc state test |
| Chơi thắng tự nhiên, cân bằng | Bị chặn MVP | Thiếu placement/HUD/phân loại tháp và quái |

## Chi tiết kiểm tra Play Mode

1. Khởi tạo AudioManager và GameManager.
2. Tải đủ 12 AudioClip từ Resources.
3. Menu có đúng trạng thái nhạc.
4. Runtime canvas có AudioSettingsUI.
5. Panel có hai slider.
6. PointerEventData trên thanh Music ánh xạ đúng vị trí sang volume.
7. AxisEventData hướng phải làm tăng giá trị slider.
8. Hai slider điều chỉnh riêng BGM/SFX.
9. Clamp volume về 0–1.
10. Ghi volume vào PlayerPrefs.
11. Xóa AudioManager trùng sau một frame.
12. Chuyển state Preparation.
13. Bắt đầu wave chuyển state Battle.
14. Spawn Enemy trên waypoint.
15. Diệt quái cuối của wave test gây Win và timeScale=0.
16. Win chuyển state Result.
17. Về menu xóa trạng thái kết quả, phục hồi timeScale=1 và nhạc Menu.
18. Lives=0 gây Lose và state Result.
19. Chuyển Level_2 giữ một AudioManager.
20. Giữ volume qua chuyển scene.
21. Level_3 khởi tạo manager và bảng audio.
22. Level_3 có bốn đường waypoint.

Các bước 15 và 18 là kiểm thử điều kiện kết quả bằng tác động có kiểm soát, không phải bằng chứng người chơi có thể thắng/thua qua một lượt đầy đủ. Phép thử pointer/axis gọi event handler Unity; không phải thao tác thiết bị chuột/bàn phím native. Kiểm tra volume xác minh PlayerPrefs và giữ qua scene, chưa kiểm thử mở lại process với cấu hình đã đổi.

Script PowerShell chạy lại QA đã được kiểm tra cú pháp bằng PowerShell AST parser. Phần smoke ghi lại hash executable, resources.assets và Assembly-CSharp.dll để nhận diện dữ liệu build, vì hash file exe của Unity riêng lẻ không đủ phân biệt thay đổi script/audio. Audit audio thông thường kiểm tra hash so với provenance đã ghi; chỉ lệnh sinh lại có chủ đích mới ghi nhận bộ nguồn mới.

## Kiểm tra hình ảnh

Ảnh tạo từ scene thực trong Unity, tạm đổi canvas sang ScreenSpaceCamera và render vào RenderTexture bằng URP, rồi khôi phục canvas. Đã xem cả ba ảnh: panel nằm trong màn hình, nhãn và giá trị không cắt, TEST SOUND/CLOSE rõ, nút PLAY vẫn có chỗ thao tác. Khi mở panel, một phần logo/nền bị che như một bảng nổi. Đây là kiểm tra render, không thay thế desktop screenshot hoặc cảm giác thao tác.

## Cảnh báo build còn lại

- Unity Services: project chưa được liên kết project ID trên dashboard. Không thiết lập tài khoản hoặc kết nối dịch vụ trong phiên này.
- `System.Windows.Forms.dll` được cảnh báo không được hỗ trợ trong platform build hiện tại. Chưa khoanh vùng assembly/package gây tham chiếu. Player smoke đạt nhưng không chứng minh mọi chức năng package đều an toàn.

Lượt build trước còn có cảnh báo pragma debug deprecated của TextMeshPro shader. Lượt cuối dùng shader cache và báo tổng 2 cảnh báo như JSON. Không ghi rằng cảnh báo shader đã được sửa.

Đây là các cảnh báo được giữ trong báo cáo, không ghi build là sạch warning. Không tìm thấy lỗi compile ở script Sound mới trong lượt cuối.

## Các lượt chạy và giới hạn

Lượt runner đầu gặp Editor hot reload sau khi vào Play Mode, làm mất state UI và gây NullReference tại UIManager.Show. Runner được sửa để vào Play Mode với domain reload sạch, lưu và khôi phục cả cờ bật và giá trị tùy chọn Play Mode của người dùng. Lượt cuối đạt 22 assertion và build thành công. Khả năng game tự khôi phục sau hot reload chưa được sửa; KG015 vẫn mở.

Player smoke mở lần lượt ba scene, kiểm tra manager/UI/clip và tự thoát. Không sửa tiến độ mở khóa màn. Runner Play Mode khôi phục các key volume, pending scene, unlock và completion mà nó thay đổi. Scene dùng trong test không được lưu với cấu hình wave rút gọn.

Các chỉnh sửa shader prefilter và UnityConnect do Unity tự ghi trong build được dọn khỏi working tree sau kiểm tra; các thay đổi có sẵn của người dùng về Build Settings và tutorial settings được giữ lại.
