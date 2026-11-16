
export interface UsuarioAutenticadoResponse {
  id: string;
  email: string;
  nombres: string | null;
  apellidos: string | null;
  roles: string[];
}

export interface ClienteLoginResponse {
  accessToken: string;
  tokenType: string;
  expiresIn: number;
  usuario: UsuarioAutenticadoResponse;
}
