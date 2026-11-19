
import {
  AfterViewInit,
  Component,
  ElementRef,
  NgZone,
  OnDestroy,
  ViewChild,
  computed,
  inject,
  signal
} from '@angular/core';

import { CommonModule } from '@angular/common';

import {
  ActivatedRoute,
  Router
} from '@angular/router';

import { Subscription } from 'rxjs';

import { Wheel } from 'spin-wheel';
import { RuletaService } from '../../services/ruleta.service.';

import {
  ClienteRuleta,
  ParticipanteRuleta,
  RuletaDatos
} from '../../models/ruleta.model';

@Component({
  selector: 'app-ruleta',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './ruleta.html',
  styleUrl: './ruleta.css'
})
export class Ruleta implements AfterViewInit, OnDestroy {

  @ViewChild('wheelHost')
  wheelHost!: ElementRef<HTMLDivElement>;

  private readonly service = inject(RuletaService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly zone = inject(NgZone);

  private wheel: Wheel | null = null;
  private peticion?: Subscription;

  private indiceGanadorPendiente: number | null = null;

  // ==========================================
  // ESTADO GENERAL
  // ==========================================

  readonly sorteoId = signal('');

  readonly datos = signal<RuletaDatos | null>(null);

  readonly participantes = signal<ParticipanteRuleta[]>([]);

  readonly cargando = signal(false);

  readonly error = signal<string | null>(null);

  readonly girando = signal(false);

  readonly ganador = signal<ParticipanteRuleta | null>(null);

  readonly modalGanador = signal(false);

  // ==========================================
  // CONTROL DE FECHA Y PARTICIPANTES
  // ==========================================

  readonly puedeGirar = computed(() => {

    const data = this.datos();

    return !!data &&
      data.estado === 'Publicado' &&
      data.puedeSortear &&
      this.participantes().length > 0 &&
      !this.cargando() &&
      !this.girando();
  });

  readonly totalParticipantes = computed(
    () => this.participantes().length
  );

  // ==========================================
  // INICIALIZACIÓN
  // ==========================================

  ngAfterViewInit(): void {

    const id = this.route.snapshot.paramMap.get('sorteoId');

    if (!id) {
      this.error.set('No se recibió el ID de la publicación.');
      return;
    }

    this.sorteoId.set(id);

    this.cargarDatos();
  }

  ngOnDestroy(): void {
    this.peticion?.unsubscribe();
    this.eliminarRuleta();
  }

  // ==========================================
  // GET: CARGAR PARTICIPANTES
  // ==========================================

  cargarDatos(): void {

    const id = this.sorteoId();

    if (!id || this.girando()) {
      return;
    }

    this.peticion?.unsubscribe();

    this.cargando.set(true);
    this.error.set(null);

    this.ganador.set(null);
    this.modalGanador.set(false);

    this.peticion = this.service.obtenerDatos(id)
      .subscribe({

        next: respuesta => {

          if (!respuesta.success || !respuesta.data) {

            this.datos.set(null);
            this.participantes.set([]);
            this.eliminarRuleta();

            this.error.set(
              respuesta.message ||
              'No se pudieron obtener los participantes.'
            );

            this.cargando.set(false);
            return;
          }

          const data = respuesta.data;

          this.datos.set(data);

          const clientes = data.clientes.filter(
            c => Number.isFinite(c.totalAportado) &&
                 c.totalAportado > 0
          );

          const participantes = this.calcularProbabilidades(
            clientes
          );

          this.participantes.set(participantes);

          this.construirRuleta();

          this.cargando.set(false);
        },

        error: err => {

          this.datos.set(null);
          this.participantes.set([]);
          this.eliminarRuleta();

          this.error.set(
            err.error?.message ||
            'No fue posible consultar los datos de la ruleta.'
          );

          this.cargando.set(false);
        }
      });
  }

  // ==========================================
  // PROBABILIDAD POR MONTO ACUMULADO
  // ==========================================
  // P(cliente) = montoCliente / totalConfirmado
  //
  // Usamos centavos enteros para el sorteo.
  // No hay duplicados por cada aporte:
  // el backend ya agrupó por ClienteId.

  private calcularProbabilidades(
    clientes: ClienteRuleta[]
  ): ParticipanteRuleta[] {

    const participantes = clientes.map(cliente => ({
      ...cliente,

      pesoCentavos: Math.round(
        cliente.totalAportado * 100
      ),

      probabilidad: 0
    }));

    const pesoTotal = participantes.reduce(
      (total, cliente) => total + cliente.pesoCentavos,
      0
    );

    return participantes.map(cliente => ({
      ...cliente,

      probabilidad: pesoTotal > 0
        ? cliente.pesoCentavos / pesoTotal * 100
        : 0
    }));
  }

  // ==========================================
  // CREAR LA RULETA VISUAL
  // ==========================================

  private construirRuleta(): void {

    this.eliminarRuleta();

    const participantes = this.participantes();

    if (!participantes.length) {
      return;
    }

    const colores = [
      '#7c3aed',
      '#2563eb',
      '#0891b2',
      '#059669',
      '#ea580c',
      '#db2777',
      '#4f46e5',
      '#0f766e'
    ];

    const items = participantes.map((cliente, index) => ({

      label:
        `${cliente.nombreCliente} · S/ ${cliente.totalAportado.toFixed(2)}`,

      weight: cliente.pesoCentavos,

      backgroundColor: colores[index % colores.length],

      labelColor: '#ffffff',

      value: cliente.clienteId

    }));

    this.wheel = new Wheel(
      this.wheelHost.nativeElement,
      {
        items,

        // El usuario no puede arrastrar la ruleta.
        // Solo se gira con nuestro botón.
        isInteractive: false,

        // Puntero situado en la parte superior.
        pointerAngle: 270,

        itemLabelFontSizeMax: 16,
        itemLabelRadius: 0.86,
        itemLabelRadiusMax: 0.22,

        borderWidth: 5,
        borderColor: '#ffffff',

        lineWidth: 2,
        lineColor: '#ffffff',

        onRest: event => {

          this.zone.run(() => {

            if (!this.girando()) {
              return;
            }

            const indice = this.indiceGanadorPendiente;

            if (
              indice === null ||
              event.currentIndex !== indice
            ) {
              this.girando.set(false);
              this.error.set(
                'La ruleta no terminó en el participante esperado. ' +
                'Puedes volver a girarla.'
              );
              return;
            }

            const seleccionado =
              this.participantes()[indice];

            if (seleccionado) {
              this.ganador.set(seleccionado);
              this.modalGanador.set(true);
            }

            this.indiceGanadorPendiente = null;
            this.girando.set(false);

          });
        }
      }
    );
  }

  // ==========================================
  // GENERADOR DE NÚMEROS ALEATORIOS
  // ==========================================
  // Entero uniforme entre 0 y limite - 1.
  // Evita el sesgo de usar Math.random().
  // Admitimos hasta Number.MAX_SAFE_INTEGER
  // unidades de centavo.

  private enteroSeguro(limite: number): number {

    const rango = 2 ** 53;

    if (
      !Number.isSafeInteger(limite) ||
      limite <= 0
    ) {
      throw new Error(
        'El total de aportes no permite calcular un sorteo seguro.'
      );
    }

    const maximoAceptado =
      Math.floor(rango / limite) * limite;

    const valores = new Uint32Array(2);

    while (true) {

      crypto.getRandomValues(valores);

      const numero =
        (valores[0] & 0x1fffff) * (2 ** 32) +
        valores[1];

      if (numero < maximoAceptado) {
        return numero % limite;
      }
    }
  }

  // ==========================================
  // ELEGIR GANADOR SEGÚN MONTO
  // ==========================================

  private elegirIndiceGanador(): number {

    const participantes = this.participantes();

    const pesoTotal = participantes.reduce(
      (total, p) => total + p.pesoCentavos,
      0
    );

    const numeroGanador = this.enteroSeguro(
      pesoTotal
    );

    let acumulado = 0;

    for (
      let indice = 0;
      indice < participantes.length;
      indice++
    ) {

      acumulado += participantes[indice].pesoCentavos;

      if (numeroGanador < acumulado) {
        return indice;
      }
    }

    throw new Error(
      'No se pudo seleccionar un participante.'
    );
  }

  // ==========================================
  // GIRAR RULETA
  // ==========================================

  girar(): void {

    if (!this.puedeGirar() || !this.wheel) {
      return;
    }

    const data = this.datos();

    if (
      !data ||
      !data.fechaSorteo ||
      Date.now() < new Date(data.fechaSorteo).getTime()
    ) {
      this.error.set(
        'Todavía no ha llegado la fecha del sorteo.'
      );
      return;
    }

    try {

      this.error.set(null);
      this.ganador.set(null);
      this.modalGanador.set(false);

      const indice = this.elegirIndiceGanador();

      this.indiceGanadorPendiente = indice;
      this.girando.set(true);

      // La librería dibuja la animación y
      // se detiene en el índice calculado.
      this.wheel.spinToItem(
        indice,
        6500, // Duración en milisegundos
        true, // Centro del segmento
        6,    // Vueltas completas
        1     // Sentido horario
      );

    } catch {

      this.indiceGanadorPendiente = null;
      this.girando.set(false);

      this.error.set(
        'No se pudo iniciar el giro de la ruleta.'
      );
    }
  }

  // ==========================================
  // MODAL DEL GANADOR
  // ==========================================

  cerrarGanador(): void {
    this.modalGanador.set(false);
  }

  // ==========================================
  // DESTRUIR RULETA
  // ==========================================

  private eliminarRuleta(): void {

    if (this.wheel) {
      this.wheel.remove();
      this.wheel = null;
    }

    this.indiceGanadorPendiente = null;
  }

  // ==========================================
  // REGRESAR
  // ==========================================

  volver(): void {

    this.router.navigate([
      '/publicaciones',
      this.sorteoId(),
      'aportes'
    ]);
  }
}
