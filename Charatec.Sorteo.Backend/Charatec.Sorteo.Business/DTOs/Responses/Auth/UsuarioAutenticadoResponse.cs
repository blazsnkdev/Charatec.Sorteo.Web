<<<<<<< HEAD
﻿namespace Charatec.Sorteo.Business.DTOs.Responses.Auth;
=======
﻿
namespace Charatec.Sorteo.Business.DTOs.Responses.Auth;
>>>>>>> backend

public sealed record UsuarioAutenticadoResponse(
    Guid Id,
    string Email,
    string? Nombres,
    string? Apellidos,
    string[] Roles
);
