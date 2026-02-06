using System;
using System.Collections.Generic;
using System.Text;

namespace IntegrationTests.Contracts.Orders;

public sealed class OrderCreatedDto
{
    public string OrderNumber { get; set; } = "";
    public int Id { get; set; }
    }
