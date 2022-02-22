import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from 'src/environments/environment';
import { getCurrentProjectEnv } from "../utils/project-env";

@Injectable({
  providedIn: 'root'
})
export class DataSyncService {

  private readonly envId: number;
  private readonly baseUrl: string;

  constructor(private http: HttpClient) {
    const envId = getCurrentProjectEnv().envId;
    this.envId = envId;
    this.baseUrl = `${environment.url}/api/datasync/envs/${envId}`
  }

  getUploadUrl(): string {
    return `${this.baseUrl}/upload`;
  }

  getEnvironmentData(): Observable<any> {
    return this.http.get(`${this.baseUrl}/download`);
  }

  getUserBehaviorData(params: any): Observable<any> {
    return this.http.get(`${this.baseUrl}/user-behavior`, {params});
  }

  syncToRemote(settingId: string): Observable<string> {
    return this.http.put(`${this.baseUrl}/sync-to-remote?settingId=${settingId}`, { }, { responseType: 'text' });
  }
}
