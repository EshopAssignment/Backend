using System;
using System.Collections.Generic;
using System.Text;
using Application.DTOs;

namespace Application.Interfaces;

public interface IOrderService
{
    Task<OrderCreatedDto> CreateOrderAsync(CreateOrderRequestDto request, CancellationToken cancellationToken);
}
