import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [

  {
    path: 'registro',
    title: 'Crear cuenta | Corporación Charatec',
    loadComponent: () =>
      import('./features/auth/pages/registro/registro')
        .then(m => m.Registro)
  },

  {
    path: 'publicaciones',
    title: 'Publicaciones | Charatec Sorteos',
    canActivate: [adminGuard],
    loadComponent: () =>
      import(
        './features/publicaciones/pages/publicaciones'
      ).then(m => m.Publicaciones)
  },
  {
    path: 'login',
    title: 'Iniciar sesión | Corporación Charatec',
    loadComponent: () =>
      import('./features/auth/pages/login/login')
        .then(m => m.Login)
  },

  {
    path: 'home',
    title: 'Home | Corporación Charatec',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/home/pages/home/home')
        .then(m => m.Home)
  },

  // =====================================
  // RUTA PRINCIPAL
  // =====================================
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },

  // =====================================
  // RUTAS NO ENCONTRADAS
  // =====================================
  {
    path: '**',
    redirectTo: 'login'
  }

];
