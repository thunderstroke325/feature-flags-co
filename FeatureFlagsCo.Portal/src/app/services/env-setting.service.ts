import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { environment } from 'src/environments/environment';
import { getCurrentProjectEnv } from "../utils/project-env";
import { EnvironmentSetting } from "../config/types";
import { Observable } from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class EnvSettingService {

  private readonly envId: number;
  private readonly baseUrl: string;

  constructor(private http: HttpClient) {
    const envId = getCurrentProjectEnv().envId;
    this.envId = envId;
    this.baseUrl = `${environment.url}/api/v2/envs/${envId}/settings`
  }

  get(type: string): Observable<EnvironmentSetting[]> {
    return this.http.get<EnvironmentSetting[]>(this.baseUrl, {params: {type}});
  }

  upsert(settings: EnvironmentSetting[]): Observable<EnvironmentSetting[]> {
    return this.http.post<EnvironmentSetting[]>(this.baseUrl, settings);
  }

  delete(settingId: string): Observable<EnvironmentSetting[]> {
    return this.http.delete<EnvironmentSetting[]>(this.baseUrl, {params: {settingId}});
  }
}
