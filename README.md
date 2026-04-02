# Trò Chơi Xếp Khối (Block Puzzle) - Phiên Bản Nâng Cấp

## 🌟 Mô tả
Đây là dự án trò chơi giải đố xếp khối (Block Puzzle) được phát triển bài bản bằng Unity. Game kết hợp giữa lối chơi cổ điển và những tính năng hiện đại, mang lại trải nghiệm mượt mà và đầy thử thách.

## 🚀 Tính năng nổi bật mới

### 📥 Hệ thống Cất Khối (Hold Block)
- **Đổi khối linh hoạt**: Cho phép người chơi cất một khối đang cầm vào ô **HOLD** để trao đổi lấy khối khác.
- **Không giới hạn lượt**: Bạn có thể đổi qua lại liên tục giữa các slot chính và ô Hold trong cùng một lượt chơi để tìm ra phương án tối ưu nhất.
- **Tiện lợi**: Khối trong ô Hold có thể được kéo trực tiếp ra bàn cờ bất cứ lúc nào.

### 🛡️ Chiến thuật "Cứu thua" (Hold to Rescue)
- **Hồi sinh đợt khối**: Nếu bạn bị kẹt (không thể đặt khối nào vào bàn), bạn có thể cất một khối vào ô Hold (nếu Hold đang trống). Hành động này sẽ làm trống các slot chính và kích hoạt việc sinh ra 3 khối mới, giúp bạn tiếp tục hành trình ghi điểm.

### 👁️ Nhận diện thông minh (Visual Feedback)
- **Tự động mờ khối**: Hệ thống sẽ tự động tính toán và làm mờ (Fade out) những khối không còn chỗ đặt trên bàn chơi. Giúp bạn tập trung vào những nước đi khả thi.
- **Cập nhật tức thời**: Khối sẽ sáng trở lại ngay lập tức nếu bạn vừa xóa hàng và tạo ra chỗ trống mới cho nó.

### 🎮 Điều khiển siêu nhạy (Precise Controls)
- **Nhấp đâu cũng trúng**: Cải tiến thao tác kéo thả, cho phép bạn nhấp chuột/chạm vào bất kỳ ô vuông nào trong khối để bắt đầu kéo, thay vì phải nhấn đúng tâm như các phiên bản cũ.
- **Bảo vệ Menu**: Hệ thống tự động đóng băng các khối khi bạn đang mở bảng Cài đặt hoặc Game Over, tránh các thao tác nhầm lẫn.

### 📺 Hệ thống Quảng cáo & Hồi sinh
- **Xóa hàng cứu trợ**: Xem quảng cáo để xóa 2 hàng trung tâm sau khi thua, giúp giải phóng không gian bàn cờ và tiếp tục phá kỷ lục điểm số.

## 🕹️ Cách chơi
1. Kéo các khối từ 3 khay chứa bên dưới (hoặc từ ô **HOLD**) vào bảng.
2. Xếp các khối để tạo thành một hàng ngang hoặc dọc đầy đủ để ghi điểm.
3. Sử dụng ô **HOLD** (bên phải) để cất giữ khối quan trọng hoặc xoay vòng đợt khối mới.
4. Trò chơi kết thúc khi **tất cả** các khối hiện có (bao gồm cả khối trong Hold) đều không còn chỗ để đặt vào bàn.

## 🛠️ Yêu cầu kỹ thuật
- **Unity Version**: Unity 2022.3 LTS hoặc mới hơn.
- **Input System**: Sử dụng New Input System Package cho độ phản hồi tốt nhất.
- **Hệ điều hành**: Windows, macOS, hoặc Linux.
