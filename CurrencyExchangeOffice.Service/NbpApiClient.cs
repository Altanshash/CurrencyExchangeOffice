using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace CurrencyExchangeOffice.Service
{
    public class NbpApiClient
    {
        private static readonly HttpClient _http = new HttpClient();
        private const string BaseUrl = "http://api.nbp.pl/api";

        public NbpRate GetCurrentRate(string currencyCode)
        {
            try
            {
                string url = $"{BaseUrl}/exchangerates/rates/c/{currencyCode}/?format=json";
                var response = _http.GetStringAsync(url).Result;
                var data = JsonConvert.DeserializeObject<NbpResponse>(response);
                return data?.rates?[0];
            }
            catch
            {
                try
                {
                    string url = $"{BaseUrl}/exchangerates/rates/a/{currencyCode}/?format=json";
                    var response = _http.GetStringAsync(url).Result;
                    var data = JsonConvert.DeserializeObject<NbpResponse>(response);
                    var rate = data?.rates?[0];
                    if (rate != null)
                    {
                        rate.bid = rate.mid * 0.99m;
                        rate.ask = rate.mid * 1.01m;
                    }
                    return rate;
                }
                catch { return null; }
            }
        }

        public List<NbpRate> GetHistoricalRates(string currencyCode, string startDate, string endDate)
        {
            try
            {
                string url = $"{BaseUrl}/exchangerates/rates/a/{currencyCode}/{startDate}/{endDate}/?format=json";
                var response = _http.GetStringAsync(url).Result;
                var data = JsonConvert.DeserializeObject<NbpResponse>(response);
                return data?.rates ?? new List<NbpRate>();
            }
            catch { return new List<NbpRate>(); }
        }

        public List<NbpTableEntry> GetAllRates()
        {
            try
            {
                string url = $"{BaseUrl}/exchangerates/tables/a/?format=json";
                var response = _http.GetStringAsync(url).Result;
                var tables = JsonConvert.DeserializeObject<List<NbpTable>>(response);
                return tables?[0]?.rates ?? new List<NbpTableEntry>();
            }
            catch { return new List<NbpTableEntry>(); }
        }
    }

    public class NbpResponse
    {
        public string currency { get; set; }
        public string code { get; set; }
        public List<NbpRate> rates { get; set; }
    }

    public class NbpRate
    {
        public string effectiveDate { get; set; }
        public decimal? bid { get; set; }
        public decimal? ask { get; set; }
        public decimal? mid { get; set; }
    }

    public class NbpTable
    {
        public List<NbpTableEntry> rates { get; set; }
    }

    public class NbpTableEntry
    {
        public string currency { get; set; }
        public string code { get; set; }
        public decimal mid { get; set; }
    }
}