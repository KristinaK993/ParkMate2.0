UPDATE Users
SET IsAdmin = 0
WHERE IsAdmin IS NULL;
