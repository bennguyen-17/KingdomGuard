# Thay âm giao dịch bằng Coin Splash

Cập nhật tiếp theo: phiên thay Preparation đã chạy lại bộ QA/build, bao gồm Coin Splash. Xem PREPARATION_UPDATE_2026-11-02.md; mô tả không chạy được batch dưới đây chỉ áp dụng cho phiên chọn Coin Splash trước đó.

Ngày 02/11/2026, người dùng chọn mẫu số 3, Coin Splash của LordTomorrow trên OpenGameArt. Trang nguồn ghi CC0. Đã tải file gốc, giữ nguồn/hash và cập nhật AUDIO_CREDITS.md.

Clip gốc dài khoảng 0.3308s, mono 48kHz. Bản runtime giữ nguyên tiếng xu rơi, chuyển thành PCM16 mono 44.1kHz, fade nhẹ 1ms đầu/6ms cuối và đưa peak về 0.25. Không cắt mất phần xu lách cách. Thay nội dung SFX_Tower_Place.wav, giữ nguyên metadata/GUID và các hook giao dịch.

Mua, nâng cấp và bán hiện dùng chung cue xác nhận này. Âm chọn trước, âm thiếu Gold, âm cung/pháo/phép và nhạc không đổi.

Đã chạy audit 13 clip: PCM, hash/provenance, loop seam và bound mix đạt. Tái tạo audio bằng --generate-originals không ghi đè Coin Splash và giữ nguyên toàn bộ byte của bộ hiện tại. Đã cập nhật AudioReview.wav để có mẫu mới.

Không chạy lại Play Mode/build trong phiên này: một Unity Editor của người dùng đang mở cùng project, nên lệnh batch không thể mở project để kiểm tra. Không đóng hoặc hủy phiên Editor đó. Kết quả 40 assertion và executable đã có là của lượt trước khi đổi Coin Splash; không ghi chúng thành kết quả kiểm thử mới.

Có thể thử ngay trong Editor sau khi Unity import xong hoặc nghe trực tiếp Assets/Resources/Audio/SFX/SFX_Tower_Place.wav. Khi muốn cập nhật executable, đóng Editor cùng project rồi chạy Tools/run_sound_qa.ps1. Bản exe hiện có chưa nhận clip mới chỉ bằng việc thay file trong Assets.
