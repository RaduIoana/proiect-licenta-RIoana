import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {Observable} from 'rxjs';
import {environment} from '../environments/environment';

export interface PaymentRecord {
  id: number;
  appId: string;
  paymentDT: string;
  paymentType: number;
  paymentAmount: number;
  paymentStatus: string;
  canRefund?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class PaymentRecordsService {

  constructor(private http: HttpClient) { }

  getUserPaymentHistory(): Observable<PaymentRecord[]> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return this.http.get<PaymentRecord[]>(`${environment.apiUrl}/api/PaymentRecords/get_payRecords/user`, {headers});
  }
}
