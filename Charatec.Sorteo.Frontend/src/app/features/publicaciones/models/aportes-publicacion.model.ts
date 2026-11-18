export type EstadoAporte =
    | 'Pendiente'
    | 'Confirmado'
    | 'Rechazado'
    | 'Reembolsado';

export interface AportePublicacion {
    aporteId: string;
    sorteoId: string;
    clienteId: string;
    nombreCliente: string;
    numeroWhatsapp: string | null;
    monto: number;
    codigoOperacionYape: string | null;
    estado: EstadoAporte;
    fechaCreacion: string;
}

export interface ListarAportesPublicacionRequest {
    pagina: number;
    tamanoPagina: number;
    estado: EstadoAporte | null;
}

export type DecisionAporte = 'Aprobar' | 'Rechazar';

export interface VerificarAporteRequest {
    decision: DecisionAporte;
    observacion: string | null;
}

export interface VerificarAporteResponse {
    aporteId: string;
    estado: string;
    monto: number;
    fechaRevision: string;
    mensaje: string;
}
