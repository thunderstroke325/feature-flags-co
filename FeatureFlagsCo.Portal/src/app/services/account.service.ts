import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { IAccount, IProjectEnv, IAccountProjectEnv } from '../config/types';
import { ProjectService } from './project.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  baseUrl: string = environment.url + '/api/accounts';

  accounts: IAccount[] = [];

  constructor(
    private http: HttpClient,
    private projectService: ProjectService
  ) {
  }

  // 获取 account 列表
  getAccounts(): Observable<any> {
    const url = this.baseUrl;
    return this.http.get(url);
  }

  // 获取单个 account 详情
  getAccount(params): Observable<any> {
    const url = this.baseUrl;
    return this.http.get(url, { params });
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

  /*
    初始登录时，判断是否有 account
    无 => 创建一个
    有 => 指向第一个
  */
  afterLoginSelectAccount() {
    this.getAccounts()
      .pipe()
      .subscribe(
        res => {
          if (!res.length) {
            this.postCreateAccount({
              organizationName: 'Default'
            }).pipe()
              .subscribe(
                res => {
                  this.changeAccount(res);
                }
              );
          }
        }
      );
  }

  changeAccount(account: IAccount) {
    if (!!account) {
      localStorage.setItem('current-account', JSON.stringify(account));
      const currentAccount = this.accounts.find(ws => ws.id == account.id);
      currentAccount.organizationName = account.organizationName;
    } else {
      localStorage.setItem('current-account', '');
    }

    window.location.reload();
  }

  setAccountName(account: IAccount) {
    if (!!account) {
      localStorage.setItem('current-account', JSON.stringify(account));
      const currentAccount = this.accounts.find(ws => ws.id == account.id);
      currentAccount.organizationName = account.organizationName;
    } else {
      localStorage.setItem('current-account', '');
    }

    this.projectService.currentProjectEnvChanged$.next();
  }

  // Promise version
  // getCurrentAccount1(): Promise<IAccount> {
  //   return new Promise((resolve, reject) => {
  //     const accountId = localStorage.getItem('account');
  //     if (this.accounts.length === 0) {
  //       this.getAccounts().subscribe(res => {
  //         this.accounts = res as IAccount[];
  //         if (accountId === undefined) {
  //           resolve(this.accounts[0]);
  //         } else {
  //           resolve(this.accounts.find(ws => ws.id == JSON.parse(accountId)));
  //         }
  //       });
  //     } else {
  //       resolve(this.accounts.find(ws => ws.id == JSON.parse(accountId)));
  //     }
  //   });
  // }

  // Observable version
  getCurrentAccount(): Observable<IAccount> {
    return Observable.create(observer => {
      const accountStr = localStorage.getItem('current-account');
      if (this.accounts.length === 0 || !accountStr) {
        this.getAccounts().subscribe(res => {
          this.accounts = res as IAccount[];
          if (!accountStr) {
            const currentAcount = this.accounts[0];
            localStorage.setItem('current-account', JSON.stringify(currentAcount));
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
    const account: IAccount = JSON.parse(localStorage.getItem('current-account'));
    const projectEnv: IProjectEnv = JSON.parse(localStorage.getItem('current-project'));
    return {
      account: this.accounts.find(x => x.id === account.id),
      projectEnv
    };
  }
}
