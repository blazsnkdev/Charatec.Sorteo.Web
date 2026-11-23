
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
@Component({
  selector: 'app-error-403',
  standalone: true,
  imports: [RouterLink],
  template: `
    <main class="error-pagina">
      <section class="error-contenido">
        <span class="error-codigo">403</span>
        <h1>Acceso Denegado</h1>
        <p>
          No tienes los permisos necesarios para acceder a esta sección. Si consideras que se trata de un error, comunícate con nuestro equipo de soporte.
        </p>
        <a routerLink="/home">Volver al inicio</a>
      </section>
    </main>
  `,
  styles: [`
    .error-pagina {
      min-height: 75vh;
      display: grid;
      place-items: center;
      padding: 24px;
      text-align: center;
      background: #f4f6fa;
    }

    .error-contenido {
      max-width: 440px;
    }

    .error-codigo {
      display: block;
      color: #003bff;
      font-size: clamp(80px, 15vw, 130px);
      font-weight: 800;
      line-height: 1;
    }

    h1 {
      margin: 16px 0;
      color: #111827;
    }

    p {
      color: #64748b;
      line-height: 1.6;
    }

    a {
      display: inline-block;
      margin-top: 22px;
      padding: 12px 24px;
      background: #003bff;
      border-radius: 9px;
      color: white;
      text-decoration: none;
      font-weight: 600;
    }
  `]
})
export class Error403 {}
