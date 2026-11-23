import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';
import { clienteGuard } from './core/guards/cliente.guard';
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
    path: 'aportes',
    loadComponent: () =>
      import(
        './features/mis-aportes/pages/mis-aportes/mis-aportes'
      ).then(m => m.MisAportes),
    canActivate: [clienteGuard]
  },

  {
    path: 'publicaciones/:sorteoId/ruleta',
    loadComponent: () =>
      import(
        './features/publicaciones/pages/ruleta/ruleta'
      ).then(m => m.Ruleta),
    canActivate: [adminGuard]
  },



  {
    path: 'publicaciones/:sorteoId/aportes',
    loadComponent: () =>
      import(
        './features/publicaciones/pages/aportes-publicacion/aportes-publicacion'
      ).then(m => m.AportesPublicacion),
    canActivate: [adminGuard]
  },

  {
    path: 'sorteos',
    loadComponent: () =>
      import(
        './features/sorteos-disponibles/pages/sorteos-disponibles'
      ).then(m => m.SorteosDisponibles),
    canActivate: [clienteGuard]
  },


  {
    path: 'home',
    title: 'Home | Corporación Charatec',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/home/pages/home/home')
        .then(m => m.Home)
  },

  {
    path: 'sorteos/:sorteoId/participar',
    loadComponent: () =>
      import(
        './features/sorteos-disponibles/pages/participar/participar'
      ).then(m => m.Participar),
    canActivate: [clienteGuard]
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
