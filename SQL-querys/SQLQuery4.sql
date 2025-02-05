DROP PROCEDURE IF EXISTS dbo.AddCar;
GO

CREATE PROCEDURE dbo.AddCar
    @UserId INT,
    @LicensePlate NVARCHAR(50),
    @Model NVARCHAR(100)
AS
BEGIN
    INSERT INTO dbo.Cars (UserID, LicensePlate, Model) 
    VALUES (@UserId, @LicensePlate, @Model);
END;