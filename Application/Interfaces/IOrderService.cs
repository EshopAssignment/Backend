using System;
using System.Collections.Generic;
using System.Text;
using Application.DTOs.Order;

namespace Application.Interfaces;

public interface IOrderService
{
    Task<OrderCreatedDto> CreateOrderAsync(CreateOrderRequestDto request, CancellationToken ct);
    Task<OrderCreatedDto?> GetOrderByIdAsync(int id, CancellationToken ct);
}
