import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from 'src/environments/environment';
import { IZeroCode } from "../pages/main/switch-manage/types/zero-code";

@Injectable({
  providedIn: 'root'
})
export class ZeroCodeService {
  baseUrl: string = environment.url + '/api/zero-code';

  constructor(
    private http: HttpClient
  ) {
  }

  getZeroCodes(envId: number, featureFlagId: string): Observable<IZeroCode> {
    const url = this.baseUrl + `/${envId}/${featureFlagId}`;
    return this.http.get<IZeroCode>(url);
  }

  upsert(param: IZeroCode): Observable<any> {
    const url = this.baseUrl;
    return this.http.post(url, param);
  }

  // resetTriggerToken(id: string, featureFlagId: string): Observable<IFlagTrigger> {
  //   const url = this.baseUrl + `/token/${id}/${featureFlagId}`;
  //   return this.http.put<IFlagTrigger>(url, {});
  // }

  // createTrigger(trigger: IFlagTrigger): Observable<IFlagTrigger> {
  //   return this.http.post<IFlagTrigger>(this.baseUrl, trigger);
  // }
}
