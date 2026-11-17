
import { Component, inject, signal } from '@angular/core';

import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import { HttpErrorResponse } from '@angular/common/http';

import { Router, RouterLink } from '@angular/router';

import { AuthService } from '../../services/auth.service';
import { SesionService } from '../../../../core/services/sesion.service';

import { ApiResponse } from '../../../../core/models/api-response.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {

  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly sesionService = inject(SesionService);
  private readonly router = inject(Router);


  readonly cargando = signal(false);
  readonly mensajeError = signal('');
  readonly mostrarPassword = signal(false);

  readonly formulario = this.fb.nonNullable.group({
    email: [
      '',
      [
        Validators.required,
        Validators.email
      ]
    ],
    password: [
      '',
      [
        Validators.required
      ]
    ]
  });

  // =========================================
  // MOSTRAR / OCULTAR CONTRASEÑA
  // =========================================

  alternarPassword(): void {
    this.mostrarPassword.update(valor => !valor);
  }

  // =========================================
  // INICIAR SESIÓN
  // =========================================

  iniciarSesion(): void {

    this.mensajeError.set('');

    // Verificar campos
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    // Evitar peticiones duplicadas
    if (this.cargando()) {
      return;
    }

    this.cargando.set(true);

    const request = this.formulario.getRawValue();

    // Consumir endpoint de login
    this.authService.login(request).subscribe({

      next: (response) => {

        this.cargando.set(false);

        if (!response.success || !response.data) {
          this.mensajeError.set(
            response.message || 'No se pudo iniciar sesión.'
          );
          return;
        }

        // Guardar sesión en memoria
        this.sesionService.iniciarSesion(
          response.data.accessToken,
          response.data.expiresIn,
          response.data.usuario
        );

        // Limpiar contraseña del formulario
        this.formulario.controls.password.reset();

        // Redirigir a página privada
        void this.router.navigateByUrl('/home');
      },

      error: (error: HttpErrorResponse) => {

        this.cargando.set(false);

        const response =
          error.error as Partial<ApiResponse<unknown>> | null;

        if (error.status === 0) {
          this.mensajeError.set(
            'No se pudo conectar con el servidor.'
          );
          return;
        }

        if (error.status === 401) {
          this.mensajeError.set(
            'Correo electrónico o contraseña incorrectos.'
          );
          return;
        }

        if (error.status === 423) {
          this.mensajeError.set(
            'Tu cuenta está bloqueada temporalmente. Intenta más tarde.'
          );
          return;
        }

        if (error.status === 429) {
          this.mensajeError.set(
            'Demasiados intentos. Intenta nuevamente más tarde.'
          );
          return;
        }

        this.mensajeError.set(
          response?.message || 'Ocurrió un error al iniciar sesión.'
        );
      }
    });
  }
}
