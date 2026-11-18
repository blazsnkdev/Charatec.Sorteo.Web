
import {
  Component,
  OnInit,
  OnDestroy,
  computed,
  inject,
  signal
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { AportesPublicacionService } from '../../services/aportes-publicacion.service';

import { AportePublicacion } from '../../models/aportes-publicacion.model';
import { EstadoAporte } from '../../models/aportes-publicacion.model';



@Component({
  selector: 'app-aportes-publicacion',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './aportes-publicacion.html',
  styleUrl: './aportes-publicacion.css'
})
export class AportesPublicacion implements OnInit, OnDestroy {

  private readonly service =
    inject(AportesPublicacionService);

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  private peticion?: Subscription;
  private parametros?: Subscription;

  // ==========================================
  // PUBLICACIÓN
  // ==========================================

  readonly sorteoId = signal('');

  // ==========================================
  // LISTADO
  // ==========================================

  readonly aportes = signal<AportePublicacion[]>([]);

  readonly cargando = signal(false);

  readonly error = signal<string | null>(null);

  // ==========================================
  // FILTROS Y PAGINACIÓN
  // ==========================================

  readonly estado = signal<EstadoAporte | null>(null);

  readonly pagina = signal(1);
  readonly tamanoPagina = signal(10);

  readonly totalRegistros = signal(0);
  readonly totalPaginas = signal(0);

  readonly tienePaginaAnterior = computed(
    () => this.pagina() > 1
  );

  readonly tienePaginaSiguiente = computed(
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
  // INICIO
  // ==========================================

  ngOnInit(): void {

    this.parametros = this.route.paramMap.subscribe(
      params => {

        const id = params.get('sorteoId');

        if (!id) {
          this.error.set('No se recibió el ID de la publicación.');
          return;
        }

        this.peticion?.unsubscribe();

        this.sorteoId.set(id);
        this.estado.set(null);
        this.pagina.set(1);
        this.aportes.set([]);

        this.cargarAportes();
      }
    );
  }

  ngOnDestroy(): void {
    this.peticion?.unsubscribe();
    this.parametros?.unsubscribe();
  }

  // ==========================================
  // GET: CARGAR APORTES
  // ==========================================

  cargarAportes(): void {

    const id = this.sorteoId();

    if (!id) {
      return;
    }

    this.peticion?.unsubscribe();

    this.cargando.set(true);
    this.error.set(null);

    this.peticion = this.service
      .listar(id, {
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
            this.tamanoPagina.set(datos.tamanoPagina);

            this.totalRegistros.set(datos.totalRegistros);
            this.totalPaginas.set(datos.totalPaginas);

          } else {

            this.limpiarListado();

            this.error.set(
              respuesta.message ||
              'No se pudo cargar el listado de aportes.'
            );
          }

          this.cargando.set(false);
        },

        error: err => {

          this.limpiarListado();

          if (err.status === 401) {

            this.error.set(
              'Tu sesión ha expirado. Inicia sesión nuevamente.'
            );

          } else if (err.status === 403) {

            this.error.set(
              'No tienes permisos para consultar estos aportes.'
            );

          } else if (err.status === 404) {

            this.error.set(
              'No se encontró la publicación o el endpoint.'
            );

          } else {

            this.error.set(
              err.error?.message ||
              'Ocurrió un error al consultar los aportes.'
            );
          }

          this.cargando.set(false);
        }
      });
  }

  private limpiarListado(): void {
    this.aportes.set([]);
    this.totalRegistros.set(0);
    this.totalPaginas.set(0);
  }

  // ==========================================
  // CAMBIAR ESTADO
  // ==========================================

  cambiarEstado(event: Event): void {

    const elemento = event.target as HTMLSelectElement;
    const valor = elemento.value;

    this.estado.set(
      valor ? valor as EstadoAporte : null
    );

    this.pagina.set(1);
    this.cargarAportes();
  }

  // ==========================================
  // CAMBIAR TAMAÑO DE PÁGINA
  // ==========================================

  cambiarTamano(event: Event): void {

    const elemento = event.target as HTMLSelectElement;
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
  // MOSTRAR ESTADO AMIGABLE
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

  // ==========================================
  // REGRESAR A PUBLICACIONES
  // ==========================================

  volver(): void {
    this.router.navigate(['/publicaciones']);
  }
}
