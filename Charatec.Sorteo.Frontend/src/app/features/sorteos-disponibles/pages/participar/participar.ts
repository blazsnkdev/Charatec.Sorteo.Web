
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

import { ParticiparService } from '../../services/participar.service';

import {
  PrepararParticipacionResponse
} from '../../models/preparar-participacion.model';

import {
  RegistrarParticipacionRequest,
  RegistrarParticipacionResponse
} from '../../models/registrar-participacion.model';

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
  private readonly participarService = inject(ParticiparService);

  private peticion?: Subscription;
  private parametros?: Subscription;
  private registroPeticion?: Subscription;

  // CONFIGURACIÓN DEL SORTEO

  readonly cargando = signal(true);
  readonly error = signal<string | null>(null);
  readonly sorteoId = signal('');

  readonly configuracion =
    signal<PrepararParticipacionResponse | null>(null);

  // DESLIZADOR DE APORTE

  readonly montoSeleccionado = signal(0);

  readonly porcentajeMeta = computed(() => {
    const datos = this.configuracion();

    if (
      !datos?.metaRecaudacion ||
      datos.metaRecaudacion <= 0
    ) {
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

  // MODAL YAPE

  readonly modalYapeAbierto = signal(false);
  readonly enviandoParticipacion = signal(false);
  readonly errorRegistro = signal<string | null>(null);
  readonly registroExitoso = signal(false);

  readonly participacionRegistrada =
    signal<RegistrarParticipacionResponse | null>(null);

  // FORMULARIO

  emailContacto = '';
  numeroWhatsapp = '';
  codigoOperacionYape = '';
  aceptaTerminos = false;

  // Conserva el mismo ID al reintentar
  // exactamente una misma solicitud.

  private solicitudId: string | null = null;
  private huellaSolicitud: string | null = null;

  // WHATSAPP DE LA EMPRESA

  private readonly numeroWhatsappEmpresa = '51943771361';

  // COMPROBANTE TEMPORAL

  
  readonly envioWhatsappIniciado = signal(false);
  

  private huellaDatosCompartidos: string | null = null;


  private obtenerHuellaComprobante(): string {
    return JSON.stringify({
      sorteoId: this.sorteoId(),
      monto: this.montoSeleccionado(),
      email: this.emailContacto.trim(),
      whatsapp: this.numeroWhatsapp.trim(),
      codigo: this.codigoOperacionYape.trim()
    });
  }

  datosCompartidosVigentes(): boolean {
    return (
      this.envioWhatsappIniciado() &&
      this.huellaDatosCompartidos !== null &&
      this.huellaDatosCompartidos ===
      this.obtenerHuellaComprobante()
    );
  }

  puedeRegistrarConComprobante(): boolean {
    return (
      this.datosCompartidosVigentes() &&
      !this.enviandoParticipacion() &&
      !this.registroExitoso()
    );
  }


  seleccionarComprobante(event: Event): void {

    const input = event.target as HTMLInputElement;
    const archivo = input.files?.[0] ?? null;

    this.envioWhatsappIniciado.set(false);
    
    this.huellaDatosCompartidos = null;
    this.errorRegistro.set(null);

    if (!archivo) {
      return;
    }

    const tiposPermitidos = [
      'image/jpeg',
      'image/png',
      'image/webp'
    ];

    if (!tiposPermitidos.includes(archivo.type)) {
      this.errorRegistro.set(
        'Selecciona una imagen JPG, PNG o WEBP.'
      );
      input.value = '';
      return;
    }

    if (archivo.size > 5 * 1024 * 1024) {
      this.errorRegistro.set(
        'El comprobante no debe superar los 5 MB.'
      );
      input.value = '';
      return;
    }

    
  }

  // INICIO

  ngOnInit(): void {

    this.parametros = this.route.paramMap.subscribe(
      params => {

        const id = params.get('sorteoId');

        if (!id) {
          this.error.set(
            'No se recibió el ID del sorteo.'
          );
          this.cargando.set(false);
          return;
        }

        this.sorteoId.set(id);
        this.reiniciarFormulario();
        this.cargarConfiguracion(id);
      }
    );
  }

  ngOnDestroy(): void {
    this.peticion?.unsubscribe();
    this.parametros?.unsubscribe();
    this.registroPeticion?.unsubscribe();
  }

  // GET: CONFIGURACIÓN

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
            this.montoSeleccionado.set(datos.montoInicial);
            this.emailContacto = datos.email ?? '';
          }

          this.cargando.set(false);
        },

        error: err => {

          this.cargando.set(false);

          console.error(
            'Error al cargar la configuración del sorteo:',
            err
          );
        }
      });
  }

  // ACTUALIZAR APORTE

  cambiarMonto(valor: number | string): void {

    const datos = this.configuracion();

    if (!datos || !datos.puedeParticipar) {
      return;
    }

    const monto = Number(valor);

    if (!Number.isFinite(monto)) {
      return;
    }

    const resultado = Math.min(
      datos.aporteMaximo,
      Math.max(datos.aporteMinimo, monto)
    );

    this.montoSeleccionado.set(
      Math.round(resultado * 100) / 100
    );
  }

  // ABRIR MODAL

  abrirModalYape(): void {

    const datos = this.configuracion();

    if (!datos || !datos.puedeParticipar) {
      return;
    }

    this.errorRegistro.set(null);
    this.modalYapeAbierto.set(true);
  }

  // CERRAR MODAL

  cerrarModalYape(): void {

    if (this.enviandoParticipacion()) {
      return;
    }

    this.modalYapeAbierto.set(false);
    this.errorRegistro.set(null);
  }

  // REINICIAR FORMULARIO

  private reiniciarFormulario(): void {

    this.modalYapeAbierto.set(false);
    this.registroExitoso.set(false);
    this.participacionRegistrada.set(null);
    this.errorRegistro.set(null);

    this.emailContacto = '';
    this.numeroWhatsapp = '';
    this.codigoOperacionYape = '';
    this.aceptaTerminos = false;

    
    this.envioWhatsappIniciado.set(false);
    this.huellaDatosCompartidos = null;

    this.solicitudId = null;
    this.huellaSolicitud = null;
  }

  // VALIDAR FORMULARIO

  private validarFormulario(): string | null {

    const datos = this.configuracion();

    if (!datos || !datos.puedeParticipar) {
      return 'El sorteo no está disponible.';
    }

    const email = this.emailContacto.trim();
    const whatsapp = this.numeroWhatsapp.trim();
    const codigo = this.codigoOperacionYape.trim();
    const monto = this.montoSeleccionado();

    if (!email || !whatsapp || !codigo) {
      return 'Completa todos los campos.';
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      return 'Ingresa un correo electrónico válido.';
    }

    if (!/^9\d{8}$/.test(whatsapp)) {
      return 'Ingresa un WhatsApp peruano de 9 dígitos.';
    }

    if (!/^[0-9]{8}$/.test(codigo)) {
      return 'El código de operación Yape debe contener exactamente 8 dígitos numéricos.';
    }

    if (!this.aceptaTerminos) {
      return 'Debes aceptar los términos de participación.';
    }

    if (
      !Number.isFinite(monto) ||
      monto < datos.aporteMinimo ||
      monto > datos.aporteMaximo
    ) {
      return 'El monto está fuera del rango permitido.';
    }

    return null;
  }

  filtrarCodigoYape(valor: string): void {
    this.codigoOperacionYape = (valor ?? '')
      .replace(/[^0-9]/g, '')
      .slice(0, 8);
  }

  soloNumerosYape(event: Event): void {

    const input = event.target as HTMLInputElement;

    const valorFiltrado = input.value
      .replace(/[^0-9]/g, '')
      .slice(0, 8);

    input.value = valorFiltrado;
    this.codigoOperacionYape = valorFiltrado;
  }

  formularioListo(): boolean {
    return this.validarFormulario() === null;
  }

  // POST: REGISTRAR PARTICIPACIÓN

  registrarParticipacion(): void {

    if (!this.puedeRegistrarConComprobante()) {
      this.errorRegistro.set(
        'Primero debes compartir los datos de tu operación Yape por WhatsApp.'
      );
      return;
    }

    const mensajeError = this.validarFormulario();

    if (mensajeError) {
      this.errorRegistro.set(mensajeError);
      return;
    }

    const datos = this.configuracion();

    if (!datos) {
      return;
    }

    const monto = this.montoSeleccionado();
    const email = this.emailContacto.trim();
    const whatsapp = this.numeroWhatsapp.trim();
    const codigo = this.codigoOperacionYape.trim();

    // La huella identifica el contenido.
    // Si cambian los datos, cambia el ID.

    const huella = JSON.stringify({
      sorteoId: datos.sorteoId,
      monto,
      email,
      whatsapp,
      codigo
    });

    if (
      !this.solicitudId ||
      this.huellaSolicitud !== huella
    ) {
      this.solicitudId = crypto.randomUUID();
      this.huellaSolicitud = huella;
    }

    const request: RegistrarParticipacionRequest = {
      monto,
      numeroWhatsapp: whatsapp,
      email,
      codigoOperacionYape: codigo,
      aceptaTerminos: true,
      solicitudId: this.solicitudId
    };

    this.enviandoParticipacion.set(true);
    this.errorRegistro.set(null);

    this.registroPeticion = this.participarService
      .registrarParticipacion(
        datos.sorteoId,
        request
      )
      .subscribe({

        next: respuesta => {

          this.enviandoParticipacion.set(false);

          if (respuesta.success && respuesta.data) {

            this.participacionRegistrada.set(
              respuesta.data
            );

            this.registroExitoso.set(true);
            return;
          }

          this.errorRegistro.set(
            respuesta.message ||
            'No se pudo registrar la participación.'
          );
        },

        error: err => {

          this.enviandoParticipacion.set(false);

          if (err.status === 401) {

            this.errorRegistro.set(
              'Tu sesión no está autorizada.'
            );

          } else if (err.status === 403) {

            this.errorRegistro.set(
              'No tienes permisos para participar.'
            );

          } else if (err.status === 400) {

            this.errorRegistro.set(
              err.error?.message ||
              'Revisa los datos del formulario.'
            );

          } else if (err.status === 404) {

            this.errorRegistro.set(
              'El endpoint o sorteo no fue encontrado.'
            );

          } else if (err.status === 409) {

            this.errorRegistro.set(
              'La operación ya existe o hay un conflicto. ' +
              'Verifica tu participación antes de reintentar.'
            );

          } else {

            this.errorRegistro.set(
              'No pudimos confirmar si se guardó el registro. ' +
              'Consulta su estado antes de repetir el pago.'
            );
          }
        }
      });
  }

  // COMPARTIR POR WHATSAPP

  compartirPorWhatsapp(): void {

    const datos = this.configuracion();

    if (!datos) return;

    const error = this.validarFormulario();

    if (error) {
      this.errorRegistro.set(error);
      return;
    }

    const numero = this.numeroWhatsappEmpresa.replace(/\D/g, '');

    if (!/^51\d{9}$/.test(numero)) {
      this.errorRegistro.set(
        'El número de WhatsApp de Charatec no está configurado.'
      );
      return;
    }

    const mensaje = [
      'Hola, Corporación Charatec.',
      '',
      'Solicito la verificación de mi aporte Yape.',
      '',
      `Sorteo: ${datos.titulo}`,
      `Monto: S/ ${this.montoSeleccionado().toFixed(2)}`,
      `Correo: ${this.emailContacto.trim()}`,
      `Mi WhatsApp: ${this.numeroWhatsapp.trim()}`,
      `Código de operación Yape: ${this.codigoOperacionYape.trim()}`,
      '',
      'Mi participación quedará pendiente de aprobación.'
    ].join('\n');

    const url =
      `https://wa.me/${numero}?text=${encodeURIComponent(mensaje)}`;

    const ventana = window.open(url, '_blank');

    if (!ventana) {
      this.errorRegistro.set(
        'No se pudo abrir WhatsApp. Permite las ventanas emergentes.'
      );
      return;
    }

    ventana.opener = null;

    this.huellaDatosCompartidos =
      this.obtenerHuellaComprobante();

    this.envioWhatsappIniciado.set(true);
    this.errorRegistro.set(null);
  }


  // VOLVER

  volver(): void {
    this.router.navigate(['/sorteos']);
  }
}
