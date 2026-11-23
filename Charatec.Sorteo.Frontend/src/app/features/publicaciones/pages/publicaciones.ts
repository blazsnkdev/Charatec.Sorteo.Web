import {
Component,
OnInit,
OnDestroy,
HostListener,
computed,
inject,
signal

} from '@angular/core';

import { Router } from '@angular/router';

import { CommonModule } from '@angular/common';

import { FormsModule } from '@angular/forms';

import { Subscription, firstValueFrom } from 'rxjs';

import { PublicacionesService } from '../services/publicaciones.service';

import { SorteoListado } from '../models/sorteo-listado.model';

import { CrearSorteoRequest } from '../models/crear-sorteo-request.model';

import { ActualizarSorteoRequest } from '../models/actualizar-sorteo-request.model';

import { ResultadoPaginado } from '../../../core/models/resultado-paginado.model';

@Component({

  selector: 'app-publicaciones',

  standalone: true,

  imports: [CommonModule, FormsModule],

  templateUrl: './publicaciones.html',

  styleUrl: './publicaciones.css'

})

export class Publicaciones implements OnInit, OnDestroy {

  readonly publicacionesService = inject(PublicacionesService);
  private peticion?: Subscription;
  private peticionDetalle?: Subscription;
  private peticionCrear?: Subscription;

  //* ==========================================*

  //* 1. LISTADO GENERAL DE SORTEOS*

  //* ==========================================*

  readonly cargando = signal(false);
readonly confirmarPublicacion = signal(false);
  readonly error = signal<string | null>(null);

  private readonly router = inject(Router);

  readonly estado = signal<number | null>(null);

  readonly pagina = signal(1);

  readonly tamanoPagina = signal(10);

  readonly resultado =

    signal<ResultadoPaginado<SorteoListado> | null>(null);

  readonly sorteos = computed(

    () => this.resultado()?.registros ?? []

  );

  readonly totalPaginas = computed(

    () => this.resultado()?.totalPaginas ?? 0

  );

  readonly totalRegistros = computed(

    () => this.resultado()?.totalRegistros ?? 0

  );

  readonly paginasVisibles = computed(() => {

    const total = this.totalPaginas();

    const actual = this.pagina();

    if (total === 0) {

      return [];

    }

    const inicio = Math.max(

      1,

      Math.min(actual - 2, total - 4)

    );

    const fin = Math.min(total, inicio + 4);

    return Array.from(

      { length: fin - inicio + 1 },

      (_, indice) => inicio + indice

    );

  });


  readonly modalAbierto = signal(false);

  readonly detalleSorteo =

    signal<SorteoListado | null>(null);

  readonly cargandoDetalle = signal(false);

  readonly procesandoDetalle = signal(false);

  readonly errorDetalle = signal<string | null>(null);

  readonly mensajeDetalle = signal<string | null>(null);

  formularioDetalle = this.formularioDetalleInicial();

  private formularioDetalleOriginal = '';

  private formularioDetalleInicial() {

    return {

      titulo: '',

      descripcion: '',

      aporteMinimo: 0,

      metaRecaudacion: null as number | null,

      fechaInicio: '',

      fechaCierre: '',

      fechaSorteo: ''

    };

  }
  
abrirModalPortada(sorteo: SorteoListado): void {
    this.limpiarVistaPreviaPortada();

    this.sorteoPortada.set(sorteo);
    this.archivoPortada.set(null);
    this.errorPortada.set(null);
    this.modalPortadaAbierto.set(true);
  }

  seleccionarImagenPortada(event: Event): void {
    const input = event.target as HTMLInputElement;
    const archivo = input.files?.[0] ?? null;

    this.limpiarVistaPreviaPortada();
    this.archivoPortada.set(null);
    this.errorPortada.set(null);

    if (!archivo) return;

    const tiposPermitidos = [
      'image/jpeg',
      'image/png',
      'image/webp'
    ];

    if (!tiposPermitidos.includes(archivo.type)) {
      this.errorPortada.set(
        'Solo se permiten imágenes JPG, PNG o WEBP.'
      );
      input.value = '';
      return;
    }

    if (archivo.size === 0 ||
        archivo.size > 2 * 1024 * 1024) {
      this.errorPortada.set(
        'La imagen debe pesar como máximo 2 MB.'
      );
      input.value = '';
      return;
    }

    this.archivoPortada.set(archivo);
    this.vistaPreviaPortada.set(
      URL.createObjectURL(archivo)
    );
  }

