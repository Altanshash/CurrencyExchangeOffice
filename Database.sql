CREATE DATABASE CurrencyExchangeDB;
GO

USE CurrencyExchangeDB;
GO

CREATE TABLE Users (
    UserId       INT IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    Email        NVARCHAR(100) NOT NULL,
    CreatedAt    DATETIME      DEFAULT GETDATE()
);

CREATE TABLE Balances (
    BalanceId    INT IDENTITY(1,1) PRIMARY KEY,
    UserId       INT            NOT NULL REFERENCES Users(UserId),
    CurrencyCode NVARCHAR(10)   NOT NULL,
    Amount       DECIMAL(18,4)  NOT NULL DEFAULT 0,
    CONSTRAINT UQ_UserCurrency UNIQUE (UserId, CurrencyCode)
);

CREATE TABLE Transactions (
    TransactionId INT IDENTITY(1,1) PRIMARY KEY,
    UserId        INT            NOT NULL REFERENCES Users(UserId),
    Type          NVARCHAR(10)   NOT NULL,
    CurrencyCode  NVARCHAR(10)   NOT NULL,
    Amount        DECIMAL(18,4)  NOT NULL,
    Rate          DECIMAL(18,6)  NOT NULL,
    PLNValue      DECIMAL(18,4)  NOT NULL,
    CreatedAt     DATETIME       DEFAULT GETDATE()
);
GO
