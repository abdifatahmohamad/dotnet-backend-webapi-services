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




--DELETE FROM dbo.Users WHERE Email = 'admin@hospital.com';

-- USE [HospitalDb];


-- Delete doctor maria lopez and yonis zarqawi
-- DELETE FROM dbo.Users
-- WHERE Id IN ('aaabbcdc-a263-4d31-a1c7-1d4d47dde125', 'fee9b486-56d1-4dbb-bd73-02d1c02c3f00');

-- UPDATE dbo.Nurses
-- SET AssignedDoctorId = NULL
-- WHERE AssignedDoctorId IN ('aaabbcdc-a263-4d31-a1c7-1d4d47dde125',
--                            'fee9b486-56d1-4dbb-bd73-02d1c02c3f00');

-- SELECT  p.UserId,
--         p.FullName,
--         p.Email,
--         p.AssignedDoctorId
-- FROM dbo.Patients AS p
-- WHERE p.AssignedDoctorId IN (
--     'aaabbcdc-a263-4d31-a1c7-1d4d47dde125',
--     'fee9b486-56d1-4dbb-bd73-02d1c02c3f00'
-- );


-- UPDATE dbo.Patients
-- SET AssignedDoctorId = NULL
-- WHERE AssignedDoctorId IN (
--     'aaabbcdc-a263-4d31-a1c7-1d4d47dde125',
--     'fee9b486-56d1-4dbb-bd73-02d1c02c3f00'
-- );

-- SELECT p.UserId,
--   p.FullName,
--   p.Email,
--   p.AssignedDoctorId
-- FROM dbo.Patients AS p
-- WHERE p.Email IN ('abdi.mohamed@hospital.com', 'layla.yusuf@hospital.com');

-- SELECT n.UserId,
--   n.FullName,
--   n.Email,
--   n.AssignedDoctorId
-- FROM dbo.Nurses AS n
-- WHERE n.AssignedDoctorId IN (
--     'aaabbcdc-a263-4d31-a1c7-1d4d47dde125',
--     'fee9b486-56d1-4dbb-bd73-02d1c02c3f00'
-- );

-- DELETE FROM dbo.Doctors
-- WHERE UserId IN (
--     'aaabbcdc-a263-4d31-a1c7-1d4d47dde125',
--     'fee9b486-56d1-4dbb-bd73-02d1c02c3f00'
-- );

-- DELETE FROM dbo.Users
-- WHERE Id IN (
--     'aaabbcdc-a263-4d31-a1c7-1d4d47dde125',
--     'fee9b486-56d1-4dbb-bd73-02d1c02c3f00'
-- );

-- SELECT Id, FullName, Email, Role
-- FROM dbo.Users
-- WHERE Email LIKE '%yassir.qahdani@hospital.com%';


-- ******************************************

-- Verify patients assigned to Dr. Alice Johnson using her UserId
-- SELECT p.UserId,
--   p.FullName,
--   p.Email,
--   p.AssignedDoctorId
-- FROM dbo.Patients AS p
-- WHERE p.AssignedDoctorId IN (
--     '6bba8d3a-3598-44a5-9054-65d66c6c14f9'
-- );

-- Now, Unassign patients from Dr. Alice Johnson using assignedDoctorId
-- UPDATE dbo.Patients
-- SET AssignedDoctorId = NULL
-- WHERE AssignedDoctorId IN (
--     '6bba8d3a-3598-44a5-9054-65d66c6c14f9'
-- );

-- Verify the un-assignment
-- SELECT p.UserId,
--   p.FullName,
--   p.Email,
--   p.AssignedDoctorId
-- FROM dbo.Patients AS p
-- WHERE p.Email IN ('alice.johnson@hospital.com');


-- Verify nurses assigned to Dr. Alice Johnson using her UserId
-- SELECT n.UserId,
--   n.FullName,
--   n.Email,
--   n.AssignedDoctorId
-- FROM dbo.Nurses AS n
-- WHERE n.AssignedDoctorId IN (
--     '6bba8d3a-3598-44a5-9054-65d66c6c14f9'
-- );

-- Now, Unassign nurses from Dr. Alice Johnson using their userIds
-- UPDATE dbo.Nurses
-- SET AssignedDoctorId = NULL
-- WHERE AssignedDoctorId = '6bba8d3a-3598-44a5-9054-65d66c6c14f9';



-- Verify the nurses un-assignment 
-- SELECT n.UserId,
--   n.FullName,
--   n.Email,
--   n.AssignedDoctorId
-- FROM dbo.Nurses AS n
-- WHERE n.Email IN ('alice.johnson@hospital.com');

-- Now, delete DR. Alice Johnson from Doctors table using her UserId
-- DELETE FROM dbo.Doctors
-- WHERE UserId IN (
--     '6bba8d3a-3598-44a5-9054-65d66c6c14f9'
-- );



-- DELETE FROM dbo.Doctors
-- WHERE UserId IN (
--     '6bba8d3a-3598-44a5-9054-65d66c6c14f9'
-- );

-- DELETE FROM dbo.Doctors
-- WHERE Email IN ('yassir.qahdani@hospital.com');

-- SELECT Id, FullName, Email, Role
-- FROM dbo.Users
-- WHERE Email LIKE '%yassir.qahdani@hospital.com%';

-- SELECT p.UserId,
--   p.FullName,
--   p.Email,
--   p.AssignedDoctorId
-- FROM dbo.Patients AS p
-- WHERE p.AssignedDoctorId = '6bba8d3a-3598-44a5-9054-65d66c6c14f9';





-- ************************************************

USE [HospitalDb];

-- Create not drop tables Users, Doctors, Nurses, Patients



SELECT *
FROM dbo.Users;

-- How to know how many columns that Doctors table has?
SELECT TOP 100
  *
FROM dbo.Patients;

-- update ailment for patient id '6360c08c-a3c7-4a66-9c98-052a5428e0af'
-- UPDATE dbo.Patients
-- SET Ailment = 'Cardiology'
-- WHERE UserId = '6360c08c-a3c7-4a66-9c98-052a5428e0af';

-- Delete all users except admin 11111111-1111-1111-1111-111111111111
-- DELETE FROM dbo.Users
-- WHERE Id <> '11111111-1111-1111-1111-111111111111';