  guardarImagenPortada(): void {
    const sorteo = this.sorteoPortada();
    const archivo = this.archivoPortada();

    if (!sorteo || !archivo || this.guardandoPortada()) {
      return;
    }

    this.guardandoPortada.set(true);
    this.errorPortada.set(null);

    this.peticionPortada = this.publicacionesService
      .actualizarPortada(sorteo.id, archivo)
      .subscribe({
        next: (respuesta) => {
          if (!respuesta.success || !respuesta.data) {
            this.errorPortada.set(
              respuesta.message ||
              'No se pudo guardar la imagen.'
            );
            this.guardandoPortada.set(false);
            return;
          }

          const nuevaRuta =
            respuesta.data.imagenPortadaUrl;

          // Actualizar la tabla sin volver a consultarla.
          this.resultado.update(actual => {
            if (!actual) return actual;

            return {
              ...actual,
              registros: actual.registros.map(item =>
                item.id === sorteo.id
                  ? { ...item, imagenPortadaUrl: nuevaRuta }
                  : item
              )
            };
          });

          // Actualizar detalle si estaba consultado.
          this.detalleSorteo.update(actual =>
            actual?.id === sorteo.id
              ? { ...actual, imagenPortadaUrl: nuevaRuta }
              : actual
          );

          this.guardandoPortada.set(false);
          this.cerrarModalPortada();
        },
        error: (error: unknown) => {
          this.errorPortada.set(
            this.obtenerMensajeError(
              error,
              'Ocurrió un error al subir la imagen.'
            )
          );

          this.guardandoPortada.set(false);
        }
      });
  }

  cerrarModalPortada(): void {
    if (this.guardandoPortada()) return;

    this.limpiarVistaPreviaPortada();
    this.modalPortadaAbierto.set(false);
    this.sorteoPortada.set(null);
    this.archivoPortada.set(null);
    this.errorPortada.set(null);
  }

  private limpiarVistaPreviaPortada(): void {
    const url = this.vistaPreviaPortada();

    if (url) {
      URL.revokeObjectURL(url);
    }

    this.vistaPreviaPortada.set(null);
  }
  get detalleModificado(): boolean {

    return JSON.stringify(this.formularioDetalle) !==

      this.formularioDetalleOriginal;

  }

  get detalleEditable(): boolean {

    return this.detalleSorteo()?.estado === 'Borrador';

  }

  //* ==========================================*

  //* 3. MODAL CREAR SORTEO*

  //* ==========================================*

  readonly modalCrearAbierto = signal(false);

  readonly guardandoSorteo = signal(false);

  readonly errorCrear = signal<string | null>(null);

  readonly mensajeCrear = signal<string | null>(null);
readonly modalPortadaAbierto = signal(false);
  readonly sorteoPortada = signal<SorteoListado | null>(null);
  readonly archivoPortada = signal<File | null>(null);
  readonly vistaPreviaPortada = signal<string | null>(null);
  readonly guardandoPortada = signal(false);
  readonly errorPortada = signal<string | null>(null);

  private peticionPortada?: Subscription;
  formularioCrear = this.formularioInicial();

  private formularioInicial() {

    return {

      titulo: '',

      descripcion: '',

      aporteMinimo: 2,

      metaRecaudacion: null as number | null,

      fechaInicio: '',

      fechaCierre: '',

      fechaSorteo: ''

    };

  }

  //* ==========================================*

  //* 4. CICLO DE VIDA*

  //* ==========================================*

  ngOnInit(): void {

    this.cargarSorteos();

  }

