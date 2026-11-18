
import { Injectable, inject } from '@angular/core';

import {
  HttpClient,
  HttpHeaders,
  HttpParams
} from '@angular/common/http';

import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';

import { ApiResponse } from '../../../core/models/api-response.model';

import {
  ResultadoPaginado
} from '../../../core/models/resultado-paginado.model';

import { SesionService } from '../../../core/services/sesion.service';

import {
  AportePublicacion,
  ListarAportesPublicacionRequest
} from '../models/aportes-publicacion.model';

@Injectable({
  providedIn: 'root'
})
export class AportesPublicacionService {

  private readonly http = inject(HttpClient);
  private readonly sesionService = inject(SesionService);

  private readonly apiUrl =
    `${environment.apiUrl}/admin/publicaciones`;

  // ==========================================
  // GET: APORTES DE UNA PUBLICACIÓN
  // ==========================================

  listar(
    sorteoId: string,
    request: ListarAportesPublicacionRequest
  ): Observable<ApiResponse<ResultadoPaginado<AportePublicacion>>> {

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
      ApiResponse<ResultadoPaginado<AportePublicacion>>
    >(
      `${this.apiUrl}/${encodeURIComponent(sorteoId)}/aportes`,
      {
        headers,
        params
      }
    );
  }
}
