
import {
  Component,
  OnInit,
  OnDestroy,
  computed,
  inject,
  signal
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';

import { MisAportesService } from '../../services/mis-aportes.service';

import {
  AporteListado,
  EstadoAporte
} from '../../models/mis-aportes.model';

@Component({
  selector: 'app-mis-aportes',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './mis-aportes.html',
  styleUrl: './mis-aportes.css'
})
export class MisAportes implements OnInit, OnDestroy {

  private readonly misAportesService =
    inject(MisAportesService);

  private peticion?: Subscription;

  // ==========================================
  // ESTADOS GENERALES
  // ==========================================

  readonly cargando = signal(false);
  readonly error = signal<string | null>(null);

  readonly aportes = signal<AporteListado[]>([]);

  // ==========================================
  // FILTROS Y PAGINACIÓN
  // ==========================================

  readonly estado = signal<EstadoAporte | null>(null);

  readonly pagina = signal(1);
  readonly tamanoPagina = signal(10);

  readonly totalRegistros = signal(0);
  readonly totalPaginas = signal(0);

  readonly tieneAnterior = computed(
    () => this.pagina() > 1
  );

  readonly tieneSiguiente = computed(
    () => this.pagina() < this.totalPaginas()
  );

  readonly paginasVisibles = computed(() => {

    const total = this.totalPaginas();
    const actual = this.pagina();

    if (total <= 0) {
      return [];
    }

    const inicio = Math.max(
      1,
      Math.min(actual - 2, total - 4)
    );

    const fin = Math.min(
      total,
      inicio + 4
    );

    return Array.from(
      { length: fin - inicio + 1 },
      (_, indice) => inicio + indice
    );
  });

  // ==========================================
  // CICLO DE VIDA
  // ==========================================

  ngOnInit(): void {
    this.cargarAportes();
  }

  ngOnDestroy(): void {
    this.peticion?.unsubscribe();
  }

  // ==========================================
  // GET: LISTAR APORTES
  // ==========================================

  cargarAportes(): void {

    this.peticion?.unsubscribe();

    this.cargando.set(true);
    this.error.set(null);

    this.peticion = this.misAportesService
      .listarMisAportes({
        pagina: this.pagina(),
        tamanoPagina: this.tamanoPagina(),
        estado: this.estado()
      })
      .subscribe({

        next: respuesta => {

          if (respuesta.success && respuesta.data) {

            const datos = respuesta.data;

            this.aportes.set(datos.registros);

            this.pagina.set(datos.pagina);

            this.tamanoPagina.set(
              datos.tamanoPagina
            );

            this.totalRegistros.set(
              datos.totalRegistros
            );

            this.totalPaginas.set(
              datos.totalPaginas
            );

          } else {

            this.aportes.set([]);
            this.totalRegistros.set(0);
            this.totalPaginas.set(0);

            this.error.set(
              respuesta.message ||
              'No se pudo obtener tu historial.'
            );
          }

          this.cargando.set(false);
        },

        error: err => {

          this.aportes.set([]);
          this.totalRegistros.set(0);
          this.totalPaginas.set(0);

          if (err.status === 401) {

            this.error.set(
              'Tu sesión no es válida o ha expirado.'
            );

          } else if (err.status === 403) {

            this.error.set(
              'No tienes permiso para consultar estos aportes.'
            );

          } else {

            this.error.set(
              err.error?.message ||
              'Ocurrió un error al consultar tu historial.'
            );
          }

          this.cargando.set(false);
        }

      });
  }

  // ==========================================
  // FILTRAR POR ESTADO
  // ==========================================

  cambiarEstado(event: Event): void {

    const elemento =
      event.target as HTMLSelectElement;

    const valor = elemento.value;

    this.estado.set(
      valor
        ? valor as EstadoAporte
        : null
    );

    this.pagina.set(1);

    this.cargarAportes();
  }

  // ==========================================
  // CAMBIAR REGISTROS POR PÁGINA
  // ==========================================

  cambiarTamano(event: Event): void {

    const elemento =
      event.target as HTMLSelectElement;

    const cantidad = Number(elemento.value);

    if (![5, 10, 20, 50].includes(cantidad)) {
      return;
    }

    this.tamanoPagina.set(cantidad);
    this.pagina.set(1);

    this.cargarAportes();
  }

  // ==========================================
  // CAMBIAR PÁGINA
  // ==========================================

  irPagina(numero: number): void {

    if (this.cargando() ||
        numero < 1 ||
        numero > this.totalPaginas() ||
        numero === this.pagina()) {
      return;
    }

    this.pagina.set(numero);

    this.cargarAportes();
  }

  // ==========================================
  // TEXTO AMIGABLE PARA ESTADO
  // ==========================================

  textoEstado(estado: EstadoAporte): string {

    switch (estado) {

      case 'Pendiente':
        return 'Pendiente de revisión';

      case 'Confirmado':
        return 'Confirmado';

      case 'Rechazado':
        return 'Rechazado';

      case 'Reembolsado':
        return 'Reembolsado';

      default:
        return estado;
    }
  }
}
