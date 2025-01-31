import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { IAccount, IProjectEnv, IAccountProjectEnv } from '../config/types';
import { ProjectService } from './project.service';
import { FfcService } from "./ffc.service";
import { CURRENT_ACCOUNT, CURRENT_PROJECT } from "@utils/localstorage-keys";

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  private _baseUrl: string;
  public get baseUrl(): string {
    if (!this._baseUrl) {
      const apiVersion = this.ffcService.variation('backend-api-version', 'v1');
      this._baseUrl = `${environment.url}/api/${apiVersion}/accounts`;
    }

    return this._baseUrl;
  }

  accounts: IAccount[] = [];

  constructor(
    private http: HttpClient,
    private projectService: ProjectService,
    private ffcService: FfcService
  ) { }

  // 获取 account 列表
  getAccounts(): Observable<any> {
    const url = this.baseUrl;
    return this.http.get(url);
  }

// 创建 account
  postCreateAccount(params): Observable<any> {
    const url = this.baseUrl;
    return this.http.post(url, params);
  }

  // 更新 account
  putUpdateAccount(params): Observable<any> {
    const url = this.baseUrl;
    return this.http.put(url, params);
  }

  changeAccount(account: IAccount) {
    if (!!account) {
      localStorage.setItem(CURRENT_ACCOUNT(), JSON.stringify(account));
      const currentAccount = this.accounts.find(ws => ws.id == account.id);
      currentAccount.organizationName = account.organizationName;
    } else {
      localStorage.setItem(CURRENT_ACCOUNT(), '');
    }

    this.projectService.clearCurrentProjectEnv();
    window.location.reload();
  }

  setAccountName(account: IAccount) {
    if (!!account) {
      localStorage.setItem(CURRENT_ACCOUNT(), JSON.stringify(account));
      const currentAccount = this.accounts.find(ws => ws.id == account.id);
      currentAccount.organizationName = account.organizationName;
    } else {
      localStorage.setItem(CURRENT_ACCOUNT(), '');
    }
  }

  getCurrentAccount(): Observable<IAccount> {
    return new Observable(observer => {
      const accountStr = localStorage.getItem(CURRENT_ACCOUNT());
      if (this.accounts.length === 0 || !accountStr) {
        this.getAccounts().subscribe(res => {
          this.accounts = res as IAccount[];
          if (!accountStr) {
            const currentAcount = this.accounts[0];
            localStorage.setItem(CURRENT_ACCOUNT(), JSON.stringify(currentAcount));
            observer.next(currentAcount);
          } else {
            observer.next(this.accounts.find(ws => ws.id == JSON.parse(accountStr).id));
          }
        });
      } else {
        observer.next(this.accounts.find(ws => ws.id == JSON.parse(accountStr).id));
      }
    });
  }

  getCurrentAccountProjectEnv(): IAccountProjectEnv {
    const account: IAccount = JSON.parse(localStorage.getItem(CURRENT_ACCOUNT()));
    const projectEnv: IProjectEnv = JSON.parse(localStorage.getItem(CURRENT_PROJECT()));
    return {
      account: this.accounts.find(x => x.id === account.id),
      projectEnv
    };
  }
}
