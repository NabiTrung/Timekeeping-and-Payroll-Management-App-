# Task 13 — Tích hợp Giao tiếp TCP/JSON tại Client (`net10.0-windows`)

## 1. Danh sách file xuất ra
- `Models/TcpPackets.cs` — `RequestPacket`, `ResponsePacket`, hằng số `TcpActions`, `TcpErrorCodes`, và các DTO cho từng action.
- `Services/TcpClientHelper.cs` — gửi/nhận JSON theo từng dòng (`\n`), mặc định `127.0.0.1:8888`, timeout mặc định `8000ms` (có thể cấu hình qua `TcpClientHelper.Configure(host, port, timeoutMs)`), kiểm tra đối chiếu `RequestId`.
- `Form1.cs` (`namespace WinFormsApp1`) — cập nhật `btnKiemTra_Click` (`PING`) và `btnDangNhap_Click` (`LOGIN`).
- `FormChamCong.cs` (`namespace QuanLyChamCong`) — cập nhật `btnCheckIn_Click` (`CHECKIN`), `btnCheckOut_Click` (`CHECKOUT`), và `btnDangXuat_Click`.
- `FormLichSu.cs` (`namespace QuanLyChamCong`) — cập nhật `btnTaiLichSu_Click_1` / `btnTaiLichSu_Click` (`GET_HISTORY`).
- `FormDangKy.cs` (`namespace QuanLyChamCong`) — cập nhật `btnDangKy_Click` (`REGISTER`).

---

## 2. Quy ước giao thức TCP/JSON theo dòng (`\n`)

### Lưu ý kỹ thuật quan trọng cho Client & Server (Task 11)
1. **Framing theo từng dòng (`\n`)**: Mọi `RequestPacket` và `ResponsePacket` đều được serialize **trên đúng 1 dòng** (`WriteIndented = false`) và kết thúc bằng `\n`.
2. **UTF-8 không BOM**: Dùng `new UTF8Encoding(false)` khi tạo `StreamWriter` để tránh ký tự BOM (`0xEF 0xBB 0xBF`) ở đầu dòng làm lỗi bộ parse JSON.
3. **Đối chiếu `RequestId`**: Server (Task 11) phải gán `response.RequestId = request.RequestId` khi trả về; `TcpClientHelper` sẽ từ chối phản hồi nếu `RequestId` không khớp (`ERR_REQUEST_ID_MISMATCH`).
4. **Action lịch sử**: Chuẩn hóa thành `"GET_HISTORY"` (trước đây giao thức cũ dùng `"HISTORY"`).

---

## 3. JSON mẫu cho từng Action

### 3.1. `PING`
**Request:**
```json
{"RequestId":"a1b2c3d4e5f64789901234567890abcd","Action":"PING"}
```
**Response thành công:**
```json
{"RequestId":"a1b2c3d4e5f64789901234567890abcd","Success":true,"ErrorCode":"","Message":"Server dang hoat dong"}
```

### 3.2. `LOGIN`
**Request:**
```json
{"RequestId":"b2c3d4e5f6a14789901234567890abcd","Action":"LOGIN","MaNV":"NV001","Data":{"MatKhau":"123456"}}
```
**Response thành công:**
```json
{"RequestId":"b2c3d4e5f6a14789901234567890abcd","Success":true,"ErrorCode":"","Message":"Dang nhap thanh cong","Data":{"MaNV":"NV001","Ten":"Nguyen Van A","ChucVu":"NhanVien"}}
```
*(Client hỗ trợ đọc cả trường `Ten` lẫn `TenNV` trong `Data`.)*

**Response thất bại:**
```json
{"RequestId":"b2c3d4e5f6a14789901234567890abcd","Success":false,"ErrorCode":"AUTH_INVALID_CREDENTIALS","Message":"Sai ma nhan vien hoac mat khau"}
```

### 3.3. `REGISTER`
**Request:**
```json
{"RequestId":"c3d4e5f6a1b24789901234567890abcd","Action":"REGISTER","MaNV":"NV003","Data":{"Ten":"Tran Van C","MatKhau":"123456"}}
```
**Response thành công:**
```json
{"RequestId":"c3d4e5f6a1b24789901234567890abcd","Success":true,"ErrorCode":"","Message":"Dang ky thanh cong"}
```

### 3.4. `CHECKIN`
**Request (không kèm ảnh — Task 13 cơ bản):**
```json
{"RequestId":"d4e5f6a1b2c34789901234567890abcd","Action":"CHECKIN","MaNV":"NV001"}
```
**Request (kèm ảnh Camera — Task 10):**
```json
{"RequestId":"d4e5f6a1b2c34789901234567890abcd","Action":"CHECKIN","MaNV":"NV001","Data":{"ImagePath":"Images/CheckIn/NV001_20261123_080512.jpg","ImageBase64":"<base64_jpeg>"}}
```
**Response thành công:**
```json
{"RequestId":"d4e5f6a1b2c34789901234567890abcd","Success":true,"ErrorCode":"","Message":"Cham cong vao thanh cong","Data":{"GioVao":"08:05:12","TrangThai":"DiMuon","AnhCheckIn":"Images/CheckIn/NV001_20261123_080512.jpg"}}
```
**Response thất bại:**
```json
{"RequestId":"d4e5f6a1b2c34789901234567890abcd","Success":false,"ErrorCode":"CHECKIN_ALREADY_EXISTS","Message":"Nhan vien da cham cong hom nay"}
```

