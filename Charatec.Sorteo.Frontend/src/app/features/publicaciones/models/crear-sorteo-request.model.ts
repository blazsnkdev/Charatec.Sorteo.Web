
export interface CrearSorteoRequest {
  titulo: string;
  descripcion: string;
  imagenPortadaUrl: string | null;
  aporteMinimo: number;
  metaRecaudacion: number | null;
  fechaInicio: string;
  fechaCierre: string;
  fechaSorteo: string | null;
}

export interface SorteoCreadoResponse {
  id: string;
  administradorUsuarioId: string;
  titulo: string;
  estado: string;
}
