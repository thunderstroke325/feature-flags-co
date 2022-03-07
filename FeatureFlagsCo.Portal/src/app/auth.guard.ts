import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, Router } from '@angular/router';
import { getAuth } from 'src/app/utils';
import { FfcService } from './services/ffc.service';
import { environment } from 'src/environments/environment';
import { LOGIN_REDIRECT_URL } from "./utils/localstorage-keys";

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(
    private router: Router,
    private ffcService: FfcService
  ) { }

  async canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Promise<boolean | UrlTree> {

    return await this.checkLogin(state.url);
  }

  async checkLogin(url: string): Promise<true | UrlTree> {
    const auth = getAuth();
    if (auth) {
      await this.ffcService.initialize({
        secret: environment.projectEnvKey,
        user: {
          id: auth.id,
          email: auth.email,
          userName: auth.userName,
          customizedProperties: [{
            name: 'phoneNumber',
            value: auth.phoneNumber
          }]
        }
      });

      return true;
    }

    localStorage.setItem(LOGIN_REDIRECT_URL, url);
    return this.router.parseUrl('/login');
  }
}
