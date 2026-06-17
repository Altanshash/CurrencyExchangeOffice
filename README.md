# Currency Exchange Office

**Course:** Network Application Development                                    
**Project Title:** Currency Exchange Office System
**Author:** Altanshash Tolbalhan
**Student ID:** [64295]

## Description
A network-based currency exchange office simulation built with WCF and WPF.
Users can register, log in, check real-time NBP exchange rates, top up PLN balance,
buy and sell foreign currencies, and view full transaction history.

## Project Structure
- `WCF-Service` — WCF Web Service with business logic and NBP API integration
- `Client-Application` — WPF desktop client
- `Database` — SQL Server schema and initialization scripts
- `Documentation` — Project description and architecture overview

## How to Run
1. Install SQL Server Express and SSMS
2. Run `Database/schema.sql` in SSMS
3. Open solution in Visual Studio
4. Right-click Solution → Properties → set both projects as startup
5. Press F5

## Tech Stack
- .NET Framework 4.8
- WCF (Windows Communication Foundation)
- WPF (Windows Presentation Foundation)
- SQL Server Express
- NBP Public API (http://api.nbp.pl)
