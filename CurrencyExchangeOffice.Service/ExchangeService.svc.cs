using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace CurrencyExchangeOffice.Service
{
    public class ExchangeService : IExchangeService
    {
        private readonly NbpApiClient _nbp = new NbpApiClient();
        private const string ConnStr = @"Server=localhost\SQLEXPRESS;Database=CurrencyExchangeDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public ExchangeRate GetCurrentRate(string currencyCode)
        {
            var rate = _nbp.GetCurrentRate(currencyCode.ToUpper());
            if (rate == null) return null;
            decimal buy = rate.bid ?? rate.mid.Value * 0.99m;
            decimal sell = rate.ask ?? rate.mid.Value * 1.01m;
            return new ExchangeRate
            {
                CurrencyCode = currencyCode.ToUpper(),
                BuyRate = Math.Round(buy, 4),
                SellRate = Math.Round(sell, 4),
                Date = rate.effectiveDate
            };
        }

        public List<ExchangeRate> GetAllRates()
        {
            var entries = _nbp.GetAllRates();
            return entries.Select(e => new ExchangeRate
            {
                CurrencyCode = e.code,
                CurrencyName = e.currency,
                BuyRate = Math.Round(e.mid * 0.99m, 4),
                SellRate = Math.Round(e.mid * 1.01m, 4),
                Date = DateTime.Today.ToString("yyyy-MM-dd")
            }).ToList();
        }

        public List<HistoricalRate> GetHistoricalRates(string currencyCode, string startDate, string endDate)
        {
            var rates = _nbp.GetHistoricalRates(currencyCode, startDate, endDate);
            return rates.Select(r => new HistoricalRate
            {
                Date = r.effectiveDate,
                MidRate = r.mid ?? 0
            }).ToList();
        }

        public bool RegisterUser(string username, string passwordHash, string email)
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "INSERT INTO Users (Username, PasswordHash, Email, CreatedAt) VALUES (@u, @p, @e, GETDATE())", conn);
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@p", passwordHash);
                    cmd.Parameters.AddWithValue("@e", email);
                    cmd.ExecuteNonQuery();

                    int userId = (int)(decimal)new SqlCommand("SELECT SCOPE_IDENTITY()", conn).ExecuteScalar();
                    var balCmd = new SqlCommand(
                        "INSERT INTO Balances (UserId, CurrencyCode, Amount) VALUES (@id, 'PLN', 0)", conn);
                    balCmd.Parameters.AddWithValue("@id", userId);
                    balCmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch { return false; }
        }

        public UserDto LoginUser(string username, string passwordHash)
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "SELECT UserId, Username, Email FROM Users WHERE Username=@u AND PasswordHash=@p", conn);
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@p", passwordHash);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                            return new UserDto
                            {
                                UserId = (int)r["UserId"],
                                Username = r["Username"].ToString(),
                                Email = r["Email"].ToString(),
                                Success = true
                            };
                    }
                }
                return new UserDto { Success = false, Message = "Invalid credentials" };
            }
            catch (Exception ex)
            {
                return new UserDto { Success = false, Message = ex.Message };
            }
        }

        public AccountBalance GetBalance(int userId)
        {
            var result = new AccountBalance { Currencies = new Dictionary<string, decimal>() };
            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                var cmd = new SqlCommand(
                    "SELECT CurrencyCode, Amount FROM Balances WHERE UserId=@id", conn);
                cmd.Parameters.AddWithValue("@id", userId);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        string code = r["CurrencyCode"].ToString();
                        decimal amount = (decimal)r["Amount"];
                        if (code == "PLN") result.PLN = amount;
                        else result.Currencies[code] = amount;
                    }
                }
            }
            return result;
        }

        public bool TopUpAccount(int userId, decimal amount)
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        @"IF EXISTS (SELECT 1 FROM Balances WHERE UserId=@id AND CurrencyCode='PLN')
                            UPDATE Balances SET Amount = Amount + @amt WHERE UserId=@id AND CurrencyCode='PLN'
                          ELSE
                            INSERT INTO Balances (UserId, CurrencyCode, Amount) VALUES (@id, 'PLN', @amt)", conn);
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.Parameters.AddWithValue("@amt", amount);
                    cmd.ExecuteNonQuery();
                    LogTransaction(conn, userId, "TOPUP", "PLN", amount, 1, amount);
                    return true;
                }
            }
            catch { return false; }
        }

        public TransactionResult BuyCurrency(int userId, string currencyCode, decimal amount)
        {
            var rate = GetCurrentRate(currencyCode);
            if (rate == null)
                return new TransactionResult { Success = false, Message = "Currency not found" };

            decimal plnCost = Math.Round(amount * rate.SellRate, 2);
            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                decimal plnBalance = GetCurrencyBalance(conn, userId, "PLN");
                if (plnBalance < plnCost)
                    return new TransactionResult
                    {
                        Success = false,
                        Message = $"Insufficient PLN. Need {plnCost:F2}, have {plnBalance:F2}"
                    };

                UpdateBalance(conn, userId, "PLN", -plnCost);
                UpdateBalance(conn, userId, currencyCode, amount);
                LogTransaction(conn, userId, "BUY", currencyCode, amount, rate.SellRate, plnCost);

                return new TransactionResult
                {
                    Success = true,
                    Message = $"Bought {amount} {currencyCode} for {plnCost} PLN",
                    NewPLNBalance = GetCurrencyBalance(conn, userId, "PLN")
                };
            }
        }

        public TransactionResult SellCurrency(int userId, string currencyCode, decimal amount)
        {
            var rate = GetCurrentRate(currencyCode);
            if (rate == null)
                return new TransactionResult { Success = false, Message = "Currency not found" };

            decimal plnGain = Math.Round(amount * rate.BuyRate, 2);
            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                decimal currBalance = GetCurrencyBalance(conn, userId, currencyCode);
                if (currBalance < amount)
                    return new TransactionResult
                    {
                        Success = false,
                        Message = $"Insufficient {currencyCode}"
                    };

                UpdateBalance(conn, userId, currencyCode, -amount);
                UpdateBalance(conn, userId, "PLN", plnGain);
                LogTransaction(conn, userId, "SELL", currencyCode, amount, rate.BuyRate, plnGain);

                return new TransactionResult
                {
                    Success = true,
                    Message = $"Sold {amount} {currencyCode} for {plnGain} PLN",
                    NewPLNBalance = GetCurrencyBalance(conn, userId, "PLN")
                };
            }
        }

        public List<TransactionDto> GetTransactionHistory(int userId)
        {
            var list = new List<TransactionDto>();
            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                var cmd = new SqlCommand(
                    "SELECT * FROM Transactions WHERE UserId=@id ORDER BY CreatedAt DESC", conn);
                cmd.Parameters.AddWithValue("@id", userId);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(new TransactionDto
                        {
                            TransactionId = (int)r["TransactionId"],
                            Type = r["Type"].ToString(),
                            CurrencyCode = r["CurrencyCode"].ToString(),
                            Amount = (decimal)r["Amount"],
                            Rate = (decimal)r["Rate"],
                            PLNValue = (decimal)r["PLNValue"],
                            Date = ((DateTime)r["CreatedAt"]).ToString("yyyy-MM-dd HH:mm")
                        });
                }
            }
            return list;
        }

        private decimal GetCurrencyBalance(SqlConnection conn, int userId, string code)
        {
            var cmd = new SqlCommand(
                "SELECT ISNULL(Amount, 0) FROM Balances WHERE UserId=@id AND CurrencyCode=@c", conn);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.Parameters.AddWithValue("@c", code);
            return (decimal)(cmd.ExecuteScalar() ?? 0m);
        }

        private void UpdateBalance(SqlConnection conn, int userId, string code, decimal delta)
        {
            var cmd = new SqlCommand(
                @"IF EXISTS (SELECT 1 FROM Balances WHERE UserId=@id AND CurrencyCode=@c)
                    UPDATE Balances SET Amount = Amount + @d WHERE UserId=@id AND CurrencyCode=@c
                  ELSE
                    INSERT INTO Balances (UserId, CurrencyCode, Amount) VALUES (@id, @c, @d)", conn);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.Parameters.AddWithValue("@c", code);
            cmd.Parameters.AddWithValue("@d", delta);
            cmd.ExecuteNonQuery();
        }

        private void LogTransaction(SqlConnection conn, int userId, string type,
            string code, decimal amount, decimal rate, decimal plnValue)
        {
            var cmd = new SqlCommand(
                @"INSERT INTO Transactions (UserId, Type, CurrencyCode, Amount, Rate, PLNValue, CreatedAt)
                  VALUES (@id, @t, @c, @a, @r, @p, GETDATE())", conn);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.Parameters.AddWithValue("@t", type);
            cmd.Parameters.AddWithValue("@c", code);
            cmd.Parameters.AddWithValue("@a", amount);
            cmd.Parameters.AddWithValue("@r", rate);
            cmd.Parameters.AddWithValue("@p", plnValue);
            cmd.ExecuteNonQuery();
        }
    }
}