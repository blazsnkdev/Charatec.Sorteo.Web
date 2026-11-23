
import { inject, Injectable } from '@angular/core';
import {
  HttpClient,
  HttpHeaders,
  HttpParams
} from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';

import { ApiResponse } from '../../../core/models/api-response.model';
import { ResultadoPaginado } from '../../../core/models/resultado-paginado.model';
import { SorteoListado } from '../models/sorteo-listado.model';
import { SesionService } from '../../../core/services/sesion.service';
import {
  CrearSorteoRequest,
  SorteoCreadoResponse
} from '../models/crear-sorteo-request.model';


import {
  ActualizarSorteoRequest,
  SorteoPublicadoResponse
} from '../models/actualizar-sorteo-request.model';


@Injectable({
  providedIn: 'root'
})
export class PublicacionesService {

  private readonly http = inject(HttpClient);
  private readonly sesionService = inject(SesionService);

  private readonly apiUrl = `${environment.apiUrl}/sorteo`;

  listar(
    pagina: number = 1,
    tamanoPagina: number = 10,
    estado: number | null = null
  ): Observable<ApiResponse<ResultadoPaginado<SorteoListado>>> {

    let params = new HttpParams()
      .set('pagina', pagina)
      .set('tamanoPagina', tamanoPagina);

    // Si estado es null, la API devuelve TODOS
    if (estado !== null) {
      params = params.set('estado', estado);
    }

    // Obtener el JWT de la sesión actual
    const token = this.sesionService.obtenerToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token ?? ''}`
    });

    return this.http.get<
      ApiResponse<ResultadoPaginado<SorteoListado>>
    >(
      this.apiUrl,
      { params, headers }
    );
  }


  obtenerPorId(
    id: string
  ): Observable<ApiResponse<SorteoListado>> {

    const token = this.sesionService.obtenerToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token ?? ''}`
    });

    return this.http.get<ApiResponse<SorteoListado>>(
      `${this.apiUrl}/${id}`,
      { headers }
    );
  }


actualizarPortada(sorteoId: string, archivo: File) {
  const formData = new FormData();
  formData.append('archivo', archivo);

  return this.http.patch<
    ApiResponse<{
      sorteoId: string;
      imagenPortadaUrl: string;
    }>
  >(
    `${this.apiUrl}/${sorteoId}/portada`,
    formData
  );
}

  crear(
    request: CrearSorteoRequest
  ): Observable<ApiResponse<SorteoCreadoResponse>> {

    const token = this.sesionService.obtenerToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token ?? ''}`
    });

    return this.http.post<ApiResponse<SorteoCreadoResponse>>(
      this.apiUrl,
      request,
      { headers }
    );
  }

obtenerUrlImagen(ruta: string | null | undefined): string {
  if (!ruta) return '';

  if (/^https?:\/\//i.test(ruta)) {
    return ruta;
  }

  // La URL base de tu API debe ser absoluta,
  // por ejemplo http://localhost:5000/api/v1/sorteo
  const origenApi = new URL(this.apiUrl).origin;

  return new URL(ruta, origenApi).href;
}

  subirPortada(
    sorteoId: string,
    archivo: File
  ): Observable<ApiResponse<{ imagenPortadaUrl: string }>> {

    const token = this.sesionService.obtenerToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token ?? ''}`
    });

    const formData = new FormData();

    formData.append('archivo', archivo, archivo.name);

    return this.http.post<
      ApiResponse<{ imagenPortadaUrl: string }>
    >(
      `${this.apiUrl}/${sorteoId}/portada`,
      formData,
      { headers }
    );
  }


  // ==========================================
  // ACTUALIZAR BORRADOR - PUT
  // ==========================================

  actualizar(
    id: string,
    request: ActualizarSorteoRequest
  ): Observable<ApiResponse<SorteoListado>> {

    const token = this.sesionService.obtenerToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token ?? ''}`
    });

    return this.http.put<ApiResponse<SorteoListado>>(
      `${this.apiUrl}/${id}`,
      request,
      { headers }
    );
  }


  // ==========================================
  // PUBLICAR SORTEO - PATCH
  // ==========================================

  publicar(
    id: string
  ): Observable<ApiResponse<SorteoPublicadoResponse>> {

    const token = this.sesionService.obtenerToken();

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token ?? ''}`
    });

    return this.http.patch<
      ApiResponse<SorteoPublicadoResponse>
    >(
      `${this.apiUrl}/${id}/publicar`,
      null,
      { headers }
    );
  }



}
