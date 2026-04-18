using System.Text.Json.Serialization;

namespace CyberesaBoosterBridge.Models;

public class BoosterAvailabilityResponse
{
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("searchCode")] public string? SearchCode { get; set; }
    [JsonPropertyName("status")] public string? Status { get; set; }
    [JsonPropertyName("hotels")] public List<BoosterHotel> Hotels { get; set; } = new();
    [JsonPropertyName("checkIn")] public string? CheckIn { get; set; }
    [JsonPropertyName("checkOut")] public string? CheckOut { get; set; }
    [JsonPropertyName("nights")] public int Nights { get; set; }
    [JsonPropertyName("datetime")] public string? Datetime { get; set; }
}

public class BoosterHotel
{
    [JsonPropertyName("hotelId")] public int HotelId { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("rating")] public int Rating { get; set; }
    [JsonPropertyName("address")] public string? Address { get; set; }
    [JsonPropertyName("score")] public int Score { get; set; }
    [JsonPropertyName("hotelChainId")] public int HotelChainId { get; set; }
    [JsonPropertyName("accTypeId")] public int AccTypeId { get; set; }
    [JsonPropertyName("city")] public string? City { get; set; }
    [JsonPropertyName("cityId")] public int CityId { get; set; }
    [JsonPropertyName("zoneId")] public int ZoneId { get; set; }
    [JsonPropertyName("zone")] public string? Zone { get; set; }
    [JsonPropertyName("country")] public string? Country { get; set; }
    [JsonPropertyName("countryId")] public int CountryId { get; set; }
    [JsonPropertyName("lat")] public double Lat { get; set; }
    [JsonPropertyName("long")] public double Long { get; set; }
    [JsonPropertyName("marketingText")] public string? MarketingText { get; set; }
    [JsonPropertyName("minRate")] public double MinRate { get; set; }
    [JsonPropertyName("maxRate")] public double MaxRate { get; set; }
    [JsonPropertyName("currency")] public string? Currency { get; set; }
    [JsonPropertyName("rooms")] public List<BoosterRoom> Rooms { get; set; } = new();
    [JsonPropertyName("photos")] public List<string> Photos { get; set; } = new();
}

public class BoosterRoom
{
    [JsonPropertyName("code")] public int Code { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("rates")] public List<BoosterRate> Rates { get; set; } = new();
}

public class BoosterRate
{
    [JsonPropertyName("rateKey")] public string? RateKey { get; set; }
    [JsonPropertyName("amountWithoutPromotion")] public string? AmountWithoutPromotion { get; set; }
    [JsonPropertyName("rateClass")] public string? RateClass { get; set; }
    [JsonPropertyName("contractId")] public int ContractId { get; set; }
    [JsonPropertyName("rateType")] public string? RateType { get; set; }
    [JsonPropertyName("paymentType")] public string? PaymentType { get; set; }
    [JsonPropertyName("allotment")] public int Allotment { get; set; }
    [JsonPropertyName("availability")] public string? Availability { get; set; }
    [JsonPropertyName("amount")] public double Amount { get; set; }
    [JsonPropertyName("boardCode")] public int BoardCode { get; set; }
    [JsonPropertyName("boardName")] public string? BoardName { get; set; }
    [JsonPropertyName("cancellationPolicies")] public List<BoosterCancellationPolicy> CancellationPolicies { get; set; } = new();
    [JsonPropertyName("rooms")] public int Rooms { get; set; }
    [JsonPropertyName("adults")] public int Adults { get; set; }
    [JsonPropertyName("children")] public int Children { get; set; }
    [JsonPropertyName("infant")] public int Infant { get; set; }
    [JsonPropertyName("childrenAges")] public string? ChildrenAges { get; set; }
}

public class BoosterCancellationPolicy
{
    [JsonPropertyName("fromDate")] public string? FromDate { get; set; }
    [JsonPropertyName("amount")] public double Amount { get; set; }
    [JsonPropertyName("currency")] public string? Currency { get; set; }
}
