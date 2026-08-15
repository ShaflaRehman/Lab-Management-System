CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY,
    Username NVARCHAR(100),
    Email NVARCHAR(100),
    Password NVARCHAR(100),
    Role NVARCHAR(50)
);

ALTER TABLE Users
ADD CONSTRAINT UQ_Users_Email UNIQUE (Email);

ALTER TABLE Users
ADD CONSTRAINT UQ_Users_Username UNIQUE (Username);

ALTER TABLE Users
ADD PhoneNumber NVARCHAR(20);

ALTER TABLE Users
ADD Department NVARCHAR(100);

CREATE TABLE LabAttendants (
    UserId INT PRIMARY KEY,  -- same as Id in Users
    LabId INT,               -- foreign key to Labs
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (LabId) REFERENCES Labs(LabId)
);

CREATE TABLE Students (
    UserId INT PRIMARY KEY,  -- same as Id in Users
    Degree NVARCHAR(100),
    Syndicate NVARCHAR(100),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE TABLE Labs (
    LabId INT PRIMARY KEY IDENTITY,
    LabName NVARCHAR(100)
);




