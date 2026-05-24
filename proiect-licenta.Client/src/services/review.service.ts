import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {firstValueFrom, Observable} from 'rxjs';
import {environment} from '../environments/environment';

export interface Review {
  appId: number,
  username: string,
  title: string,
  content: string,
  rating: number,
  postDate: string,
  editDate?: string
}

@Injectable({
  providedIn: 'root'
})
export class ReviewService {

  constructor(private http: HttpClient) { }

   async checkExistingReview(appId: number): Promise<boolean> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return await firstValueFrom(this.http.get<boolean>(`${environment.apiUrl}/api/Reviews/exists/${appId}`, {headers}));
  }

  async review(data:any, appId: number): Promise<Review> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return await firstValueFrom(this.http.post<any>(`${environment.apiUrl}/api/Reviews/${appId}`, data, {headers}));
  }

  async editReview(data:any, appId: number): Promise<Review> {
    console.log("editing");
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return await firstValueFrom(this.http.put<any>(`${environment.apiUrl}/api/Reviews/${appId}`, data, {headers}));
  }

  async deleteReview(appId: number): Promise<void> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return await firstValueFrom(this.http.delete<void>(`${environment.apiUrl}/api/Reviews/${appId}`, {headers}));
  }

  getAppReviews(appId: number): Observable<Review[]> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return this.http.get<Review[]>(`${environment.apiUrl}/api/Reviews/by_app/${appId}`, {headers});
  }

  getUserReview(appId:number): Observable<Review> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return this.http.get<any>(`${environment.apiUrl}/api/Reviews/own/${appId}`, {headers});
  }
}
