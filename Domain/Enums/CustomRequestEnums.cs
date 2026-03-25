
using System.Text.Json.Serialization;

namespace Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<CustomRequestStatus>))]
public enum CustomRequestStatus
{
    New = 1,
    Reviewed = 2,
    Quoted = 3,
    Closed = 4,
    Rejected = 5
}
[JsonConverter(typeof(JsonStringEnumConverter<CustomQuoteStatus>))]
public enum CustomQuoteStatus
{
    Draft = 1,
    Sent = 2,
    Accepted = 3,
    Rejected = 4,
    Expired = 5,
}
