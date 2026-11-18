using System;
using System.Collections.Generic;
using System.Text;

namespace Charatec.Sorteo.Business.Options
{
    public sealed class ParticipacionOptions
    {
        public decimal AporteMaximo { get; set; } = 100m;//TOOD: esto sera modificable
        public decimal PasoAporte { get; set; } = 1m;
    }
}