### 3.5. `CHECKOUT`
**Request:**
```json
{"RequestId":"e5f6a1b2c3d44789901234567890abcd","Action":"CHECKOUT","MaNV":"NV001"}
```
**Response thành công:**
```json
{"RequestId":"e5f6a1b2c3d44789901234567890abcd","Success":true,"ErrorCode":"","Message":"Cham cong ra thanh cong","Data":{"GioRa":"17:30:00","TrangThai":"DungGio"}}
```

### 3.6. `GET_HISTORY`
**Request:**
```json
{"RequestId":"f6a1b2c3d4e54789901234567890abcd","Action":"GET_HISTORY","MaNV":"NV001"}
```
**Response thành công (mảng trực tiếp trong `Data`):**
```json
{"RequestId":"f6a1b2c3d4e54789901234567890abcd","Success":true,"ErrorCode":"","Message":"Tai lich su thanh cong","Data":[{"NgayChamCong":"2026-11-23","GioVao":"08:05:12","GioRa":"17:30:00","IP":"127.0.0.1","TrangThai":"DiMuon","AnhCheckIn":"Images/CheckIn/NV001_20261123_080512.jpg"}]}
```
*(Client hỗ trợ cả trường hợp `Data` là JSON array `[...]` hoặc object bọc có key `Records`, `Items`, hoặc `History`.)*

---

## 4. Danh sách mã lỗi phía Client (`TcpErrorCodes`)
| ErrorCode | Ý nghĩa |
|---|---|
| `ERR_CONNECTION_FAILED` | Không kết nối được tới `ServerHost:ServerPort` (`SocketException`). |
| `ERR_TIMEOUT` | Quá thời gian chờ `TimeoutMilliseconds` (mặc định 8 giây). |
| `ERR_CANCELLED` | Tác vụ bị hủy qua `CancellationToken`. |
| `ERR_SERVER_DISCONNECTED` | Server ngắt kết nối giữa chừng hoặc trả chuỗi rỗng. |
| `ERR_INVALID_RESPONSE` | Không deserialize được JSON phản hồi từ Server. |
| `ERR_REQUEST_ID_MISMATCH` | `RequestId` trả về không trùng với `RequestId` đã gửi. |
| `ERR_CLIENT_ERROR` | Ngoại lệ khác tại lớp giao tiếp Client. |

Khi hiển thị lỗi trên UI, các Form gọi `response.FormatErrorDisplay(...)` để hiện dạng `[{ErrorCode}] {Message}`.

---

## 5. Hướng dẫn tích hợp vào dự án `WinFormsApp1`
1. Copy thư mục `Models/` và `Services/` vào thư mục project `WinFormsApp1/`.
2. Copy đè các file `Form1.cs`, `FormChamCong.cs`, `FormLichSu.cs`, `FormDangKy.cs` vào `WinFormsApp1/`.
3. **KHÔNG** xóa hoặc sửa các file `*.Designer.cs` và `*.resx` gốc của `Form1`, `FormDangKy`, `FormLichSu`:
   - `Form1` giữ nguyên namespace `WinFormsApp1`.
   - `FormChamCong`, `FormDangKy`, `FormLichSu` giữ nguyên namespace `QuanLyChamCong`.
   - `FormLichSu.cs` giữ nguyên cả `btnTaiLichSu_Click_1` và `btnDong_Click_1` để khớp `FormLichSu.Designer.cs`.
   - `FormDangKy.cs` giữ nguyên `lblTieuDe_Click` để khớp `FormDangKy.Designer.cs`.
4. Nếu có cấu hình IP/Port động (ví dụ màn hình cài đặt hoặc `FormAdmin`), gọi:
   ```csharp
   TcpClientHelper.Configure("127.0.0.1", 8888, 8000);
   ```
5. Nếu ghép chung với **Task 10 (Camera Check-In)**, thực hiện theo thứ tự:
   - Copy **Task 13** trước, sau đó copy **Task 10** đè lên `FormChamCong.cs`, `FormChamCong.Designer.cs` và thêm `Services/CameraHelper.cs`.
6. **Ghi chú về Form Admin**: Dự án gốc hiện chưa có Form Admin (`FormAdmin.cs`). Khi thành viên phụ trách thêm Form Admin, chỉ cần gọi `await TcpClientHelper.SendRequestAsync("ACTION_NAME", maNV, payload)` và hiển thị `response.FormatErrorDisplay()` tương tự các Form trên.
