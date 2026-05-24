import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {firstValueFrom, map, Observable} from 'rxjs';
import {environment} from '../environments/environment';

export interface Refund {
  id: number,
  paymentId: number,
  paymentType: string,
  username: string,
  walletAddress: string,
  requestDT: string,
  appId: number,
  appName: string,
  sum: number,
  status: string,
}

@Injectable({
  providedIn: 'root'
})
export class RefundService {

  constructor(private http: HttpClient) { }

  getAllRefundsAdmin(): Observable<Refund[]> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return this.http.get<Refund[]>(`${environment.apiUrl}/api/refunds/get_all_refunds/admin`, {headers});
  }

  async requestPaymentRefund(paymentId: number) {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return await firstValueFrom(this.http.post<any>(`${environment.apiUrl}/api/refunds/refund/${paymentId}`, {}, {headers}));
  }

  async grantRefund(refundId: number) {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return await firstValueFrom(this.http.post<any>(`${environment.apiUrl}/api/refunds/grant/${refundId}`, {}, {headers}));
  }
}
