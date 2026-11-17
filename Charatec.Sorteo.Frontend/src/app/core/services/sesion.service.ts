
import { Injectable, signal } from '@angular/core';

import {
  UsuarioAutenticadoResponse
} from '../../features/auth/models/cliente-login-response.model';

interface SesionGuardada {
  accessToken: string;
  expiraEn: number;
  usuario: UsuarioAutenticadoResponse;
}

@Injectable({
  providedIn: 'root'
})
export class SesionService {

  private readonly storageKey = 'charatec_sesion';

  private readonly tokenState = signal<string | null>(null);

  private readonly usuarioState =
    signal<UsuarioAutenticadoResponse | null>(null);

  private readonly expiraEnState = signal<number | null>(null);

  readonly token = this.tokenState.asReadonly();
  readonly usuario = this.usuarioState.asReadonly();

  constructor() {
    this.restaurarSesion();
  }

  // ==========================================
  // 1. INICIAR SESIÓN
  // ==========================================

  iniciarSesion(
    accessToken: string,
    expiresIn: number,
    usuario: UsuarioAutenticadoResponse
  ): void {

    const expiraEn = Date.now() + expiresIn * 1000;

    const sesion: SesionGuardada = {
      accessToken,
      expiraEn,
      usuario
    };

    this.tokenState.set(accessToken);
    this.usuarioState.set(usuario);
    this.expiraEnState.set(expiraEn);

    if (typeof window !== 'undefined') {
      sessionStorage.setItem(
        this.storageKey,
        JSON.stringify(sesion)
      );
    }
  }

  // ==========================================
  // 2. RESTAURAR SESIÓN AL RECARGAR
  // ==========================================

  private restaurarSesion(): void {

    if (typeof window === 'undefined') {
      return;
    }

    const guardado = sessionStorage.getItem(this.storageKey);

    if (!guardado) {
      return;
    }

    try {
      const sesion = JSON.parse(guardado) as SesionGuardada;

      if (
        typeof sesion.accessToken !== 'string' ||
        !sesion.accessToken ||
        typeof sesion.expiraEn !== 'number' ||
        !Number.isFinite(sesion.expiraEn) ||
        !sesion.usuario ||
        !Array.isArray(sesion.usuario.roles)
      ) {
        this.cerrarSesion();
        return;
      }

      // Verificar vencimiento
      if (Date.now() >= sesion.expiraEn) {
        this.cerrarSesion();
        return;
      }

      // Restaurar Angular Signals
      this.tokenState.set(sesion.accessToken);
      this.usuarioState.set(sesion.usuario);
      this.expiraEnState.set(sesion.expiraEn);

    } catch {
      this.cerrarSesion();
    }
  }

  // ==========================================
  // 3. COMPROBAR AUTENTICACIÓN
  // ==========================================

  autenticado(): boolean {

    const token = this.tokenState();
    const expiraEn = this.expiraEnState();

    return (
      token !== null &&
      expiraEn !== null &&
      Date.now() < expiraEn
    );
  }

  // ==========================================
  // 4. OBTENER TOKEN
  // ==========================================

  obtenerToken(): string | null {

    if (!this.autenticado()) {
      this.cerrarSesion();
      return null;
    }

    return this.tokenState();
  }

  // ==========================================
  // 5. COMPROBAR ROLES
  // ==========================================

  tieneRol(rol: string): boolean {
    return this.autenticado() &&
      (this.usuarioState()?.roles.includes(rol) ?? false);
  }

  // ==========================================
  // 6. CERRAR SESIÓN
  // ==========================================

  cerrarSesion(): void {

    this.tokenState.set(null);
    this.usuarioState.set(null);
    this.expiraEnState.set(null);

    if (typeof window !== 'undefined') {
      sessionStorage.removeItem(this.storageKey);
    }
  }
}
