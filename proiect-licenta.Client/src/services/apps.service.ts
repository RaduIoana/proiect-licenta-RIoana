import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders, HttpParams} from '@angular/common/http';
import {firstValueFrom, Observable} from 'rxjs';
import {environment} from '../environments/environment';
import {LicenseService} from './license.service';

export interface App {
  id: number;
  icon?: AppImage;
  devId?: string;
  name: string;
  description: string;
  price: number;
  launchDate: string;
  rating?: number;
  discount?: number;
}

export interface AppFilters {
  categories?: number[];
  sortBy?: string;
  order?: string;
  library: boolean;
  devApps: boolean;
}

export interface AppImage {
  id: number;
  path: string;
}

@Injectable({
  providedIn: 'root'
})

export class AppsService {

  constructor(private http: HttpClient, private licenseService: LicenseService) {}

  async getAppPrice(id: number): Promise<number> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return await firstValueFrom(this.http.get<number>(`${environment.apiUrl}/api/Appstore/price/${id}`, {headers}));
  }

  async getAppRating(id: number): Promise<number> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return await firstValueFrom(this.http.get<number>(`${environment.apiUrl}/api/Apps/rating/${id}`, {headers}));
  }

  async getAppIcon(id: number): Promise<AppImage> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    // trying to fix api returning array with 1 element
    return (await firstValueFrom(this.http.get<AppImage[]>(`${environment.apiUrl}/api/file/images/icon/${id}`, {headers})))[0];
  }

  getAppScreenshots(id: number): Observable<AppImage[]> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return this.http.get<AppImage[]>(`${environment.apiUrl}/api/file/images/screenshot/${id}`, {headers});
  }

  async postAppIcon(id: number, formData: FormData): Promise<string> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return await firstValueFrom(this.http.post<any>(`${environment.apiUrl}/api/file/images/icon/${id}`,
      formData, {headers}));
  }

  async postAppScreenshots(id: number, formData: FormData): Promise<string[]> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return await firstValueFrom(this.http.post<any>(`${environment.apiUrl}/api/file/images/screenshot/${id}`,
      formData, {headers}));
  }

  async postAppFile(id: number, formData: FormData): Promise<string> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return await firstValueFrom(this.http.post<any>(`${environment.apiUrl}/api/file/ipfs/${id}`,
      formData, {headers}));
  }

  async editAppIcon(id: number, formData: FormData): Promise<string> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return await firstValueFrom(this.http.put<any>(`${environment.apiUrl}/api/file/images/icon/${id}`,
      formData, {headers}));
  }

  async editAppScreenshots(id: number, formData: FormData): Promise<string[]> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return await firstValueFrom(this.http.put<any>(`${environment.apiUrl}/api/file/images/screenshot/${id}`,
      formData, {headers}));
  }

  getApps(filters: AppFilters): Observable<App[]> {
    let params = new HttpParams();
    if (filters.categories?.length) {
      filters.categories.forEach((cat) => {
        params = params.append('categories', cat.toString());
      });
    }

    if (filters.sortBy) {
      params = params.set('sortBy', filters.sortBy);
    }

    if (filters.order) {
      params = params.set('order', filters.order);
    }

    params = params.set('library', filters.library);
    params = params.set('devApps', filters.devApps);

    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return this.http.get<App[]>(`${environment.apiUrl}/api/Apps/get_apps/`, {params, headers});
  }

  getAppById(id: number): Observable<App> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return this.http.get<App>(`${environment.apiUrl}/api/Apps/${id}`, {headers});
  }

  async createApp(app: App, categories: number[]): Promise<number> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    const result: App = await firstValueFrom(this.http.post<any>(`${environment.apiUrl}/api/Apps`, {
      DevId: app.devId,
      Name: app.name,
      Description: app.description,
      Price: app.price,
      LaunchDate: app.launchDate,
      Rating: 0,
      Discount: 0
    }, {headers}));

    await firstValueFrom(this.http.put<any>(`${environment.apiUrl}/api/Categories/assign/${result.id}`,
      categories, {headers}));

    return result.id;
  }

  async editApp(app: App, categories: number[]): Promise<any> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    const result: App = await firstValueFrom(this.http.put<any>(`${environment.apiUrl}/api/Apps`, {
      Id: app.id,
      DevId: app.devId,
      Name: app.name,
      Description: app.description,
      Price: app.price,
      LaunchDate: app.launchDate,
      Rating: app.rating,
      Discount: app.discount
    }, {headers}));

    await firstValueFrom(this.http.put<any>(`${environment.apiUrl}/api/Categories/assign/${result.id}`,
      categories, {headers}));
  }

  async appIsOwnedByUser(id: number): Promise<boolean> {
    if (sessionStorage.getItem('accessToken') == null){
      return false;
    }
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return await firstValueFrom(this.http.get<boolean>(`${environment.apiUrl}/api/Apps/appInLibraryCheck/${id}`, {headers}));
  }

  async userIsAppDeveloper(id: number): Promise<boolean> {
    if (sessionStorage.getItem('accessToken') == null){
      return false;
    }
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    return await firstValueFrom(this.http.get<boolean>(`${environment.apiUrl}/api/Apps/isAppDeveloper/${id}`, {headers}));
  }

  async addFreeAppToLibrary(app: App): Promise<string | null> {
    try{
      const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);

      this.http.get(`${environment.apiUrl}/api/Appstore/ipfs`, {headers});

      const record = await firstValueFrom(this.http.post<any>(`${environment.apiUrl}/api/PaymentRecords/`, {
          AppId: app.id,
          PaymentType: 0
        },
        {headers}));

      await this.licenseService.mintFreeLicense(record.appId);

      await firstValueFrom(this.http.post<any>(`${environment.apiUrl}/api/Library/`, {
          AppId: record.appId,
          PaymentId: record.id
        },
        {headers}));

      return "App added successfully.";
    } catch (error:any){
      console.error("Error adding app", error.message);
      return "Error adding app to library.";
    }
  }

  async addPaidAppToLibrary(appId: number, recordId: number): Promise<string | null> {
    try{
      const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
      await firstValueFrom(this.http.post<any>(`${environment.apiUrl}/api/Library/`, {
          AppId: appId,
          PaymentId: recordId
        },
        {headers}));
      return "App added successfully.";
    } catch (error){
      console.error("Error adding app", error);
      return "Error adding app to library.";
    }
  }

  async removeAppFromLibrary(appId: number): Promise<void> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    await firstValueFrom(this.http.delete<void>(`${environment.apiUrl}/api/Library/${appId}`, {headers}));
  }

  async installApp(appId:number): Promise<void> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    const response = await firstValueFrom(this.http.get(`${environment.apiUrl}/api/Appstore/install/${appId}`,
      {
        headers,
        responseType: 'blob',
        observe: 'response'
      }));

    const blob = response.body as Blob;
    const contentDisposition = response.headers.get('Content-Disposition');
    console.log(contentDisposition);
    //fallback
    let fileName = "app.exe";

    //assigning filename from content disp
    if (contentDisposition) {
      const match = contentDisposition.match(/filename="?([^"]+)"?/);
      if (match && match[1]) {
        fileName = match[1];
      }
    }

    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.click();
    window.URL.revokeObjectURL(url);
  }

  async revokeFreeApp(appId: number): Promise<void> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${sessionStorage.getItem('accessToken')}`);
    await firstValueFrom(this.http.delete<void>(`${environment.apiUrl}/api/License/revoke/free/${appId}`, {headers}));
  }
}
