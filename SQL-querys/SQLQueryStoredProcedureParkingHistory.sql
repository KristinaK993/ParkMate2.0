CREATE PROCEDURE GetParkingHistory
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        U.UserID,
        U.UserName,
        C.CarID,
        C.LicensePlate,
        C.Model,
        P.ParkingID,
        P.Timestamp,
        P.Duration,
        P.PayMethod
    FROM Parking P
    INNER JOIN Cars C ON P.CarID = C.CarID
    INNER JOIN Users U ON C.UserID = U.UserID
    WHERE U.UserID = @UserID
    ORDER BY P.Timestamp DESC;
END;
