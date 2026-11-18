
export type EstadoAporte =
  | 'Pendiente'
  | 'Confirmado'
  | 'Rechazado'
  | 'Reembolsado';

export interface AporteListado {
  id: string;
  sorteoId: string;
  tituloSorteo: string;
  monto: number;
  estado: EstadoAporte;
  fechaCreacion: string;
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

export interface ListarMisAportesParametros {
  pagina: number;
  tamanoPagina: number;
  estado: EstadoAporte | null;
}
