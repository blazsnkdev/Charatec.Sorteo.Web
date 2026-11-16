import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';

import { ApiResponse } from '../../../core/models/api-response.model';

import { ClienteRegistrarRequest } from '../models/cliente-registrar-request.model';
import { ClienteRegistradoResponse } from '../models/cliente-registrado-response.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${environment.apiUrl}/auth`;

  registrar(
    request: ClienteRegistrarRequest
  ): Observable<ApiResponse<ClienteRegistradoResponse>> {

    return this.http.post<ApiResponse<ClienteRegistradoResponse>>(
      `${this.apiUrl}/register`,
      request
    );
  }
}
