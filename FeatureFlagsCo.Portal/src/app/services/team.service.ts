import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class TeamService {

  baseUrl: string = environment.url + '/api/accounts/#accountId/members';

  constructor(
    private http: HttpClient
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
