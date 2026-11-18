using Charatec.Sorteo.Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Charatec.Sorteo.Data.Interfaces
{
    public interface IAporteRepository
    {
        IQueryable<Aporte> Consultar();

        IQueryable<AporteRevisionConsulta> ConsultarPorSorteo(
            Guid sorteoId
        );
        Task<RevisionAporteResultado> VerificarAsync(
            Guid aporteId,
            Guid administradorUsuarioId,
            bool aprobar,
            string? observacion,
            CancellationToken cancellationToken = default
        );

    }
}
