
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { SesionService } from '../services/sesion.service';

export const adminGuard: CanActivateFn = () => {

  const sesionService = inject(SesionService);
  const router = inject(Router);

  // Verificar que el usuario esté autenticado
  if (!sesionService.autenticado()) {
    return router.createUrlTree(['/login']);
  }

  // Verificar rol Administrador
  if (!sesionService.tieneRol('Administrador')) {
    return router.createUrlTree(['/']);
  }

  // Autorizar acceso
  return true;
};
