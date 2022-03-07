import { Injectable } from '@angular/core';
import { Resolve, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { EnvironmentUserService } from 'src/app/services/environment-user.service';

@Injectable({
  providedIn: 'root'
})
export class SwitchUserResolver implements Resolve<boolean> {

  constructor(private userService: EnvironmentUserService) { }

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
