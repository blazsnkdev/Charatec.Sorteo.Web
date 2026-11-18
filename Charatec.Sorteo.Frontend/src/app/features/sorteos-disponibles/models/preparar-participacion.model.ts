export interface PrepararParticipacionResponse {
  sorteoId: string;

  titulo: string;
  descripcion: string;
  imagenPortadaUrl: string | null;

  fechaInicio: string;
  fechaCierre: string;
  fechaSorteo: string | null;

  aporteMinimo: number;
  aporteMaximo: number;
  pasoAporte: number;
  montoInicial: number;

  metaRecaudacion: number | null;
  moneda: string;

  usuarioId: string;
  email: string | null;

  puedeParticipar: boolean;
  mensajeDisponibilidad: string;
}
