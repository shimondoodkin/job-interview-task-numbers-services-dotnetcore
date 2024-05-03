USE [master];
GO

-- user for healthcheck
IF NOT EXISTS (SELECT * FROM sys.sql_logins WHERE name = 'newuser')
BEGIN
    CREATE LOGIN [newuser] WITH PASSWORD = 'password123', CHECK_POLICY = OFF;
    ALTER SERVER ROLE [sysadmin] ADD MEMBER [newuser];
END
GO

/*

-- Check if the 'messages' database exists and create it if it does not
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'messages')
BEGIN
    CREATE DATABASE messages;
    PRINT 'Database "messages" created';
END
ELSE
    PRINT 'Database "messages" already exists';
GO

-- Ensure the login does not already exist before creating
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'messages_user')
BEGIN
    CREATE LOGIN messages_user WITH PASSWORD = 'example123', CHECK_POLICY = OFF;
    PRINT 'Login "messages_user" created';
END
ELSE
    PRINT 'Login "messages_user" already exists';
GO

-- Change context to the 'messages' database
USE [messages];
GO

-- Create a database user for the login if it does not exist
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'messages_user')
BEGIN
    CREATE USER messages_user FOR LOGIN messages_user;
    PRINT 'Database user "messages_user" created in "messages"';
END
ELSE
    PRINT 'Database user "messages_user" already exists in "messages"';
GO

-- Grant owner rights to the user
EXEC sp_addrolemember 'db_owner', 'messages_user';
PRINT 'User "messages_user" is granted db_owner permissions on "messages"';
GO

*/