  ngOnDestroy(): void {

    this.peticion?.unsubscribe();
    this.peticionDetalle?.unsubscribe();
    this.peticionCrear?.unsubscribe();
    this.peticionPortada?.unsubscribe();
  this.limpiarVistaPreviaPortada();

  }

  //* ==========================================*

  //* 5. CONSULTAR SORTEOS PAGINADOS*

  //* ==========================================*

  cargarSorteos(): void {

    this.peticion?.unsubscribe();

    this.cargando.set(true);

    this.error.set(null);

    this.peticion = this.publicacionesService.listar(

      this.pagina(),

      this.tamanoPagina(),

      this.estado()

    ).subscribe({

      next: (respuesta) => {

        if (respuesta.success && respuesta.data) {

          this.resultado.set(respuesta.data);

          //* Si la página actual quedó fuera de rango*

          //* después de un cambio en los registros,*

          //* regresar a la última página disponible.*

          const total = respuesta.data.totalPaginas;

          if (

            total > 0 &&

            this.pagina() > total

          ) {

            this.pagina.set(total);

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

            'No tienes permisos para consultar los sorteos.'

          );

        } else {

          this.error.set(

            'Ocurrió un error al consultar los sorteos.'

          );

        }

        this.cargando.set(false);

      }

    });

  }

  verAportes(sorteoId: string): void {

    //* Cerrar el modal de publicación.*

    this.cerrarDetalle();

    //* Enviar el ID del sorteo seleccionado.*

    this.router.navigate([

      '/publicaciones',

      sorteoId,

      'aportes'

    ]);

  }

  //* ==========================================*

  //* 6. FILTROS Y PAGINACIÓN*

  //* ==========================================*

  cambiarEstado(event: Event): void {

    const valor =

      (event.target as HTMLSelectElement).value;

    this.estado.set(

      valor === '' ? null : Number(valor)

    );

    this.pagina.set(1);

    this.cargarSorteos();

  }

  cambiarTamano(event: Event): void {

    const valor = Number(

      (event.target as HTMLSelectElement).value

    );

    this.tamanoPagina.set(valor);

    this.pagina.set(1);

    this.cargarSorteos();

  }

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

  etiquetaEstado(estado: string): string {

    return estado;

  }

  //* ==========================================*

  //* 7. CONVERTIR FECHAS PARA LOS FORMULARIOS*

  //* ==========================================*

  //* Convierte fecha UTC de la API al horario*

  //* de Perú para input datetime-local.*

  private fechaParaInput(

    valor: string | null

  ): string {

    if (!valor) {

      return '';

    }

    const fecha = new Date(valor);

    if (Number.isNaN(fecha.getTime())) {

      return '';

    }

    const partes = new Intl.DateTimeFormat('en-GB', {

      timeZone: 'America/Lima',

      year: 'numeric',

      month: '2-digit',

      day: '2-digit',

      hour: '2-digit',

      minute: '2-digit',

      hourCycle: 'h23'

    }).formatToParts(fecha);

    const obtener = (tipo: string): string =>

      partes.find(

        parte => parte.type === tipo

      )?.value ?? '';

    return (

      `${obtener('year')}-${obtener('month')}-${obtener('day')}` +

      `T${obtener('hour')}:${obtener('minute')}`

    );

  }

  //* Convierte datetime-local al formato ISO*

  //* que espera tu API ASP.NET Core.*

  private fechaPeru(valor: string): string {

    //* Ejemplo de entrada:*

    //* 2026-12-01T10:00*

    //

    //* Ejemplo de salida:*

    //* 2026-12-01T10:00:00-05:00*

    return `${valor.slice(0, 16)}:00-05:00`;

  }

  //* ==========================================*

  //* 8. CARGAR FORMULARIO DE DETALLE*

  //* ==========================================*

  private cargarFormularioDetalle(

    sorteo: SorteoListado

  ): void {

    this.formularioDetalle = {

      titulo: sorteo.titulo,

      descripcion: sorteo.descripcion,

      aporteMinimo: sorteo.aporteMinimo,

      metaRecaudacion: sorteo.metaRecaudacion,

      fechaInicio: this.fechaParaInput(

        sorteo.fechaInicio

      ),

      fechaCierre: this.fechaParaInput(

        sorteo.fechaCierre

      ),

      fechaSorteo: this.fechaParaInput(

        sorteo.fechaSorteo

      )

    };

    this.formularioDetalleOriginal =

      JSON.stringify(this.formularioDetalle);

  }

