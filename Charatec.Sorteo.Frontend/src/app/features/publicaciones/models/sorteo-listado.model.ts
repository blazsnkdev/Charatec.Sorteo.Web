
export type EstadoSorteo =
  | 'Borrador'
  | 'Publicado'
  | 'Cerrado'
  | 'Finalizado'
  | 'Cancelado';

export interface SorteoListado {
  id: string;
  administradorUsuarioId: string;
  titulo: string;
  descripcion: string;
  imagenPortadaUrl: string | null;
  aporteMinimo: number;
  metaRecaudacion: number | null;
  fechaInicio: string;
  fechaCierre: string;
  fechaSorteo: string | null;
  estado: EstadoSorteo;
  fechaCreacion: string;
}
