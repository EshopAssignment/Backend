using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Auth;

public sealed record LoginDto(string Email, string Password);

public sealed record RegisterDto(string Email, string Password, string DisplayName);