  //* ==========================================*

  //* 9. CONSULTAR DETALLE DE UN SORTEO*

  //* GET /api/v1/sorteo/{id}*

  //* ==========================================*

  abrirDetalle(id: string): void {
    this.confirmarPublicacion.set(false);
    this.peticionDetalle?.unsubscribe();

    this.modalAbierto.set(true);

    this.detalleSorteo.set(null);

    this.formularioDetalle =

      this.formularioDetalleInicial();

    this.formularioDetalleOriginal = '';

    this.errorDetalle.set(null);

    this.mensajeDetalle.set(null);

    this.cargandoDetalle.set(true);

    this.procesandoDetalle.set(false);

    this.peticionDetalle =

      this.publicacionesService.obtenerPorId(id).subscribe({

        next: (respuesta) => {

          if (respuesta.success && respuesta.data) {

            const sorteo = respuesta.data;

            this.detalleSorteo.set(sorteo);

            this.cargarFormularioDetalle(sorteo);

          } else {

            this.errorDetalle.set(

              respuesta.message ||

              'No se pudo obtener el detalle del sorteo.'

            );

          }

          this.cargandoDetalle.set(false);

        },

        error: (error) => {

          if (error.status === 401) {

            this.errorDetalle.set(

              'Tu sesión expiró. Inicia sesión nuevamente.'

            );

          } else if (error.status === 403) {

            this.errorDetalle.set(

              'No tienes permisos para consultar este sorteo.'

            );

          } else if (error.status === 404) {

            this.errorDetalle.set(

              'El sorteo solicitado no existe.'

            );

          } else {

            this.errorDetalle.set(

              'Ocurrió un error al consultar el sorteo.'

            );

          }

          this.cargandoDetalle.set(false);

        }

      });

  }

  //* ==========================================*

  //* 10. VALIDAR DATOS ANTES DEL PUT*

  //* ==========================================*

  private construirActualizacion():

    ActualizarSorteoRequest | null {

    const form = this.formularioDetalle;

    //* Título y descripción*

    if (

      form.titulo.trim().length < 5 ||

      form.titulo.trim().length > 180 ||

      form.descripcion.trim().length < 10 ||

      form.descripcion.trim().length > 3000

    ) {

      this.errorDetalle.set(

        'El título debe tener entre 5 y 180 caracteres ' +

        'y la descripción entre 10 y 3000.'

      );

      return null;

    }

    //* Aporte mínimo*

    const aporte = Number(form.aporteMinimo);

    if (

      !Number.isFinite(aporte) ||

      aporte < 0.01 ||

      aporte > 99999999.99

    ) {

      this.errorDetalle.set(

        'El aporte mínimo debe ser mayor que cero.'

      );

      return null;

    }

    //* Meta de recaudación*

    const meta =

      form.metaRecaudacion === null ||

        form.metaRecaudacion === undefined

        ? null

        : Number(form.metaRecaudacion);

    if (

      meta !== null &&

      (

        !Number.isFinite(meta) ||

        meta < aporte ||

        meta > 999999999.99

      )

    ) {

      this.errorDetalle.set(

        'La meta debe ser igual o mayor al aporte mínimo.'

      );

      return null;

    }

    //* Fechas obligatorias*

    if (

      !form.fechaInicio ||

      !form.fechaCierre

    ) {

      this.errorDetalle.set(

        'Las fechas de inicio y cierre son obligatorias.'

      );

      return null;

    }

    const inicio = this.fechaPeru(

      form.fechaInicio

    );

    const cierre = this.fechaPeru(

      form.fechaCierre

    );

    const inicioMs = Date.parse(inicio);

    const cierreMs = Date.parse(cierre);

    if (

      !Number.isFinite(inicioMs) ||

      !Number.isFinite(cierreMs) ||

      cierreMs <= inicioMs

    ) {

      this.errorDetalle.set(

        'La fecha de cierre debe ser posterior al inicio.'

      );

      return null;

    }

    //* Fecha del sorteo opcional*

    const fechaSorteo = form.fechaSorteo

      ? this.fechaPeru(form.fechaSorteo)

      : null;

    if (fechaSorteo) {

      const fechaSorteoMs =

        Date.parse(fechaSorteo);

      if (

        !Number.isFinite(fechaSorteoMs) ||

        fechaSorteoMs <= cierreMs

      ) {

        this.errorDetalle.set(

          'La fecha del sorteo debe ser posterior al cierre.'

        );

        return null;

      }

    }

    //* Construir el cuerpo JSON del PUT*

    return {

      titulo: form.titulo.trim(),

      descripcion: form.descripcion.trim(),

      aporteMinimo: aporte,

      metaRecaudacion: meta,

      fechaInicio: inicio,

      fechaCierre: cierre,

      fechaSorteo

    };

  }

