
import {
  Component,
  OnInit,
  OnDestroy,
  computed,
  inject,
  signal
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';

import {
  ParticiparService
} from '../../services/participar.service';

import {
  PrepararParticipacionResponse
} from '../../models/preparar-participacion.model';

@Component({
  selector: 'app-participar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './participar.html',
  styleUrl: './participar.css'
})
export class Participar implements OnInit, OnDestroy {

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  private readonly participarService =
    inject(ParticiparService);

  private peticion?: Subscription;
  private parametros?: Subscription;

  // ==========================================
  // INFORMACIÓN DEL SORTEO
  // ==========================================

  readonly cargando = signal(true);

  readonly error = signal<string | null>(null);

  readonly sorteoId = signal<string>('');

  readonly configuracion =
    signal<PrepararParticipacionResponse | null>(null);

  // ==========================================
  // MONTO SELECCIONADO
  // ==========================================

  readonly montoSeleccionado = signal<number>(0);

  readonly porcentajeMeta = computed(() => {

    const datos = this.configuracion();

    if (!datos?.metaRecaudacion ||
        datos.metaRecaudacion <= 0) {
      return null;
    }

    return (
      this.montoSeleccionado() /
      datos.metaRecaudacion
    ) * 100;
  });

  readonly porcentajeBarra = computed(() => {
    return Math.min(
      100,
      Math.max(0, this.porcentajeMeta() ?? 0)
    );
  });

  // ==========================================
  // DATOS PARA EL FUTURO POST
  // ==========================================

  numeroWhatsapp = '';
  codigoOperacionYape = '';
  emailContacto = '';

  // ==========================================
  // CICLO DE VIDA
  // ==========================================

  ngOnInit(): void {

    this.parametros = this.route.paramMap.subscribe(
      params => {

        const id = params.get('sorteoId');

        if (!id) {
          this.error.set('No se recibió el ID del sorteo.');
          this.cargando.set(false);
          return;
        }

        this.sorteoId.set(id);

        this.cargarConfiguracion(id);
      }
    );
  }

  ngOnDestroy(): void {
    this.peticion?.unsubscribe();
    this.parametros?.unsubscribe();
  }

  // ==========================================
  // CONSULTAR API
  // ==========================================

  cargarConfiguracion(id: string): void {

    this.peticion?.unsubscribe();

    this.cargando.set(true);
    this.error.set(null);
    this.configuracion.set(null);

    this.peticion = this.participarService
      .obtenerConfiguracion(id)
      .subscribe({

        next: respuesta => {

          if (respuesta.success && respuesta.data) {

            const datos = respuesta.data;

            this.configuracion.set(datos);

            // El backend establece el monto inicial.
            this.montoSeleccionado.set(
              datos.montoInicial
            );

            // Precargar correo disponible.
            this.emailContacto = datos.email ?? '';

          } else {

            this.error.set(
              respuesta.message ||
              'No se pudo obtener la información del sorteo.'
            );
          }

          this.cargando.set(false);
        },

        error: error => {

          if (error.status === 401) {

            this.error.set(
              'Tu sesión expiró. Inicia sesión nuevamente.'
            );

          } else if (error.status === 403) {

            this.error.set(
              'No tienes permisos para participar.'
            );

          } else if (error.status === 404) {

            this.error.set(
              'El sorteo no existe o no está publicado.'
            );

          } else {

            this.error.set(
              'Ocurrió un error al consultar el sorteo.'
            );
          }

          this.cargando.set(false);
        }
      });
  }

  // ==========================================
  // CAMBIAR MONTO DEL DESLIZADOR
  // ==========================================

  cambiarMonto(valor: number | string): void {

    const datos = this.configuracion();

    if (!datos || !datos.puedeParticipar) {
      return;
    }

    const monto = Number(valor);

    if (!Number.isFinite(monto)) {
      return;
    }

    const minimo = datos.aporteMinimo;
    const maximo = datos.aporteMaximo;

    const resultado = Math.min(
      maximo,
      Math.max(minimo, monto)
    );

    this.montoSeleccionado.set(
      Math.round(resultado * 100) / 100
    );
  }

  // ==========================================
  // REGRESAR A SORTEOS DISPONIBLES
  // ==========================================

  volver(): void {
    this.router.navigate(['/sorteos']);
  }
}
