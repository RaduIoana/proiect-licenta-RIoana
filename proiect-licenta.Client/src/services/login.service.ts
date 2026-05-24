import {HttpClient, HttpHeaders} from '@angular/common/http';
import { Injectable } from '@angular/core';
import {catchError, map, Observable, of, switchMap} from 'rxjs';
import {environment} from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class LoginService {

  constructor(private http: HttpClient) {}

  onLogin(data: any): Observable<any> {
    return this.http.post<any>(`${environment.apiUrl}/api/Auth/login`, data);
  }

  onRegister(data: any): Observable<any> {
    return this.http.post<any>(`${environment.apiUrl}/api/Auth/register`, data);
  }

  logout(): void {
    sessionStorage.removeItem("accessToken");
    sessionStorage.removeItem("role");
    sessionStorage.removeItem("username");
    window.location.reload();
  }

}