  //* ==========================================*

  //* 11. ACTUALIZAR BORRADOR*

  //* PUT /api/v1/sorteo/{id}*

  //* ==========================================*

  private async guardarDetalleInterno():

    Promise<boolean> {

    const sorteo = this.detalleSorteo();

    if (

      !sorteo ||

      !this.detalleEditable

    ) {

      return false;

    }

    const request =

      this.construirActualizacion();

    if (!request) {

      return false;

    }

    try {

      const respuesta = await firstValueFrom(

        this.publicacionesService.actualizar(

          sorteo.id,

          request

        )

      );

      if (

        !respuesta.success ||

        !respuesta.data

      ) {

        this.errorDetalle.set(

          respuesta.message ||

          'No se pudieron guardar los cambios.'

        );

        return false;

      }

      //* Actualizar datos que muestra el modal.*

      this.detalleSorteo.set(respuesta.data);

      //* Actualizar formulario y establecer*

      //* los nuevos datos como guardados.*

      this.cargarFormularioDetalle(

        respuesta.data

      );

      //* Actualizar tabla sin cerrar modal.*

      this.cargarSorteos();

      return true;

    } catch (error: unknown) {

      this.errorDetalle.set(

        this.obtenerMensajeError(

          error,

          'Ocurrió un error al actualizar el sorteo.'

        )

      );

      return false;

    }

  }


async guardarCambiosDetalle(): Promise<void> {

  if (
    this.procesandoDetalle() ||
    !this.detalleEditable ||
    this.cargandoDetalle()
  ) {
    return;
  }

  // Cerrar confirmación pendiente.
  this.confirmarPublicacion.set(false);

  this.procesandoDetalle.set(true);
  this.errorDetalle.set(null);
  this.mensajeDetalle.set(null);

  try {
    const guardado = await this.guardarDetalleInterno();

    if (guardado) {
      this.mensajeDetalle.set(
        'Cambios guardados correctamente.'
      );
    }

  } finally {
    this.procesandoDetalle.set(false);
  }
}


  //* ==========================================*

  //* 12. PUBLICAR SORTEO*

  //* PATCH /api/v1/sorteo/{id}/publicar*

  //* ==========================================*

  async publicarSorteoDetalle(): Promise<void> {

    const sorteo = this.detalleSorteo();

    if (

      !sorteo ||

      !this.detalleEditable ||

      this.procesandoDetalle() ||

      this.cargandoDetalle()

    ) {

      return;

    }

this.confirmarPublicacion.set(false);

    this.procesandoDetalle.set(true);

    this.errorDetalle.set(null);

    this.mensajeDetalle.set(null);

    try {

      //* Si el formulario tiene cambios pendientes,*

      //* ejecutar primero PUT.*

      if (this.detalleModificado) {

        const guardado =

          await this.guardarDetalleInterno();

        //* Si el PUT falla, no publicar.*

        if (!guardado) {

          return;

        }

      }

      //* Confirmar publicación mediante PATCH.*

      const respuesta = await firstValueFrom(

        this.publicacionesService.publicar(

          sorteo.id

        )

      );

      if (

        !respuesta.success ||

        !respuesta.data

      ) {

        this.errorDetalle.set(

          respuesta.message ||

          'No se pudo publicar el sorteo.'

        );

        return;

      }

      //* Actualizar estado del modal.*

      this.detalleSorteo.update(actual =>

        actual

          ? {

            ...actual,

            estado: 'Publicado'

          }

          : actual

      );

      this.mensajeDetalle.set(

        'Sorteo publicado correctamente.'

      );

      //* Actualizar la lista y sus filtros.*

      this.cargarSorteos();

    } catch (error: unknown) {

      this.errorDetalle.set(

        this.obtenerMensajeError(

          error,

          'No se pudo publicar el sorteo.'

        )

      );

    } finally {

      this.procesandoDetalle.set(false);

    }

  }

