# Đồng bộ main ngày 2 tháng 11 năm 2026

Đã lấy origin/main tại commit e93506c và tích hợp vào nhánh van-khai trên nền checkpoint 4d714af. Giữ lịch sử của cả hai nhánh, không reset hoặc ghi đè phần Sound/QA/Lead.

Các xung đột ở GameManager, Enemy, Bullet, Tower, UIManager và EditorBuildSettings được giải quyết theo gameplay/UI mới của main rồi nối lại âm thanh: khởi tạo manager, nhạc chuẩn bị/chiến đấu, âm lâu đài mất máu, quái chết, đạn chạm mục tiêu, cung/pháo và Win/Lose. Bảng SOUND dùng canvas riêng để không phụ thuộc cấu trúc HUD cũ; menu mới cũng khởi tạo bảng âm thanh và tiếng click.

Build Settings giữ MainMenu và ba Level của main. API cấu hình wave trong runner QA được đổi sang WaveDefinition để tiếp tục biên dịch với code mới. Metadata cho thư mục Editor được tạo lại vì main đã chuyển metadata cũ sang thư mục khác.

Kiểm tra trong phiên đồng bộ: Unity 6000.6.0f1 biên dịch C# thành công; kiểm tra 12 clip PCM/provenance đạt; kiểm tra scene/GUID và metadata đạt. Log biên dịch ở Logs/MergeMainCompile.log. Không chạy lại toàn bộ Play Mode, build Windows hoặc duyệt UI trong phiên kéo code này.

Các báo cáo ngày 26/10 và executable Builds/QA phản ánh checkpoint cũ, không phải bằng chứng nghiệm thu bản main mới. Suite QA cũ cần cập nhật kỳ vọng cho MainMenu riêng, bước chuẩn bị wave đầu, Boss tự thêm ở wave cuối và số scene tăng từ ba lên bốn trước khi nghiệm thu lại toàn bộ.

GDD và thay đổi tutorial settings chưa commit được giữ riêng. Không push remote trong phiên này.
