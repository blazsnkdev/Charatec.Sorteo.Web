
export interface RegistrarParticipacionRequest {
  monto: number;
  numeroWhatsapp: string;
  email: string;
  codigoOperacionYape: string;
  aceptaTerminos: boolean;
  solicitudId: string;
}

export interface RegistrarParticipacionResponse {
  participacionId: string;
  sorteoId: string;
  ordenPagoId: string;
  aporteId: string;
  monto: number;
  moneda: string;
  estadoAporte: string;
  estadoVerificacion: string;
  mensaje: string;
}