  //* ==========================================*

  //* 13. CERRAR MODAL DE DETALLE*

  //* ==========================================*

solicitarPublicacion(): void {
  if (
    !this.detalleSorteo() ||
    !this.detalleEditable ||
    this.procesandoDetalle() ||
    this.cargandoDetalle()
  ) {
    return;
  }

  this.errorDetalle.set(null);
  this.mensajeDetalle.set(null);
  this.confirmarPublicacion.set(true);
}

cancelarPublicacion(): void {
  this.confirmarPublicacion.set(false);
}

  cerrarDetalle(): void {

    //* No cerrar mientras se ejecuta PUT/PATCH.*

    if (this.procesandoDetalle()) {

      return;

    }

    //* Confirmar salida si existen cambios.*

    if (

      this.detalleEditable &&

      this.detalleModificado

    ) {

      const salir = window.confirm(

        'Tienes cambios sin guardar. ¿Deseas salir?'

      );

      if (!salir) {

        return;

      }

    }

    this.peticionDetalle?.unsubscribe();
this.confirmarPublicacion.set(false);
    this.modalAbierto.set(false);

    this.detalleSorteo.set(null);

    this.errorDetalle.set(null);

    this.mensajeDetalle.set(null);

    this.cargandoDetalle.set(false);

    this.formularioDetalle =

      this.formularioDetalleInicial();

    this.formularioDetalleOriginal = '';

  }

  //* ==========================================*

  //* 14. ABRIR MODAL CREAR SORTEO*

  //* ==========================================*

  abrirCrear(): void {

    this.formularioCrear =

      this.formularioInicial();

    this.errorCrear.set(null);

    this.mensajeCrear.set(null);

    this.guardandoSorteo.set(false);

    this.modalCrearAbierto.set(true);

  }

  cerrarCrear(): void {

    if (this.guardandoSorteo()) {

      return;

    }

    this.peticionCrear?.unsubscribe();

    this.modalCrearAbierto.set(false);

    //* Refrescar listado si ya se creó.*

  }

  //* ==========================================*

  //* 16. CREAR NUEVO SORTEO*

  //* POST /api/v1/sorteo*

  //* ==========================================*

