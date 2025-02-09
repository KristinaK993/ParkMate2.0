--ALTER TABLE Users ADD IsAdmin BIT DEFAULT 0;

--SELECT COLUMN_NAME 
--FROM INFORMATION_SCHEMA.COLUMNS 
--WHERE TABLE_NAME = 'Users';

INSERT INTO Users (UserName, Email, Password, IsAdmin)
VALUES ('Admin', 'admin@parkmate.com', 'hashedpassword', 1);

