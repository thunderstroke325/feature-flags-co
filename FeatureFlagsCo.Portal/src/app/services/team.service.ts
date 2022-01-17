import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { FfcService } from "./ffc.service";

@Injectable({
  providedIn: 'root'
})
export class TeamService {

  private _baseUrl: string;
  public get baseUrl(): string {
    if (!this._baseUrl) {
      const apiVersion = this.ffcService.variation('backend-api-version', 'v1');
      this._baseUrl = `${environment.url}/api/${apiVersion}/accounts/#accountId/members`;
    }

    return this._baseUrl;
  }

  constructor(
    private http: HttpClient,
    private ffcService: FfcService
  ) { }

  public getMembers(accountId: number): Observable<any> {
    const url = this.baseUrl.replace(/#accountId/ig, `${accountId}`);
    return this.http.get(url);
  }

  public searchMembers(accountId: number, searchText: string): Observable<any> {
    const url = this.baseUrl.replace(/#accountId/ig, `${accountId}`) + `/${searchText}`;
    return this.http.get(url);
  }

  public postAddMemberToAccount(accountId: number, params): Observable<any> {
    const url = this.baseUrl.replace(/#accountId/ig, `${accountId}`);
    return this.http.post(url, params);
  }

  public removeMember(accountId: number, userId: string): Observable<any> {
    const url = this.baseUrl.replace(/#accountId/ig, `${accountId}`) + `/${userId}`;
    return this.http.delete(url);
  }
}
