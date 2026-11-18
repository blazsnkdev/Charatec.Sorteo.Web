using Charatec.Sorteo.Business.DTOs.Responses.Participaciones;
using Charatec.Sorteo.Common.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace Charatec.Sorteo.Business.Interfaces
{
    public interface IParticipacionService
    {
        Task<Result<PrepararParticipacionResponse>>
        ObtenerAsync(
            Guid usuarioId,
            string? email,
            Guid sorteoId,
            CancellationToken cancellationToken = default
        );
    }
}
