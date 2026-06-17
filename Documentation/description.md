# Project Documentation

## Project Title
Currency Exchange Office

## Course
Network Application Development

## Description
A desktop application that simulates an online currency exchange office.

## What Was Built

### WCF Web Service
- Connects to NBP API to get real exchange rates
- Handles user registration and login
- Processes buy and sell currency transactions
- Stores all data in SQL Server database

### WPF Client Application
- Login and registration screen
- View current exchange rates (live from NBP)
- Top up PLN balance
- Buy and sell foreign currencies
- View transaction history

### Database (SQL Server)
- Users table
- Balances table
- Transactions table

## How It Works
1. User registers and logs in
2. User tops up PLN balance
3. User checks live exchange rates
4. User buys or sells currencies
5. All transactions are saved to database