  guardarNuevoSorteo(): void {

    if (this.guardandoSorteo()) {

      return;

    }

    this.errorCrear.set(null);

    this.mensajeCrear.set(null);

    const form = this.formularioCrear;

    //* Validar campos obligatorios.*

    if (

      form.titulo.trim().length < 5 ||

      form.titulo.trim().length > 180 ||

      form.descripcion.trim().length < 10 ||

      form.descripcion.trim().length > 3000 ||

      !form.fechaInicio ||

      !form.fechaCierre

    ) {

      this.errorCrear.set(

        'Completa correctamente el título, ' +

        'la descripción y las fechas obligatorias.'

      );

      return;

    }

    //* Validar aporte mínimo.*

    const aporte =

      Number(form.aporteMinimo);

    if (

      !Number.isFinite(aporte) ||

      aporte < 0.01 ||

      aporte > 99999999.99

    ) {

      this.errorCrear.set(

        'El aporte mínimo no es válido.'

      );

      return;

    }

    //* Validar meta.*

    const meta =

      form.metaRecaudacion === null ||

        form.metaRecaudacion === undefined

        ? null

        : Number(form.metaRecaudacion);

    if (

      meta !== null &&

      (

        !Number.isFinite(meta) ||

        meta < aporte ||

        meta > 999999999.99

      )

    ) {

      this.errorCrear.set(

        'La meta debe ser igual o mayor al aporte mínimo.'

      );

      return;

    }

    //* Validar fechas.*

    const fechaInicio =

      this.fechaPeru(form.fechaInicio);

    const fechaCierre =

      this.fechaPeru(form.fechaCierre);

    const inicioMs =

      Date.parse(fechaInicio);

    const cierreMs =

      Date.parse(fechaCierre);

    if (

      !Number.isFinite(inicioMs) ||

      !Number.isFinite(cierreMs) ||

      cierreMs <= inicioMs

    ) {

      this.errorCrear.set(

        'La fecha de cierre debe ser posterior al inicio.'

      );

      return;

    }

    //* Fecha del sorteo opcional.*

    const fechaSorteo = form.fechaSorteo

      ? this.fechaPeru(form.fechaSorteo)

      : null;

    if (fechaSorteo) {

      const sorteoMs =

        Date.parse(fechaSorteo);

      if (

        !Number.isFinite(sorteoMs) ||

        sorteoMs <= cierreMs

      ) {

        this.errorCrear.set(

          'La fecha del sorteo debe ser posterior al cierre.'

        );

        return;

      }

    }

    //* Construir request.*

    const request: CrearSorteoRequest = {

      titulo: form.titulo.trim(),

      descripcion: form.descripcion.trim(),

      imagenPortadaUrl: null,

      aporteMinimo: aporte,

      metaRecaudacion: meta,

      fechaInicio,

      fechaCierre,

      fechaSorteo

    };

    this.guardandoSorteo.set(true);

    this.peticionCrear =

      this.publicacionesService.crear(

        request

      ).subscribe({

        next: (respuesta) => {

          if (

            !respuesta.success ||

            !respuesta.data?.id

          ) {

            this.errorCrear.set(

              respuesta.message ||

              'No se pudo registrar el sorteo.'

            );

            this.guardandoSorteo.set(false);

            return;

          }

          this.finalizarCreacion();

        },

        error: (error) => {

          this.guardandoSorteo.set(false);

          this.errorCrear.set(

            error.error?.message ||

            'Ocurrió un error al registrar el sorteo.'

          );

        }

      });

  }

  //* ==========================================*

  //* 18. FINALIZAR CREACIÓN*

  //* ==========================================*

  private finalizarCreacion(): void {

    this.guardandoSorteo.set(false);

    this.errorCrear.set(null);

    this.mensajeCrear.set(

      'Borrador registrado correctamente.'

    );

    this.modalCrearAbierto.set(false);

    //* Mostrar todos los estados para que*

    //* aparezca el nuevo borrador.*

    this.estado.set(null);

    this.pagina.set(1);

    this.cargarSorteos();

  }

  //* ==========================================*

  //* 19. EXTRAER MENSAJES DE ERROR HTTP*

  //* ==========================================*

  private obtenerMensajeError(

    error: unknown,

    mensajePredeterminado: string

  ): string {

    if (

      typeof error === 'object' &&

      error !== null &&

      'error' in error

    ) {

      const respuesta = error.error;

      if (

        typeof respuesta === 'object' &&

        respuesta !== null &&

        'message' in respuesta &&

        typeof respuesta.message === 'string'

      ) {

        return respuesta.message;

      }

    }

    return mensajePredeterminado;

  }

  //* ==========================================*

  //* 20. CERRAR MODALES CON ESCAPE*

  //* ==========================================*

  @HostListener('document:keydown.escape')

  cerrarConEscape(): void {
if (this.modalPortadaAbierto()) {
  this.cerrarModalPortada();
  return;
}
    if (this.modalCrearAbierto()) {

      this.cerrarCrear();

      return;

    }

    if (this.modalAbierto()) {

      this.cerrarDetalle();

    }

  }

}
