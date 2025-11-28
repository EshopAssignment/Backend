using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Order;

public sealed record OrderCreatedDto(
    int OrderId,
    string OrderNumber,
    DateTime OrderDate,
    decimal Total
    );
