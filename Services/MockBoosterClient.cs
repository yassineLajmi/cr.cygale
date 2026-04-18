using System.Globalization;
using System.Text.Json;
using CyberesaBoosterBridge.Models;

namespace CyberesaBoosterBridge.Services;

public class MockBoosterClient : IBoosterClient
{
    private readonly ILogger<MockBoosterClient> _log;

    public MockBoosterClient(ILogger<MockBoosterClient> log) => _log = log;

    public Task<BoosterAvailabilityResponse?> GetAvailabilityAsync(
        IDictionary<string, string?> query,
        CancellationToken ct)
    {
        var qs = string.Join("&", query
            .Where(kv => !string.IsNullOrEmpty(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}"));
        var callId = BoosterCallLogger.NewCallId();
        var reqDump = $"GET /hotels/availability?{qs}\n(mock)\n";
        BoosterCallLogger.SaveRequest(callId, reqDump);
        _log.LogInformation(
            "\n===== BOOSTER REQ (MOCK) [{CallId}] =====\n{Dump}==============================",
            callId, reqDump);

        var checkin = (query.TryGetValue("checkin", out var ci) ? ci : null) ?? "2026-06-15";
        var checkout = (query.TryGetValue("checkout", out var co) ? co : null) ?? "2026-06-18";
        int cityId = query.TryGetValue("cityId", out var c) && int.TryParse(c, out var ci2) ? ci2 : 104;

        int nights = 3;
        if (DateTime.TryParseExact(checkin, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d1) &&
            DateTime.TryParseExact(checkout, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d2))
        {
            nights = Math.Max(1, (int)(d2 - d1).TotalDays);
        }

        var resp = new BoosterAvailabilityResponse
        {
            Total = 2,
            SearchCode = "MOCK-" + Guid.NewGuid().ToString("N")[..10],
            Status = "OK",
            CheckIn = checkin,
            CheckOut = checkout,
            Nights = nights,
            Datetime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
            Hotels = new List<BoosterHotel>
            {
                new()
                {
                    HotelId = 10001,
                    Name = "Mock Palace Resort",
                    Rating = 5,
                    Address = "1 Avenue des Tests",
                    City = "MockCity",
                    CityId = cityId,
                    Country = "France",
                    CountryId = 33,
                    Lat = 43.7102,
                    Long = 7.2620,
                    MarketingText = "A mocked luxury hotel for integration tests.",
                    MinRate = 220.0,
                    MaxRate = 480.0,
                    Currency = "EUR",
                    Photos = new List<string> { "https://example.com/mock-palace.jpg" },
                    Rooms = new List<BoosterRoom>
                    {
                        new()
                        {
                            Code = 501,
                            Name = "Deluxe Double",
                            Rates = new List<BoosterRate>
                            {
                                new()
                                {
                                    RateKey = $"{checkin.Replace("-", "")}|{checkout.Replace("-", "")}|10001|501|5|1|2~0~|MOCK",
                                    AmountWithoutPromotion = "250.00",
                                    RateClass = "NOR",
                                    ContractId = 3,
                                    PaymentType = "AT_WEB",
                                    Allotment = 7,
                                    Availability = "A",
                                    Amount = 220.00,
                                    BoardCode = 5,
                                    BoardName = "All Inclusive",
                                    Rooms = 1,
                                    Adults = 2,
                                    CancellationPolicies = new List<BoosterCancellationPolicy>
                                    {
                                        new() { FromDate = checkin, Amount = 220.00, Currency = "EUR" }
                                    }
                                },
                                new()
                                {
                                    RateKey = $"{checkin.Replace("-", "")}|{checkout.Replace("-", "")}|10001|501|2|1|2~0~|MOCK",
                                    RateClass = "NRF",
                                    ContractId = 3,
                                    PaymentType = "AT_WEB",
                                    Allotment = 3,
                                    Availability = "A",
                                    Amount = 180.00,
                                    BoardCode = 2,
                                    BoardName = "Breakfast",
                                    Rooms = 1,
                                    Adults = 2
                                }
                            }
                        }
                    }
                },
                new()
                {
                    HotelId = 10042,
                    Name = "Mock Seaside Inn",
                    Rating = 3,
                    Address = "12 Promenade du Mock",
                    City = "MockCity",
                    CityId = cityId,
                    Country = "France",
                    CountryId = 33,
                    Lat = 43.6950,
                    Long = 7.2710,
                    MarketingText = "Simple mocked inn near the sea.",
                    MinRate = 85.0,
                    MaxRate = 140.0,
                    Currency = "EUR",
                    Photos = new List<string> { "https://example.com/mock-inn.jpg" },
                    Rooms = new List<BoosterRoom>
                    {
                        new()
                        {
                            Code = 201,
                            Name = "Standard Twin",
                            Rates = new List<BoosterRate>
                            {
                                new()
                                {
                                    RateKey = $"{checkin.Replace("-", "")}|{checkout.Replace("-", "")}|10042|201|1|1|2~0~|MOCK",
                                    RateClass = "NOR",
                                    ContractId = 7,
                                    PaymentType = "AT_HOTEL",
                                    Allotment = 5,
                                    Availability = "A",
                                    Amount = 92.50,
                                    BoardCode = 1,
                                    BoardName = "Room Only",
                                    Rooms = 1,
                                    Adults = 2
                                }
                            }
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(resp, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
        BoosterCallLogger.SaveResponse(callId, $"HTTP 200 OK\n{json}");
        _log.LogInformation(
            "\n===== BOOSTER RES (MOCK) [{CallId}] =====\nHTTP 200 OK\n{Body}\n==============================",
            callId, json);

        return Task.FromResult<BoosterAvailabilityResponse?>(resp);
    }
}
