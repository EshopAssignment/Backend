

namespace Application.DTOs.Reservations;

public sealed record SetCartReservationDto(
    string CartId,
    int ProductId,
    int Quantity,
    int ReservationTtlMinutes = 30
    );
