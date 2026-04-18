using System.Globalization;
using CyberesaBoosterBridge.Models;

namespace CyberesaBoosterBridge.Services;

public static class HotelSearchTranslator
{
    public static IDictionary<string, string?> ToBoosterQuery(HotelSearchReq req)
    {
        var sd = req.SearchDetails ?? throw new InvalidOperationException("SearchDetails missing");
        var bd = sd.BookingDetails ?? throw new InvalidOperationException("BookingDetails missing");

        var q = new Dictionary<string, string?>
        {
            ["cityId"] = bd.IdCity.ToString(CultureInfo.InvariantCulture),
            ["checkin"] = ConvertDate(bd.FromDate),
            ["checkout"] = ConvertDate(bd.ToDate),
            ["pax"] = BuildPax(bd.Rooms)
        };

        var f = sd.Filters;
        if (f is not null)
        {
            if (!string.IsNullOrWhiteSpace(f.Nationality))
                q["clientNationality"] = f.Nationality;
            if (!string.IsNullOrWhiteSpace(f.HotelsIds))
                q["hotelCodes"] = f.HotelsIds.Replace(';', ',');
            if (f.ItemsPerPage is > 0)
                q["itemsPerPage"] = f.ItemsPerPage.Value.ToString(CultureInfo.InvariantCulture);
        }
        return q;
    }

    public static HotelSearchRes ToCyberesaResponse(BoosterAvailabilityResponse? br, HotelSearchReq req)
    {
        var res = new HotelSearchRes
        {
            FromDate = req.SearchDetails?.BookingDetails?.FromDate,
            ToDate = req.SearchDetails?.BookingDetails?.ToDate,
            Language = req.SearchDetails?.BookingDetails?.Language,
            City = req.SearchDetails?.BookingDetails?.IdCity is int id
                ? new CityRef { Id = id, Name = br?.Hotels.FirstOrDefault()?.City }
                : null
        };

        if (br is null)
        {
            res.ErrorResult = new ErrorResult { Id = "EMPTY", Message = "No response from Booster" };
            res.Hotels.Count = 0;
            return res;
        }

        var currency = br.Hotels.FirstOrDefault()?.Currency;
        res.Currency = currency;

        res.Hotels.Count = br.Total;
        res.Hotels.Hotels = br.Hotels.Select(MapHotel).ToList();
        return res;
    }

    private static HotelRes MapHotel(BoosterHotel h)
    {
        return new HotelRes
        {
            Id = h.HotelId.ToString(CultureInfo.InvariantCulture),
            Title = h.Name,
            Category = new CategoryRes { Id = h.Rating.ToString(CultureInfo.InvariantCulture), Value = $"{h.Rating}*" },
            ThumbImage = h.Photos.FirstOrDefault() is { } url ? new ThumbImage { Url = url } : null,
            Localization = new Localization { Latitude = h.Lat, Longitude = h.Long },
            Address = h.Address,
            Summary = h.MarketingText,
            Rooms = new RoomsRes { Rooms = h.Rooms.Select(r => MapRoom(r, h.Currency)).ToList() }
        };
    }

    private static RoomResItem MapRoom(BoosterRoom r, string? currency)
    {
        var first = r.Rates.FirstOrDefault();
        return new RoomResItem
        {
            Id = r.Code.ToString(CultureInfo.InvariantCulture),
            Count = first?.Rooms ?? 1,
            Adult = first?.Adults ?? 0,
            Child = first?.Children ?? 0,
            Title = r.Name,
            AvailableQuantity = r.Rates.Sum(x => x.Allotment),
            Boardings = new Boardings
            {
                Items = r.Rates.Select(rate => MapBoarding(rate, currency)).ToList()
            }
        };
    }

    private static Boarding MapBoarding(BoosterRate rate, string? currency)
    {
        return new Boarding
        {
            Id = rate.BoardCode.ToString(CultureInfo.InvariantCulture),
            OfferId = rate.RateKey,
            Title = rate.BoardName,
            Available = new Available
            {
                Code = rate.Availability,
                Status = rate.Availability switch { "A" => "Available", "S" => "Stop", "R" => "Request", _ => null },
                Value = rate.Allotment.ToString(CultureInfo.InvariantCulture)
            },
            Rate = new Rate
            {
                Currency = currency,
                Value = rate.Amount.ToString(CultureInfo.InvariantCulture)
            },
            NonRefundable = (rate.RateClass?.Equals("NRF", StringComparison.OrdinalIgnoreCase) ?? false) ? "true" : "false",
            CancellationPolicies = rate.CancellationPolicies.Select(cp => new CancellationPolicy
            {
                FromDate = cp.FromDate,
                Fee = cp.Amount.ToString(CultureInfo.InvariantCulture)
            }).ToList()
        };
    }

    private static string? ConvertDate(string? src)
    {
        if (string.IsNullOrWhiteSpace(src)) return null;
        string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" };
        if (DateTime.TryParseExact(src, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return src;
    }

    private static string BuildPax(List<RoomReq> rooms)
    {
        if (rooms.Count == 0) return "1";
        var parts = new List<string>();
        foreach (var r in rooms)
        {
            var pax = r.Pax ?? new Pax { Adult = 1 };
            var seg = new List<string> { pax.Adult.ToString(CultureInfo.InvariantCulture) };
            foreach (var c in pax.Children)
            {
                var age = ExtractAge(c.Age);
                for (int i = 0; i < c.Count; i++) seg.Add(age);
            }
            foreach (var inf in pax.Infants)
            {
                for (int i = 0; i < inf.Count; i++) seg.Add("0");
            }
            parts.Add(string.Join(",", seg));
        }
        return string.Join(";", parts);
    }

    private static string ExtractAge(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "0";
        var first = new string(raw.TakeWhile(char.IsDigit).ToArray());
        return string.IsNullOrEmpty(first) ? "0" : first;
    }
}
