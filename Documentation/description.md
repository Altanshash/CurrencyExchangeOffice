# 💱 Currency Exchange Office

## What is this?
A fully working online currency exchange simulation — register, fund your account, 
and trade real currencies using live rates from the National Bank of Poland.

## What can you do?
| Feature | Description |
|---|---|
| 👤 Register / Login | Create your account and sign in securely |
| 📊 Live Rates | Real-time exchange rates from NBP API |
| 💰 Top Up | Add PLN to your virtual wallet |
| 🛒 Buy Currency | Buy USD, EUR, GBP and more |
| 💵 Sell Currency | Sell your currencies back for PLN |
| 📜 History | See every transaction you've made |

## Tech Stack
| Layer | Technology |
|---|---|
| 🔧 Backend | WCF Web Service (.NET Framework 4.8) |
| 🖥️ Frontend | WPF Desktop Application |
| 🗄️ Database | SQL Server Express |
| 🌐 Rates API | NBP Public API (api.nbp.pl) |

## Architecture
User → WPF Client → WCF Service → NBP API → SQL Server DB

## How to Run
1. 🗄️ Run `Database.sql` in SSMS
2. 💻 Open solution in Visual Studio
3. ▶️ Set both projects as startup → Press F5
4. 🎉 Register and start trading!
