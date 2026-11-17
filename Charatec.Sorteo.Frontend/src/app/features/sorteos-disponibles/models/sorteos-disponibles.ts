
export interface SorteoDisponible {
  id: string;
  titulo: string;
  descripcion: string;
  imagenPortadaUrl: string | null;
  aporteMinimo: number;
  metaRecaudacion: number | null;
  fechaInicio: string;
  fechaCierre: string;
  fechaSorteo: string | null;
  estado: string;
}

export interface ResultadoPaginado<T> {
  registros: T[];
  pagina: number;
  tamanoPagina: number;
  totalRegistros: number;
  totalPaginas: number;
  tienePaginaAnterior: boolean;
  tienePaginaSiguiente: boolean;
}

export interface ApiResponse<T> {
  success: boolean;
  statusCode: number;
  message: string;
  data: T | null;
  errors: unknown;
}
