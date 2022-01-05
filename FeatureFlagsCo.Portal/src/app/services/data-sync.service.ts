import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DataSyncService {
  baseUrl: string = environment.url + '/api/datasync/envs/#envId/';

  constructor(
    private http: HttpClient
  ) {
  }

  getUploadUrl(envId: number): string {
    return this.baseUrl.replace(/#envId/ig, `${envId}`) + 'upload';
  }

  getEnvironmentData(envId: number): Observable<any> {
    const url = this.baseUrl.replace(/#envId/ig, `${envId}`) + `download`;
    return this.http.get(url);
  }

  getUserBehaviorData(envId: number, params: any): Observable<any> {
    const url = this.baseUrl.replace(/#envId/ig, `${envId}`) + `user-behavior`;
    return this.http.get(url, {params});
  }
}
