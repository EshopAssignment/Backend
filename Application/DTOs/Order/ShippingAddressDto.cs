using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Order;

public sealed record ShippingAddressDto
(
    string Street,
    string City,
    string PostalCode,
    string Country
);

