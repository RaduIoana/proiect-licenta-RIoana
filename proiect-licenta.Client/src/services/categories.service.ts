import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {environment} from '../environments/environment';
import {firstValueFrom, Observable} from 'rxjs';

export interface Category {
  id: number;
  name: string;
}

@Injectable({
  providedIn: 'root'
})
export class CategoriesService {

  constructor(private http: HttpClient) { }

  getAllCategories(): Observable<Category[]> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return this.http.get<Category[]>(`${environment.apiUrl}/api/Categories/get_all_categories`, {headers})
  }

  getAppCategories(appId: number): Observable<Category[]> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return this.http.get<Category[]>(`${environment.apiUrl}/api/Categories/forApp/${appId}`, {headers})
  }

  async createCategory(name: string): Promise<Category> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return await firstValueFrom(this.http.post<any>(`${environment.apiUrl}/api/Categories/`, {
      Name: name
    }, {headers}));
  }
}
