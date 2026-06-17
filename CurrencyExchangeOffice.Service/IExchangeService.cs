using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace CurrencyExchangeOffice.Service
{
    [ServiceContract]
    public interface IExchangeService
    {
        [OperationContract]
        ExchangeRate GetCurrentRate(string currencyCode);

        [OperationContract]
        List<ExchangeRate> GetAllRates();

        [OperationContract]
        List<HistoricalRate> GetHistoricalRates(string currencyCode, string startDate, string endDate);

        [OperationContract]
        bool RegisterUser(string username, string passwordHash, string email);

        [OperationContract]
        UserDto LoginUser(string username, string passwordHash);

        [OperationContract]
        AccountBalance GetBalance(int userId);

        [OperationContract]
        bool TopUpAccount(int userId, decimal amount);

        [OperationContract]
        TransactionResult BuyCurrency(int userId, string currencyCode, decimal amount);

        [OperationContract]
        TransactionResult SellCurrency(int userId, string currencyCode, decimal amount);

        [OperationContract]
        List<TransactionDto> GetTransactionHistory(int userId);
    }

    [DataContract]
    public class ExchangeRate
    {
        [DataMember] public string CurrencyCode { get; set; }
        [DataMember] public string CurrencyName { get; set; }
        [DataMember] public decimal BuyRate { get; set; }
        [DataMember] public decimal SellRate { get; set; }
        [DataMember] public string Date { get; set; }
    }

    [DataContract]
    public class HistoricalRate
    {
        [DataMember] public string Date { get; set; }
        [DataMember] public decimal MidRate { get; set; }
    }

    [DataContract]
    public class UserDto
    {
        [DataMember] public int UserId { get; set; }
        [DataMember] public string Username { get; set; }
        [DataMember] public string Email { get; set; }
        [DataMember] public bool Success { get; set; }
        [DataMember] public string Message { get; set; }
    }

    [DataContract]
    public class AccountBalance
    {
        [DataMember] public decimal PLN { get; set; }
        [DataMember] public Dictionary<string, decimal> Currencies { get; set; }
    }

    [DataContract]
    public class TransactionResult
    {
        [DataMember] public bool Success { get; set; }
        [DataMember] public string Message { get; set; }
        [DataMember] public decimal NewPLNBalance { get; set; }
    }

    [DataContract]
    public class TransactionDto
    {
        [DataMember] public int TransactionId { get; set; }
        [DataMember] public string Type { get; set; }
        [DataMember] public string CurrencyCode { get; set; }
        [DataMember] public decimal Amount { get; set; }
        [DataMember] public decimal Rate { get; set; }
        [DataMember] public decimal PLNValue { get; set; }
        [DataMember] public string Date { get; set; }
    }
}