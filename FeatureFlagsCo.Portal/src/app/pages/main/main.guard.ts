import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { FfcService } from 'src/app/services/ffc.service';
import { getAuth } from 'src/app/utils';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class MainGuard implements CanActivate {

  constructor(
    private ffcService: FfcService
  ) {

  }

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
      this.initAuth();
      return true;
  }

   // 初始化登录人员信息
   private initAuth() {
    const auth = getAuth();

    this.ffcService.client.initialize(
      environment.projectEnvKey,
      {
        key: auth.email,
        email: auth.email,
        userName: auth.email.split("@")[0],
        customizeProperties: [{
          name: 'phoneNumber',
          value: auth.phoneNumber
        }]
      });
  }
}
