
import { Injectable, inject } from '@angular/core';

import {
  HttpClient,
  HttpHeaders
} from '@angular/common/http';

import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';

import {
  ApiResponse
} from '../../../core/models/api-response.model';

import {
  SesionService
} from '../../../core/services/sesion.service';

import {
  RuletaDatos
} from '../models/ruleta.model';

@Injectable({
  providedIn: 'root'
})
export class RuletaService {

  private readonly http = inject(HttpClient);

  private readonly sesionService = inject(SesionService);

  private readonly apiUrl =
    `${environment.apiUrl}/admin/publicaciones`;

  // ==========================================
  // GET: DATOS PARA LA RULETA
  // ==========================================

  obtenerDatos(
    sorteoId: string
  ): Observable<ApiResponse<RuletaDatos>> {

    const token = this.sesionService.obtenerToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token ?? ''}`
    });

    return this.http.get<ApiResponse<RuletaDatos>>(
      `${this.apiUrl}/${encodeURIComponent(sorteoId)}/ruleta`,
      { headers }
    );
  }
}
