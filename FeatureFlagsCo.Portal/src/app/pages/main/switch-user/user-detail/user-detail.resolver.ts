import { Injectable } from '@angular/core';
import {
  Router, Resolve,
  RouterStateSnapshot,
  ActivatedRouteSnapshot
} from '@angular/router';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { UserService } from 'src/app/services/user.service';

@Injectable({
  providedIn: 'root'
})
export class SwitchUserResolver implements Resolve<boolean> {

  constructor(
    private userService: UserService
  ) {

  }

  resolve(route: ActivatedRouteSnapshot): Observable<boolean> {
    const id: string = decodeURIComponent(route.params['id']);

    return this.userService.getEnvUserDetail({ id })
      .pipe(
        map(res => {
          this.userService.setCurrentUser(res);
          return res;
        })
      );
  }
}
