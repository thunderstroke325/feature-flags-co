import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve } from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { SwitchService } from '../../../../services/switch.service';
import { CSwitchParams } from '../types/switch-new';

@Injectable()
export class TargetConditionsResolver implements Resolve<CSwitchParams> {

  constructor(
    private switchServe: SwitchService
  ) { }

  resolve(route: ActivatedRouteSnapshot): Observable<CSwitchParams> {
    const id: string = decodeURIComponent(route.params['id']);
    return this.switchServe.getSwitchDetail(id)
      .pipe(
        map(res => res)
      );
  }
}
