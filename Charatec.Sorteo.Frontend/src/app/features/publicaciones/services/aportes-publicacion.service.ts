import { Injectable, inject } from '@angular/core';
import {
  HttpClient,
  HttpHeaders,
  HttpParams
} from '@angular/common/http';
import { AportesPublicacionResponse } from '../models/aportes-publicacion.model';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models/api-response.model';
import {
  VerificarAporteRequest,
  VerificarAporteResponse
} from '../models/aportes-publicacion.model';

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


listar(
  sorteoId: string,
  request: ListarAportesPublicacionRequest
): Observable<ApiResponse<AportesPublicacionResponse>> {

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

  return this.http.get<ApiResponse<AportesPublicacionResponse>>(
    `${this.apiUrl}/${encodeURIComponent(sorteoId)}/aportes`,
    {
      headers,
      params
    }
  );
}


  obtenerResumen(sorteoId: string) {
    return this.http.get<ApiResponse<{
      tituloSorteo: string;
      totalConfirmado: number;
    }>>(
      `${this.apiUrl}/api/v1/admin/publicaciones/${sorteoId}/ruleta`
    );
  }

  verificarAporte(
    aporteId: string,
    request: VerificarAporteRequest
  ): Observable<ApiResponse<VerificarAporteResponse>> {

    const token = this.sesionService.obtenerToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token ?? ''}`//si el token es null, envia un string vacío
    });

    return this.http.patch<ApiResponse<VerificarAporteResponse>>(
      `${environment.apiUrl}/admin/aportes/${encodeURIComponent(aporteId)}/verificacion`,
      request,
      { headers }
    );
  }


}
