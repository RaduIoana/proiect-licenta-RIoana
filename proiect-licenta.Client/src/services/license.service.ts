import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {firstValueFrom} from 'rxjs';
import {environment} from '../environments/environment';
import {TransactionResponse} from 'ethers';
import {App} from './apps.service';

@Injectable({
  providedIn: 'root'
})
export class LicenseService {

  constructor(private http: HttpClient) { }

  async insertPayment(tx: TransactionResponse, app: App): Promise<number> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    const response = await firstValueFrom(this.http.post<any>(`${environment.apiUrl}/api/PaymentRecords/`, {
      AppId: app.id,
      PaymentType: 1,
      Tx: tx.hash
    }, {headers}))
    return response.id;
  }

  async confirmPayment(id: number): Promise<any> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return await firstValueFrom(this.http.post<any>(`${environment.apiUrl}/api/PaymentRecords/confirm/${id}`, null, {headers}));
  }

  async mintLicense(id: number, recordId: number): Promise<any> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return await firstValueFrom(this.http.get<any>(`${environment.apiUrl}/api/License/mint/${id}/${recordId}`, {headers}));
  }

  async mintFreeLicense(id: number): Promise<any> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return await firstValueFrom(this.http.get<any>(`${environment.apiUrl}/api/License/mintFree/${id}`, {headers}));
  }
}
