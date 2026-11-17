import { Component,inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { SesionService } from '../../../core/services/sesion.service';
@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './header.html',
  styleUrl: './header.css'
})
export class Header {
  readonly menuAbierto = signal(false);

    // Servicio que administra el JWT
  readonly sesionService = inject(SesionService);
  private readonly router = inject(Router);

  alternarMenu(): void {
    this.menuAbierto.update(abierto => !abierto);
  }

  cerrarMenu(): void {
    this.menuAbierto.set(false);
  }
    cerrarSesion(): void {
    this.sesionService.cerrarSesion();

    this.cerrarMenu();

    void this.router.navigateByUrl('/login');
  }
}
