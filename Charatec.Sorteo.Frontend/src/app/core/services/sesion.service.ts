
import { Injectable, computed, signal } from '@angular/core';

import {
  UsuarioAutenticadoResponse
} from '../../features/auth/models/cliente-login-response.model';

@Injectable({
  providedIn: 'root'
})
export class SesionService {

  private readonly tokenState = signal<string | null>(null);

  private readonly usuarioState =
    signal<UsuarioAutenticadoResponse | null>(null);

  private readonly expiraEnState = signal<number | null>(null);

  // Estados públicos de solo lectura
  readonly token = this.tokenState.asReadonly();

  readonly usuario = this.usuarioState.asReadonly();

  readonly autenticado = computed(() => {
    const token = this.tokenState();
    const expiraEn = this.expiraEnState();

    return token !== null &&
      expiraEn !== null &&
      Date.now() < expiraEn;
  });

  // =========================================
  // GUARDAR SESIÓN
  // =========================================

  iniciarSesion(
    accessToken: string,
    expiresIn: number,
    usuario: UsuarioAutenticadoResponse
  ): void {

    const expiraEn = Date.now() + expiresIn * 1000;

    this.tokenState.set(accessToken);
    this.usuarioState.set(usuario);
    this.expiraEnState.set(expiraEn);
  }

  // =========================================
  // OBTENER TOKEN VÁLIDO
  // =========================================

  obtenerToken(): string | null {

    if (!this.autenticado()) {
      return null;
    }

    return this.tokenState();
  }

  // =========================================
  // VERIFICAR ROLES
  // =========================================

  tieneRol(rol: string): boolean {
    return this.autenticado() &&
      (this.usuarioState()?.roles.includes(rol) ?? false);
  }

  // =========================================
  // CERRAR SESIÓN
  // =========================================

  cerrarSesion(): void {
    this.tokenState.set(null);
    this.usuarioState.set(null);
    this.expiraEnState.set(null);
  }
}
