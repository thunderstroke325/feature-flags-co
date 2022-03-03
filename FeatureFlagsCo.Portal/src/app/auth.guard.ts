import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, Router } from '@angular/router';
import { AuthService } from './services/auth.service';
import { getAuth } from 'src/app/utils';
import { FfcService } from './services/ffc.service';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(
    private router: Router,
    private authService: AuthService,
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
          id: auth.email,
          email: auth.email,
          userName: auth.email.split("@")[0],
          customizedProperties: [{
            name: 'phoneNumber',
            value: auth.phoneNumber
          }]
        }
      });

      return true;
    }

    this.authService.redirectUrl = url;

    return this.router.parseUrl('/login');
  }

}
