
USE QuanLyChamCong;
GO



IF OBJECT_ID('dbo.CaLam', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CaLam
    (
        MaCa VARCHAR(20) PRIMARY KEY,
        TenCa NVARCHAR(50) NOT NULL,
        GioBatDau TIME NOT NULL,
        GioKetThuc TIME NOT NULL,

        CONSTRAINT CK_CaLam_Gio
            CHECK (GioKetThuc > GioBatDau)
    );
END;
GO



IF NOT EXISTS (
    SELECT 1
    FROM dbo.CaLam
    WHERE MaCa = 'HC'
)
BEGIN
    INSERT INTO dbo.CaLam
        (MaCa, TenCa, GioBatDau, GioKetThuc)
    VALUES
        ('HC', N'Ca hành chính', '08:00', '17:00');
END;
GO



IF COL_LENGTH('dbo.ChamCong', 'MaCa') IS NULL
BEGIN
    ALTER TABLE dbo.ChamCong
    ADD MaCa VARCHAR(20) NULL;
END;
GO

/* =============================================
   4. CAP NHAT DU LIEU CHAM CONG CU
   ============================================= */

IF EXISTS (
    SELECT 1
    FROM dbo.ChamCong
    WHERE MaCa IS NULL
)
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.CaLam
        WHERE MaCa = 'HC'
    )
    BEGIN
        THROW 50001,
            N'Không tìm thấy ca HC trong bảng CaLam.',
            1;
    END;

    UPDATE dbo.ChamCong
    SET MaCa = 'HC'
    WHERE MaCa IS NULL;
END;
GO



IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ChamCong')
      AND name = 'MaCa'
      AND is_nullable = 1
)
BEGIN
    ALTER TABLE dbo.ChamCong
    ALTER COLUMN MaCa VARCHAR(20) NOT NULL;
END;
GO


IF EXISTS (
    SELECT 1
    FROM dbo.ChamCong cc
    LEFT JOIN dbo.CaLam cl
        ON cc.MaCa = cl.MaCa
    WHERE cl.MaCa IS NULL
)
BEGIN
    THROW 50002,
        N'Có MaCa trong ChamCong không tồn tại trong CaLam.',
        1;
END;
GO


IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_ChamCong_CaLam'
      AND parent_object_id = OBJECT_ID('dbo.ChamCong')
)
BEGIN
    ALTER TABLE dbo.ChamCong
    ADD CONSTRAINT FK_ChamCong_CaLam
    FOREIGN KEY (MaCa)
    REFERENCES dbo.CaLam(MaCa);
END;
GO
