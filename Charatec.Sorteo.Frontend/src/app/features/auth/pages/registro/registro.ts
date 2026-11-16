import { Component, inject, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';

import { AuthService } from '../../services/auth.service';
import { ApiResponse } from '../../../../core/models/api-response.model';

@Component({
  selector: 'app-registro',
  imports: [ReactiveFormsModule],
  templateUrl: './registro.html',
  styleUrl: './registro.css'
})
export class Registro {

  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);

  readonly cargando = signal(false);
  readonly mensajeExito = signal('');
  readonly mensajeError = signal('');
  readonly errores = signal<string[]>([]);

  readonly formulario = this.formBuilder.nonNullable.group({
    nombres: ['', [
      Validators.required,
      Validators.minLength(2),
      Validators.maxLength(100)
    ]],

    apellidos: ['', [
      Validators.required,
      Validators.minLength(2),
      Validators.maxLength(100)
    ]],

    email: ['', [
      Validators.required,
      Validators.email,
      Validators.maxLength(254)
    ]],

    telefono: ['', [
      Validators.required,
      Validators.pattern(/^9\d{8}$/)
    ]],

    password: ['', [
      Validators.required,
      Validators.minLength(6)
    ]]
  });

  registrar(): void {

    this.mensajeExito.set('');
    this.mensajeError.set('');
    this.errores.set([]);

    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    if (this.cargando()) {
      return;
    }

    this.cargando.set(true);

    const request = this.formulario.getRawValue();

    this.authService.registrar(request).subscribe({

      next: (response) => {
        this.cargando.set(false);

        if (response.success) {
          this.mensajeExito.set(response.message);
          this.formulario.reset();
        } else {
          this.mensajeError.set(response.message);
        }
      },

      error: (error: HttpErrorResponse) => {
        this.cargando.set(false);

        const response =
          error.error as Partial<ApiResponse<unknown>> | null;

        this.mensajeError.set(
          typeof response?.message === 'string'
            ? response.message
            : 'No se pudo completar el registro.'
        );

        if (response?.errors) {
          this.errores.set(
            Object.values(response.errors)
              .filter(Array.isArray)
              .flat()
          );
        }
      }
    });
  }
}
