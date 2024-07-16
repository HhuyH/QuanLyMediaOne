
IF EXISTS (SELECT NAME FROM SYS.DATABASES WHERE NAME='QuanLyMediaOne')
	DROP DATABASE QuanLyMediaOne
GO

IF EXISTS (SELECT NAME FROM SYS.DATABASES WHERE NAME='QuanLyMediaOne')
	ALTER DATABASE QuanLyMediaOne SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

CREATE DATABASE QuanLyMediaOne
	ON(NAME='QuanLyMediaOne_DATA',FILENAME='D:\Code\SQL\QuanLyMediaOne.MDF')
	LOG ON(NAME='QuanLyMediaOne_LOG',FILENAME='D:\Code\SQL\QuanLyMediaOne.LDF')
GO

USE QuanLyMediaOne
GO

Create table [NGUOIDUNG] (
	[MAND] Integer Identity NOT NULL UNIQUE,
	[TAIKHOAN] Varchar(50) NOT NULL,
	[MATKHAU] Varchar(50) NULL,
	[VAITRO] Varchar(50) NULL,
Primary Key  ([MAND])
) 
go

Create table [NHANVIEN] (
	[MANV] Varchar(50) NOT NULL UNIQUE,
	[MAND] Char(10) NOT NULL,
	[HO] Nvarchar(20) NULL,
	[TEN] Nvarchar(20) NULL,
	[EMAIL] Varchar(50) NULL,
	[SDT] Integer NULL,
	[GIOITINH] Nvarchar(5),
	[DIACHI] Nvarchar(500) NULL,
	[LUONG] Numeric(10,0) NULL,
Primary Key  ([MANV])
) 
go

Create table [KHACHHANG] (
	[MAKH] Varchar(50) NOT NULL UNIQUE,
	[MAND] Char(10) NOT NULL,
	[HO] Nvarchar(20) NULL,
	[TEN] Nvarchar(20) NULL,
	[EMAIL] Varchar(50) NULL,
	[GIOITINH] Nvarchar(3),
	[SDT] Integer NULL,
	[DIACHI] Nvarchar(500) NULL,
Primary Key  ([MAKH])
) 
go

ALTER TABLE KHACHHANG
ALTER COLUMN SDT Integer NULL;
GO

Create table [DANHMUCSANPHAM] (
	[MADANHMUC] Integer Identity NOT NULL UNIQUE,
	[TENDANHMUC] Nvarchar(50) NULL,
	[MOTA] Nvarchar(200) NULL,
Primary Key  ([MADANHMUC])
) 
go

Create table [SANPHAM] (
	[MASANPHAM] Integer Identity NOT NULL UNIQUE,
	[TENSANPHAM] Nvarchar(50) NOT NULL,
	[GIA] Numeric(10,0) NULL,
	[SOLUONG] Integer NULL,
	[MADANHMUC] Integer NOT NULL,
Primary Key  ([MASANPHAM])
) 
go

Create table [DONHANG] (
	[MADONHANG] Integer Identity NOT NULL UNIQUE,
	[MAKH] Varchar(50),
	[NGAYLAPDONHAN] Datetime Default GETDATE() NULL Constraint [CK_NGAYLAP] Check (NGAYLAPDONHAN>=GETDATE()),
	[TONGGIATIEN] Numeric(10,0) NULL,
	[NGUOINHAN] NVARCHAR(255),
	[DIACHIGIAO] VARCHAR(255),
	[TRANGTHAIHUY] NVARCHAR(50) DEFAULT N'Chưa hủy',
	[TRANGTHAITHANHTOAN] NVARCHAR(50) DEFAULT N'Chưa thanh toán',
	[TRANGTHAIXACNHAN] NVARCHAR(50) DEFAULT N'Chưa xác nhận'
Primary Key  ([MADONHANG])
) 
go

Create table [CHITIETDONHAN] (
	[MADONHANG] Integer NOT NULL,
	[MASANPHAM] Integer NOT NULL,
	[SOLUONG] Integer NULL,
	[GIATIEN] Numeric(10,0) NULL,
	[GIAMGIA] Numeric(10,0) NULL,
Primary Key  ([MADONHANG],[MASANPHAM])
) 
go

Create table [GIOHANG] (
	[MAGH] Integer Identity NOT NULL UNIQUE,
	[MASANPHAM] Integer NOT NULL,
	[MAKH] Varchar(50) NOT NULL,
	[SOLUONG] Integer NULL,
	[GIATIEN] Numeric(10,0) NULL,
Primary Key  ([MASANPHAM],[MAKH])
) 
go


ALTER TABLE DONHANG ADD foreign key([MAKH]) references [KHACHHANG] ([MAKH]) 
Alter table [GIOHANG] add  foreign key([MAKH]) references [KHACHHANG] ([MAKH]) 
go
Alter table [SANPHAM] add  foreign key([MADANHMUC]) references [DANHMUCSANPHAM] ([MADANHMUC]) 
go
Alter table [CHITIETDONHAN] add  foreign key([MASANPHAM]) references [SANPHAM] ([MASANPHAM]) 
go
Alter table [GIOHANG] add  foreign key([MASANPHAM]) references [SANPHAM] ([MASANPHAM]) 
go
Alter table [CHITIETDONHAN] add  foreign key([MADONHANG]) references [DONHANG] ([MADONHANG]) 
go
Alter table [KHACHHANG] add  foreign key([MAND]) references [NGUOIDUNG] ([MAND]) 
go
Alter table [NHANVIEN] add  foreign key([MAND]) references [NGUOIDUNG] ([MAND]) 
go


Set quoted_identifier on
go

Set quoted_identifier off
go

INSERT INTO [DANHMUCSANPHAM] ([TENDANHMUC], [MOTA]) VALUES
(N'Đĩa nhạc', N'Mô tả'),
(N'Đĩa phim', N'Mô tả'),
(N'Sách', N'Mô tả');
GO

-- Giả sử các mã danh mục (MADANHMUC) là 1, 2, 3 tương ứng với các danh mục đã chèn ở trên.
INSERT INTO [SANPHAM] ([TENSANPHAM], [GIA], [SOLUONG], [MADANHMUC]) VALUES
(N'Album nhạc A', 100000, 50, 1),
(N'Album nhạc B', 150000, 30, 1),
(N'Album nhạc C', 120000, 20, 1),
(N'Phim hành động ', 200000, 40, 2),
(N'Phim kinh dị ', 250000, 25, 2),
(N'Phim hoạt hình ', 180000, 35, 2),
(N'Tiểu thuyết ', 50000, 100, 3),
(N'Sách giáo khoa ', 70000, 60, 3),
(N'Tập truyện ngắn ', 80000, 80, 3);
GO


