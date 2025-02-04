
---- L�gg till 10 anv�ndare
--INSERT INTO Users (UserName, Email, Password) VALUES
--('Alice Johnson', 'alice@example.com', 'password123'),
--('Bob Smith', 'bob@example.com', 'securepass'),
--('Charlie Brown', 'charlie@example.com', 'charlie123'),
--('David Wilson', 'david@example.com', 'davidpass'),
--('Eva Davis', 'eva@example.com', 'eva1234'),
--('Frank Miller', 'frank@example.com', 'frankpass'),
--('Grace Lee', 'grace@example.com', 'grace2024'),
--('Henry Clark', 'henry@example.com', 'henrypass'),
--('Isla Thompson', 'isla@example.com', 'isla456'),
--('Jack White', 'jack@example.com', 'jack789');

---- L�gg till bilar f�r varje anv�ndare
--INSERT INTO Cars (UserID, LicensePlate, Model) VALUES
--(1, 'ABC123', 'Toyota Corolla'),
--(2, 'XYZ789', 'Honda Civic'),
--(3, 'LMN456', 'Ford Focus'),
--(4, 'JKL321', 'Tesla Model 3'),
--(5, 'PQR654', 'BMW X5'),
--(6, 'STU987', 'Mercedes C-Class'),
--(7, 'VWX258', 'Audi A4'),
--(8, 'YZA753', 'Volvo XC90'),
--(9, 'BCD159', 'Nissan Qashqai'),
--(10, 'EFG357', 'Kia Sportage');

---- L�gg till data i UserCar-tabellen (f�r many-to-many-relation)
--INSERT INTO UserCar (UserID, CarID) VALUES
--(1, 1), (2, 2), (3, 3), (4, 4), (5, 5),
--(6, 6), (7, 7), (8, 8), (9, 9), (10, 10);

-- L�gg till parkeringshistorik
--INSERT INTO Parking (CarID, Timestamp, Duration, PayMethod) VALUES
--(1, GETDATE(), 2.5, 'Credit Card'),
--(2, GETDATE(), 1.0, 'Mobile Payment'),
--(3, GETDATE(), 3.0, 'Cash'),
--(4, GETDATE(), 1.5, 'Credit Card'),
--(5, GETDATE(), 2.0, 'Mobile Payment'),
--(6, GETDATE(), 4.0, 'Debit Card'),
--(7, GETDATE(), 0.5, 'Credit Card'),
--(8, GETDATE(), 3.5, 'Cash'),
--(9, GETDATE(), 2.2, 'Mobile Payment'),
--(10, GETDATE(), 1.8, 'Credit Card');