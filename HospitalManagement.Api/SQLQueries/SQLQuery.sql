-- SELECT '✅ HospitalDb is ready!' AS Message;

-- SELECT * FROM dbo.Users;

-- Databases → HospitalDb → Tables → dbo.Users

-- SELECT TOP (1000) [Id], [FullName], [Email], [PasswordHash], [Role]
-- FROM [HospitalDb].[dbo].[Users];

-- SELECT * FROM dbo.Users WHERE Email = 'dr.smith@example.com';

-- SELECT * FROM dbo.Users
-- WHERE Email = 'dr.smith@example.com';

--DELETE FROM dbo.Users
-- WHERE Email = 'dr.smith@example.com';

/* INSERT INTO Users (FullName, Email, PasswordHash, Role)
VALUES (
  'Admin User',
  'admin@hospital.com',
  '$2a$11$sFQW2Z2XeVRcnBa9ncf1FupxRWmbqxq7xEoO7sIEl53DGuQ6TkKke', -- BCrypt hash of Admin@secure!2025
  'Admin'
)
*/


SELECT * FROM dbo.Users;

--DELETE FROM dbo.Users WHERE Email = 'admin@hospital.com';



