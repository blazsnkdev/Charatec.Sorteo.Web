
import { Injectable, inject } from '@angular/core';

import {
  HttpClient,
  HttpHeaders,
  HttpParams
} from '@angular/common/http';

import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';

import { ApiResponse } from '../../../core/models/api-response.model';

import { SesionService } from '../../../core/services/sesion.service';

import {
  AporteListado,
  ResultadoPaginado,
  ListarMisAportesParametros
} from '../models/mis-aportes.model';

@Injectable({
  providedIn: 'root'
})
export class MisAportesService {

  private readonly http = inject(HttpClient);

  private readonly sesionService = inject(SesionService);

  // ==========================================
  // API DE APORTES DEL CLIENTE
  // ==========================================

  private readonly apiUrl =
    `${environment.apiUrl}/cliente/aporte`;

  // ==========================================
  // GET: LISTADO PAGINADO
  // ==========================================

  listarMisAportes(
    request: ListarMisAportesParametros
  ): Observable<ApiResponse<ResultadoPaginado<AporteListado>>> {

    const token = this.sesionService.obtenerToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token ?? ''}`
    });

    let params = new HttpParams()
      .set('pagina', request.pagina.toString())
      .set('tamanoPagina', request.tamanoPagina.toString());

    if (request.estado) {
      params = params.set('estado', request.estado);
    }

    return this.http.get<
      ApiResponse<ResultadoPaginado<AporteListado>>
    >(
      this.apiUrl,
      {
        headers,
        params
      }
    );
  }
}
