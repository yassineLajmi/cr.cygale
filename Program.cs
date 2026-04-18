using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CyberesaBoosterBridge.Models;
using CyberesaBoosterBridge.Services;

var builder = WebApplication.CreateBuilder(args);

var boosterOpts = builder.Configuration.GetSection("BoosterBC").Get<BoosterOptions>()
    ?? new BoosterOptions();
builder.Services.AddSingleton(boosterOpts);

if (boosterOpts.UseMock)
{
    builder.Services.AddSingleton<IBoosterClient, MockBoosterClient>();
}
else
{
    builder.Services.AddHttpClient<IBoosterClient, BoosterClient>();
}

var app = builder.Build();

// Cyberesa-compatible endpoints: accept GET (strRQ in query) and POST (strRQ as form field).
// Path matches the staging convention: /cr.ws.clicngo.uapi/hotel/webservicerq
foreach (var path in new[]
{
    "/cr.ws.clicngo.uapi/hotel/webservicerq",
    "/cr.ws.clicngo.uapi/hotel/webservicejsonrq",
    "/cr.ws/Hotel.asmx/WebServiceRQ",
    "/cr.ws/Hotel.asmx/WebServiceJsonRQ"
})
{
    app.MapGet(path, HandleGet);
    app.MapPost(path, HandlePost);
}

app.Run();

static async Task<IResult> HandleGet(HttpContext ctx, IBoosterClient booster, CancellationToken ct)
{
    var strRq = ctx.Request.Query["strRQ"].ToString();
    return await HandleAsync(strRq, booster, ct);
}

static async Task<IResult> HandlePost(HttpContext ctx, IBoosterClient booster, CancellationToken ct)
{
    string strRq;
    if (ctx.Request.HasFormContentType)
    {
        var form = await ctx.Request.ReadFormAsync(ct);
        strRq = form["strRQ"].ToString();
    }
    else
    {
        using var reader = new StreamReader(ctx.Request.Body, Encoding.UTF8);
        strRq = await reader.ReadToEndAsync(ct);
    }
    return await HandleAsync(strRq, booster, ct);
}

static async Task<IResult> HandleAsync(string? strRq, IBoosterClient booster, CancellationToken ct)
{
    if (string.IsNullOrWhiteSpace(strRq))
        return XmlResult(ErrorResponse("MISSING_STRRQ", "strRQ parameter is missing or empty"));

    HotelSearchReq req;
    try
    {
        req = DeserializeXml<HotelSearchReq>(strRq);
    }
    catch (Exception ex)
    {
        return XmlResult(ErrorResponse("INVALID_XML", $"Unable to parse HotelSearchREQ: {ex.Message}"));
    }

    try
    {
        var query = HotelSearchTranslator.ToBoosterQuery(req);
        var boosterRes = await booster.GetAvailabilityAsync(query, ct);
        var cyberesaRes = HotelSearchTranslator.ToCyberesaResponse(boosterRes, req);
        return XmlResult(cyberesaRes);
    }
    catch (HttpRequestException ex)
    {
        return XmlResult(ErrorResponse("BOOSTER_UPSTREAM", ex.Message));
    }
    catch (Exception ex)
    {
        return XmlResult(ErrorResponse("INTERNAL_ERROR", ex.Message));
    }
}

static HotelSearchRes ErrorResponse(string id, string message) =>
    new()
    {
        ErrorResult = new ErrorResult { Id = id, Message = message },
        Hotels = new HotelsList { Count = 0 }
    };

static T DeserializeXml<T>(string xml)
{
    var ser = new XmlSerializer(typeof(T));
    using var sr = new StringReader(xml);
    using var xr = XmlReader.Create(sr, new XmlReaderSettings { IgnoreWhitespace = true });
    return (T)ser.Deserialize(xr)!;
}

static IResult XmlResult(HotelSearchRes res)
{
    var ser = new XmlSerializer(typeof(HotelSearchRes));
    var ns = new XmlSerializerNamespaces();
    ns.Add("", "");
    using var ms = new MemoryStream();
    using (var xw = XmlWriter.Create(ms, new XmlWriterSettings
    {
        Encoding = new UTF8Encoding(false),
        Indent = true,
        OmitXmlDeclaration = false
    }))
    {
        ser.Serialize(xw, res, ns);
    }
    return Results.Bytes(ms.ToArray(), "text/xml; charset=utf-8");
}
