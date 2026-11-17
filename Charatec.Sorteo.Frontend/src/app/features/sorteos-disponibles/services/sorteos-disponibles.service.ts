
import { inject, Injectable } from '@angular/core';
import {
  HttpClient,
  HttpHeaders,
  HttpParams
} from '@angular/common/http';

import { Observable } from 'rxjs';

import {
  ApiResponse,
  ResultadoPaginado,
  SorteoDisponible
} from '../models/sorteos-disponibles';

import { SesionService } from '../../../core/services/sesion.service';

@Injectable({
  providedIn: 'root'
})
export class SorteosDisponiblesService {

  private readonly http = inject(HttpClient);
  private readonly sesionService = inject(SesionService);

  private readonly apiUrl =
    'https://localhost:7161/api/v1/cliente/sorteos';

  listar(
    pagina: number = 1,
    tamanoPagina: number = 10
  ): Observable<ApiResponse<ResultadoPaginado<SorteoDisponible>>> {

    const token = this.sesionService.obtenerToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token ?? ''}`
    });

    const params = new HttpParams()
      .set('pagina', pagina.toString())
      .set('tamanoPagina', tamanoPagina.toString());

    return this.http.get<
      ApiResponse<ResultadoPaginado<SorteoDisponible>>
    >(
      this.apiUrl,
      {
        headers,
        params
      }
    );
  }
}
