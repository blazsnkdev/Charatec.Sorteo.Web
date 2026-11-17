
import {
  Component,
  OnInit,
  OnDestroy,
  computed,
  inject,
  signal,
  HostListener
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { SorteosDisponiblesService } from '../services/sorteos-disponibles.service';
import { SorteoDisponible } from '../models/sorteos-disponibles';
import { ResultadoPaginado } from '../models/sorteos-disponibles';



@Component({
  selector: 'app-sorteos-disponibles',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './sorteos-disponibles.html',
  styleUrl: './sorteos-disponibles.css'
})
export class SorteosDisponibles implements OnInit, OnDestroy {

  private readonly sorteosService =
    inject(SorteosDisponiblesService);

  private peticion?: Subscription;

  // =====================================
  // ESTADO DE LA VISTA
  // =====================================

  readonly cargando = signal(false);
  readonly error = signal<string | null>(null);

  readonly pagina = signal(1);
  readonly tamanoPagina = signal(9);

  readonly resultado =
    signal<ResultadoPaginado<SorteoDisponible> | null>(null);

  readonly sorteos = computed(
    () => this.resultado()?.registros ?? []
  );

  readonly totalRegistros = computed(
    () => this.resultado()?.totalRegistros ?? 0
  );

  readonly totalPaginas = computed(
    () => this.resultado()?.totalPaginas ?? 0
  );

  readonly tieneAnterior = computed(
    () => this.resultado()?.tienePaginaAnterior ?? false
  );

  readonly tieneSiguiente = computed(
    () => this.resultado()?.tienePaginaSiguiente ?? false
  );

  readonly paginasVisibles = computed(() => {
    const actual = this.pagina();
    const total = this.totalPaginas();

    if (total === 0) {
      return [];
    }

    const inicio = Math.max(
      1,
      Math.min(actual - 2, Math.max(1, total - 4))
    );

    const fin = Math.min(total, inicio + 4);

    return Array.from(
      { length: fin - inicio + 1 },
      (_, i) => inicio + i
    );
  });

  // =====================================
  // MODAL DE DETALLES
  // =====================================

  readonly sorteoSeleccionado =
    signal<SorteoDisponible | null>(null);

  readonly modalAbierto = signal(false);

  // =====================================
  // CICLO DE VIDA
  // =====================================

  ngOnInit(): void {
    this.cargarSorteos();
  }

  ngOnDestroy(): void {
    this.peticion?.unsubscribe();
  }

  // =====================================
  // CONSULTAR API
  // =====================================

  cargarSorteos(): void {

    this.peticion?.unsubscribe();

    this.cargando.set(true);
    this.error.set(null);

    this.peticion = this.sorteosService.listar(
      this.pagina(),
      this.tamanoPagina()
    ).subscribe({

      next: (respuesta) => {

        if (respuesta.success && respuesta.data) {

          this.resultado.set(respuesta.data);

          if (
            respuesta.data.totalPaginas > 0 &&
            this.pagina() > respuesta.data.totalPaginas
          ) {
            this.pagina.set(respuesta.data.totalPaginas);
            this.cargarSorteos();
            return;
          }

        } else {
          this.resultado.set(null);

          this.error.set(
            respuesta.message ||
            'No se pudieron obtener los sorteos.'
          );
        }

        this.cargando.set(false);
      },

      error: (error) => {

        this.resultado.set(null);

        if (error.status === 401) {

          this.error.set(
            'Tu sesión expiró. Inicia sesión nuevamente.'
          );

        } else if (error.status === 403) {

          this.error.set(
            'No tienes permisos para ver los sorteos.'
          );

        } else {

          this.error.set(
            'No se pudieron cargar los sorteos. Inténtalo nuevamente.'
          );
        }

        this.cargando.set(false);
      }

    });
  }

  // =====================================
  // PAGINACIÓN
  // =====================================

  irPagina(numero: number): void {

    if (
      numero < 1 ||
      numero > this.totalPaginas() ||
      numero === this.pagina() ||
      this.cargando()
    ) {
      return;
    }

    this.pagina.set(numero);
    this.cargarSorteos();
  }

  // =====================================
  // DISPONIBILIDAD POR FECHAS
  // =====================================

  estadoFechas(sorteo: SorteoDisponible): string {

    const ahora = Date.now();
    const inicio = new Date(sorteo.fechaInicio).getTime();
    const cierre = new Date(sorteo.fechaCierre).getTime();

    if (!Number.isFinite(inicio) ||
        !Number.isFinite(cierre)) {
      return 'Consultar fechas';
    }

    if (ahora < inicio) {
      return 'Próximamente';
    }

    if (ahora >= cierre) {
      return 'Plazo finalizado';
    }

    return 'En curso';
  }

  // =====================================
  // MODAL
  // =====================================

  verDetalles(sorteo: SorteoDisponible): void {

    this.sorteoSeleccionado.set(sorteo);
    this.modalAbierto.set(true);
  }

  cerrarDetalles(): void {

    this.modalAbierto.set(false);
    this.sorteoSeleccionado.set(null);
  }

  @HostListener('document:keydown.escape')
  cerrarConEscape(): void {

    if (this.modalAbierto()) {
      this.cerrarDetalles();
    }
  }
}
