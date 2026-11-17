
export interface ActualizarSorteoRequest {
  titulo: string;
  descripcion: string;
  aporteMinimo: number;
  metaRecaudacion: number | null;
  fechaInicio: string;
  fechaCierre: string;
  fechaSorteo: string | null;
}

export interface SorteoPublicadoResponse {
  id: string;
  titulo: string;
  estado: string;
  fechaActualizacion: string;
}
