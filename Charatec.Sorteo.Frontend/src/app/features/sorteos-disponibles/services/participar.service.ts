
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

import {
  RegistrarParticipacionRequest,
  RegistrarParticipacionResponse
} from '../models/registrar-participacion.model';

@Injectable({
  providedIn: 'root'
})
export class ParticiparService {

  private readonly http = inject(HttpClient);

  private readonly sesionService = inject(SesionService);

  private readonly apiUrl =
    `${environment.apiUrl}/cliente/sorteos`;

  private obtenerHeaders(): HttpHeaders {
    const token = this.sesionService.obtenerToken();

    return new HttpHeaders({
      Authorization: `Bearer ${token ?? ''}`
    });
  }

  // ==========================================
  // GET: PREPARAR PARTICIPACIÓN
  // ==========================================

  obtenerConfiguracion(
    sorteoId: string
  ): Observable<ApiResponse<PrepararParticipacionResponse>> {

    return this.http.get<
      ApiResponse<PrepararParticipacionResponse>
    >(
      `${this.apiUrl}/${encodeURIComponent(sorteoId)}/participar`,
      {
        headers: this.obtenerHeaders()
      }
    );
  }

  // ==========================================
  // POST: REGISTRAR PARTICIPACIÓN YAPE
  // ==========================================

  registrarParticipacion(
    sorteoId: string,
    request: RegistrarParticipacionRequest
  ): Observable<ApiResponse<RegistrarParticipacionResponse>> {

    return this.http.post<
      ApiResponse<RegistrarParticipacionResponse>
    >(
      `${this.apiUrl}/${encodeURIComponent(sorteoId)}/participaciones`,
      request,
      {
        headers: this.obtenerHeaders()
      }
    );
  }
}
