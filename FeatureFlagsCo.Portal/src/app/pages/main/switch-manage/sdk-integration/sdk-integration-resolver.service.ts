import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve } from '@angular/router';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { SwitchService } from '../../../../services/switch.service';
import { IFfParams } from '../types/switch-new';

@Injectable()
export class SDKIntegrationResolver implements Resolve<IFfParams> {

  public isFirstInto: boolean = true;

  constructor(
    private switchServe: SwitchService
  ) { }
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id: string = decodeURIComponent(route.params['id']);
    return this.switchServe.getSwitchDetail(id)
      .pipe(
        map(res => {
          this.switchServe.setCurrentSwitch(res.ff);
          return res;
        })
      );;
  }
}