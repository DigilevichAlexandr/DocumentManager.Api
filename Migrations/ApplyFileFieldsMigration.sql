-- Миграция для добавления полей файлов в таблицу Documents
-- Выполните этот скрипт в SQL Server Management Studio или через sqlcmd

USE DocumentManagerDb;
GO

-- Проверяем, существуют ли колонки перед добавлением
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Documents') AND name = 'ContentType')
BEGIN
    ALTER TABLE Documents ADD ContentType NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Documents') AND name = 'FileName')
BEGIN
    ALTER TABLE Documents ADD FileName NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Documents') AND name = 'FilePath')
BEGIN
    ALTER TABLE Documents ADD FilePath NVARCHAR(1000) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Documents') AND name = 'FileSize')
BEGIN
    ALTER TABLE Documents ADD FileSize BIGINT NULL;
END
GO

-- Обновляем ограничения для существующих колонок
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Documents') AND name = 'Name' AND max_length = -1)
BEGIN
    ALTER TABLE Documents ALTER COLUMN Name NVARCHAR(500) NOT NULL;
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Documents') AND name = 'Description' AND max_length = -1)
BEGIN
    ALTER TABLE Documents ALTER COLUMN Description NVARCHAR(2000) NULL;
END
GO

PRINT 'Миграция успешно применена!';
GO
