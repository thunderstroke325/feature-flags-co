import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from './services/auth.service';
import { getAuth } from 'src/app/utils';
import { AccountService } from './services/account.service';
import { FfcService } from './services/ffc.service';
import { environment } from 'src/environments/environment';


@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(
    private router: Router,
    private authService: AuthService,
    private accountService: AccountService,
    private ffcService: FfcService
  ) { }

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {

    return this.checkLogin(state.url);
  }

  checkLogin(url: string): true | UrlTree {
    const auth = getAuth();
    if (auth) {
      this.ffcService.initialize(
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

      this.accountService.afterLoginSelectAccount();
      return true;
    }

    this.authService.redirectUrl = url;

    return this.router.parseUrl('/login');
  }

}
