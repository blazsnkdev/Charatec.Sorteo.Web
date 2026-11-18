
import { Injectable, inject } from '@angular/core';
import {
  HttpClient,
  HttpHeaders
} from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';

import { ApiResponse } from '../../../core/models/api-response.model';

import { SesionService } from '../../../core/services/sesion.service';

import {
  PrepararParticipacionResponse
} from '../models/preparar-participacion.model';

@Injectable({
  providedIn: 'root'
})
export class ParticiparService {

  private readonly http = inject(HttpClient);

  private readonly sesionService = inject(SesionService);

  private readonly apiUrl =
    `${environment.apiUrl}/cliente/sorteos`;

  // ==========================================
  // GET: INFORMACIÓN PARA PARTICIPAR
  // ==========================================

  obtenerConfiguracion(
    sorteoId: string
  ): Observable<ApiResponse<PrepararParticipacionResponse>> {

    // Obtener JWT de la sesión actual
    const token = this.sesionService.obtenerToken();

    // Enviar JWT al backend
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token ?? ''}`
    });

    return this.http.get<
      ApiResponse<PrepararParticipacionResponse>
    >(
      `${this.apiUrl}/${encodeURIComponent(sorteoId)}/participar`,
      { headers }
    );
  }
}
