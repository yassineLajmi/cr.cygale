using System.Xml.Serialization;

namespace CyberesaBoosterBridge.Models;

[XmlRoot("HotelSearchREQ")]
public class HotelSearchReq
{
    [XmlElement("Credential")] public Credential? Credential { get; set; }
    [XmlElement("SearchDetails")] public SearchDetails? SearchDetails { get; set; }
}

public class Credential
{
    [XmlElement("Login")] public string? Login { get; set; }
    [XmlElement("Password")] public string? Password { get; set; }
}

public class SearchDetails
{
    [XmlElement("BookingDetails")] public BookingDetails? BookingDetails { get; set; }
    [XmlElement("Filters")] public Filters? Filters { get; set; }
}

public class BookingDetails
{
    [XmlElement("User")] public string? User { get; set; }
    [XmlElement("Language")] public string? Language { get; set; }
    [XmlElement("FromDate")] public string? FromDate { get; set; }
    [XmlElement("ToDate")] public string? ToDate { get; set; }
    [XmlElement("IdCity")] public int IdCity { get; set; }
    [XmlElement("Timeout")] public string? Timeout { get; set; }
    [XmlArray("Rooms"), XmlArrayItem("Room")] public List<RoomReq> Rooms { get; set; } = new();
}

public class RoomReq
{
    [XmlElement("Pax")] public Pax? Pax { get; set; }
    [XmlElement("Boarding")] public string? Boarding { get; set; }
}

public class Pax
{
    [XmlElement("Adult")] public int Adult { get; set; }
    [XmlElement("Child")] public List<ChildPax> Children { get; set; } = new();
    [XmlElement("Infant")] public List<InfantPax> Infants { get; set; } = new();
}

public class ChildPax
{
    [XmlAttribute("age")] public string? Age { get; set; }
    [XmlText] public int Count { get; set; }
}

public class InfantPax
{
    [XmlAttribute("age")] public string? Age { get; set; }
    [XmlText] public int Count { get; set; }
}

public class Filters
{
    [XmlElement("Nationality")] public string? Nationality { get; set; }
    [XmlElement("SourceMarket")] public string? SourceMarket { get; set; }
    [XmlElement("hotels_ids")] public string? HotelsIds { get; set; }
    [XmlElement("exclude_hotels_ids")] public string? ExcludeHotelsIds { get; set; }
    [XmlElement("ItemsPerPage")] public int? ItemsPerPage { get; set; }
    [XmlElement("PageNumber")] public int? PageNumber { get; set; }
    [XmlElement("OnlyAvailable")] public string? OnlyAvailable { get; set; }
    [XmlElement("WithoutNRF")] public string? WithoutNrf { get; set; }
}

[XmlRoot("HotelSearchRES")]
public class HotelSearchRes
{
    [XmlElement("ErrorResult")] public ErrorResult? ErrorResult { get; set; }
    [XmlElement("FromDate")] public string? FromDate { get; set; }
    [XmlElement("ToDate")] public string? ToDate { get; set; }
    [XmlElement("Language")] public string? Language { get; set; }
    [XmlElement("Currency")] public string? Currency { get; set; }
    [XmlElement("City")] public CityRef? City { get; set; }
    [XmlElement("PaginationData")] public PaginationData? PaginationData { get; set; }
    [XmlElement("Hotels")] public HotelsList Hotels { get; set; } = new();
}

public class ErrorResult
{
    [XmlAttribute("id")] public string? Id { get; set; }
    [XmlElement("message")] public string? Message { get; set; }
}

public class CityRef
{
    [XmlAttribute("id")] public int Id { get; set; }
    [XmlText] public string? Name { get; set; }
}

public class PaginationData
{
    [XmlAttribute("currentPage")] public int CurrentPage { get; set; }
    [XmlAttribute("TotalPages")] public int TotalPages { get; set; }
}

public class HotelsList
{
    [XmlAttribute("count")] public int Count { get; set; }
    [XmlElement("Hotel")] public List<HotelRes> Hotels { get; set; } = new();
}

public class HotelRes
{
    [XmlAttribute("id")] public string? Id { get; set; }
    [XmlElement("Source")] public string? Source { get; set; }
    [XmlElement("Title")] public string? Title { get; set; }
    [XmlElement("Category")] public CategoryRes? Category { get; set; }
    [XmlElement("ThumbImage")] public ThumbImage? ThumbImage { get; set; }
    [XmlElement("Localization")] public Localization? Localization { get; set; }
    [XmlElement("Address")] public string? Address { get; set; }
    [XmlElement("Summary")] public string? Summary { get; set; }
    [XmlElement("Rooms")] public RoomsRes Rooms { get; set; } = new();
}

public class CategoryRes
{
    [XmlAttribute("id")] public string? Id { get; set; }
    [XmlText] public string? Value { get; set; }
}

public class ThumbImage
{
    [XmlAttribute("url")] public string? Url { get; set; }
    [XmlText] public string? Value { get; set; }
}

public class Localization
{
    [XmlElement("Longitude")] public double Longitude { get; set; }
    [XmlElement("Latitude")] public double Latitude { get; set; }
}

public class RoomsRes
{
    [XmlElement("Room")] public List<RoomResItem> Rooms { get; set; } = new();
}

public class RoomResItem
{
    [XmlAttribute("id")] public string? Id { get; set; }
    [XmlAttribute("count")] public int Count { get; set; }
    [XmlElement("Adult")] public int Adult { get; set; }
    [XmlElement("Child")] public int Child { get; set; }
    [XmlElement("Title")] public string? Title { get; set; }
    [XmlElement("AvailableQuantity")] public int AvailableQuantity { get; set; }
    [XmlElement("Boardings")] public Boardings Boardings { get; set; } = new();
}

public class Boardings
{
    [XmlElement("Boarding")] public List<Boarding> Items { get; set; } = new();
}

public class Boarding
{
    [XmlAttribute("id")] public string? Id { get; set; }
    [XmlAttribute("OfferId")] public string? OfferId { get; set; }
    [XmlElement("Title")] public string? Title { get; set; }
    [XmlElement("Available")] public Available? Available { get; set; }
    [XmlElement("Rate")] public Rate? Rate { get; set; }
    [XmlElement("NonRefundable")] public string? NonRefundable { get; set; }
    [XmlElement("CancellationPolicy")] public List<CancellationPolicy> CancellationPolicies { get; set; } = new();
}

public class Available
{
    [XmlAttribute("code")] public string? Code { get; set; }
    [XmlAttribute("status")] public string? Status { get; set; }
    [XmlAttribute("value")] public string? Value { get; set; }
    [XmlText] public string? Text { get; set; }
}

public class Rate
{
    [XmlAttribute("currency")] public string? Currency { get; set; }
    [XmlText] public string? Value { get; set; }
}

public class CancellationPolicy
{
    [XmlElement("FromDate")] public string? FromDate { get; set; }
    [XmlElement("Fee")] public string? Fee { get; set; }
}
