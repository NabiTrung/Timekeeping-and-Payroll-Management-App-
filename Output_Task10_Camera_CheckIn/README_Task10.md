# Task 10 — Tích hợp Camera khi Check-In (`net10.0-windows`)

## 1. Lựa chọn thư viện Camera cho `.NET 10` WinForms
Khuyến nghị dùng **`OpenCvSharp4.Windows`** + **`OpenCvSharp4.Extensions`** (phiên bản `4.13.0.20261107`):
- Hỗ trợ native cho `.NET 8 / .NET 9 / .NET 10` (`net10.0-windows`), không bị cảnh báo tương thích `NU1701` hay lỗi interop COM 32-bit/64-bit như `AForge.Video.DirectShow` (`2.2.5`, ngừng cập nhật từ 2013).
- Đóng gói sẵn native DLL cho Windows và hỗ trợ DirectShow (`VideoCaptureAPIs.DSHOW`) + chuyển `Mat` sang `System.Drawing.Bitmap` trực tiếp qua `BitmapConverter.ToBitmap`.

---

## 2. Danh sách file xuất ra
- `QuanLyChamCong_PackageReference.xml` — cấu hình NuGet cần thêm vào `WinFormsApp1\QuanLyChamCong.csproj`.
- `Services/CameraHelper.cs` — quét camera, hiển thị preview lên `PictureBox`, chụp ảnh `.jpg` + chuyển Base64, và giải phóng camera an toàn.
- `FormChamCong.Designer.cs` — thêm `lblCamera`, `cboCamera`, `btnBatTatCamera`, `picCamera`, `lblTrangThaiCamera` và sự kiện `FormClosing` (giữ nguyên toàn bộ control cũ).
- `FormChamCong.cs` — kế thừa giao tiếp JSON của Task 13 (`TcpClientHelper`, `TcpActions`, `CheckInRequestData`), chụp ảnh khi bấm `btnCheckIn` và gửi kèm `ImagePath` + `ImageBase64`.
- `Update_ChamCong_Camera.sql` — script SQL thêm cột `AnhCheckIn NVARCHAR(255) NULL` vào bảng `ChamCong`.
- `Server_Reference_Snippet.cs` — đoạn mã mẫu hướng dẫn cập nhật `DatabaseHelper.cs` và `ServerManager.cs` phía Server.

---

## 3. Hướng dẫn cài NuGet & Tích hợp

### Bước 1: Cài đặt thư viện Camera vào `WinFormsApp1\QuanLyChamCong.csproj`
Chạy lệnh trong thư mục `WinFormsApp1\`:
```powershell
dotnet add package OpenCvSharp4.Windows --version 4.13.0.20261107
dotnet add package OpenCvSharp4.Extensions --version 4.13.0.20261107
```
Hoặc thêm trực tiếp 2 dòng `<PackageReference>` trong `QuanLyChamCong_PackageReference.xml` vào `<ItemGroup>` của `QuanLyChamCong.csproj` (giữ nguyên `Microsoft.Data.SqlClient` `7.1.1`).

### Bước 2: Chạy SQL cập nhật bảng `ChamCong`
Mở SQL Server Management Studio (SSMS) và thực thi file `Update_ChamCong_Camera.sql` trên database `QuanLyChamCong` để thêm cột `AnhCheckIn NVARCHAR(255) NULL`.

### Bước 3: Ghép file code Client (theo đúng thứ tự để không xung đột Task 13 & Task 10)
1. Nếu chưa ghép **Task 13**, copy toàn bộ `Output_Task13_TCP_JSON_Client\` vào `WinFormsApp1\` trước (bắt buộc phải có `Models\TcpPackets.cs` và `Services\TcpClientHelper.cs`).
2. Copy từ `Output_Task10_Camera_CheckIn\` vào `WinFormsApp1\`:
   - `Services\CameraHelper.cs` -> `WinFormsApp1\Services\CameraHelper.cs`
   - `FormChamCong.cs` -> ghi đè `WinFormsApp1\FormChamCong.cs`
   - `FormChamCong.Designer.cs` -> ghi đè `WinFormsApp1\FormChamCong.Designer.cs`
   *(Giữ nguyên `FormChamCong.resx` gốc của dự án.)*

### Bước 4: Cập nhật Server (Task 11 / Task 12)
Tham khảo `Server_Reference_Snippet.cs` để:
- Cập nhật `DatabaseHelper.CheckIn(string maNV, string ip, string? anhCheckIn = null)` và `LayLichSuChamCong(string maNV)` lưu/đọc cột `AnhCheckIn`.
- Cập nhật `ServerManager.cs` nhận `ImagePath` + `ImageBase64` trong JSON `CHECKIN`, lưu ảnh phía Server và truyền đường dẫn xuống `DatabaseHelper.CheckIn`.

---

## 4. Điểm nhấn an toàn trong `CameraHelper.cs`
- Dùng `BeginInvoke` kết hợp cờ `Interlocked` khi cập nhật `picCamera.Image` -> tránh **deadlock** khi `FormClosing` gọi `Stop()` và tránh rò rỉ bộ nhớ `Bitmap`.
- Giải phóng `VideoCapture` và `Mat` trong `finally` + `FormChamCong_FormClosing` -> không treo tiến trình sau khi đóng Form.
- Ảnh chụp lưu ở `Images/CheckIn/{MaNV}_{yyyyMMdd_HHmmss}.jpg` (trong thư mục chạy ứng dụng `bin\...\Images\CheckIn\`), chất lượng JPEG `80` giúp payload `ImageBase64` gọn nhẹ, gửi nhanh qua TCP theo 1 dòng JSON.

---

## 5. Checklist kiểm thử luồng Camera -> Check-In
1. **Máy không có webcam**: Mở `FormChamCong` không bị crash; `lblTrangThaiCamera` báo không tìm thấy webcam; bấm `btnCheckIn` hiện hộp thoại cảnh báo và cho phép chọn tiếp tục hoặc hủy.
2. **Camera bị ứng dụng khác chiếm dụng**: Hiển thị thông báo lỗi rõ ràng trên UI thay vì treo app.
3. **Có webcam hoạt động**: `picCamera` hiển thị preview mượt; chọn camera khác trong `cboCamera` đổi được nguồn video; bấm `btnCheckIn` tạo được file `.jpg` đúng định dạng `{MaNV}_{yyyyMMdd_HHmmss}.jpg` và gửi JSON `CHECKIN` chứa `ImagePath` + `ImageBase64` lên Server.
4. **Đóng Form / Đăng xuất**: Đèn webcam tắt ngay lập tức, ứng dụng thoát sạch không còn tiến trình ngầm chạy trong Task Manager.
