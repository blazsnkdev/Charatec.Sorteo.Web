
import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'registro',
    title: 'Crear cuenta | Sorteos Charatec',
    loadComponent: () =>
      import('./features/auth/pages/registro/registro')
        .then(m => m.Registro)
  },
  {
    path: '',
    redirectTo: 'registro',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: 'registro'
  }
];
