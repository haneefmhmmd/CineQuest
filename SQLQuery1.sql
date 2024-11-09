

USE StreamingServiceDB;
GO

USE movieapp;

-- User Table for Registration
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FullName VARCHAR(50) NOT NULL,
    Username VARCHAR(50) UNIQUE NOT NULL,
    PasswordHash VARCHAR(50) NOT NULL
);

USE movieapp;
ALTER TABLE Users ADD UserID INT IDENTITY(1,1) PRIMARY KEY;



-- Rating Table
CREATE TABLE Ratings (
    RatingID INT IDENTITY(1,1) PRIMARY KEY,
    MovieID INT FOREIGN KEY REFERENCES Movies(MovieID) ON DELETE CASCADE,
    UserID INT FOREIGN KEY REFERENCES Users(UserID) ON DELETE CASCADE,
    RatingValue INT CHECK (RatingValue BETWEEN 1 AND 10) NOT NULL,  -- Ratings 1-10
    RatingDate DATETIME DEFAULT GETDATE()
);
GO



-- Scaffold-DbContext "Data Source=movie-app.c5ueo6sqo8m2.us-east-1.rds.amazonaws.com,1433;Database=movieapp;TrustServerCertificate=true;User ID=admin;Password=KlEmvY4EuPoaq5DDZ22l;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models