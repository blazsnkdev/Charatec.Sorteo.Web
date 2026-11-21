export interface ClienteRuleta {
  clienteId: string;
  nombreCliente: string;
  totalAportado: number;
  fechaUltimaAprobacion: string | null;
}

export interface RuletaDatos {
  sorteoId: string;
  tituloSorteo: string;
  estado: string;
  fechaInicio: string;
  fechaSorteo: string | null;
  fechaServidor: string;
  puedeSortear: boolean;
  totalConfirmado: number;
  totalClientes: number;
  clientes: ClienteRuleta[];
}

export interface ParticipanteRuleta extends ClienteRuleta {
  probabilidad: number;
  pesoCentavos: number;
}
