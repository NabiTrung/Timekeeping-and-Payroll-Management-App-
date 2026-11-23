-- Đối chiếu với SQLQuery1.sql: thêm cột AnhCheckIn vào bảng ChamCong để lưu đường dẫn ảnh Check-In
USE QuanLyChamCong;
GO

IF COL_LENGTH(N'dbo.ChamCong', N'AnhCheckIn') IS NULL
BEGIN
    ALTER TABLE dbo.ChamCong
    ADD AnhCheckIn NVARCHAR(255) NULL;
END
GO

-- Kiểm tra cấu trúc bảng ChamCong sau khi cập nhật
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = N'dbo'
  AND TABLE_NAME = N'ChamCong'
ORDER BY ORDINAL_POSITION;
GO
