import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve } from '@angular/router';
import { Observable, of } from 'rxjs';
import { take, mergeMap } from 'rxjs/operators';
import { ProjectService } from 'src/app/services/project.service';
import { AccountService } from 'src/app/services/account.service';
import { IAccount, IProjectEnv } from 'src/app/config/types';

@Injectable()
export class AccountProjectEnvResolver implements Resolve<any> {

  public isFirstInto: boolean = true;

  constructor(
    private projectService: ProjectService,
    private accountService: AccountService,
  ) { }

  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    return this.accountService.getCurrentAccount().pipe(
        take(1),
        mergeMap((account: IAccount) => {
            return this.projectService.getCurrentProjectAndEnv(account.id).pipe(
              take(1),
              mergeMap((projectEnv: IProjectEnv) => {
                return of({});
              })
            )
          }
        )
      );
  }
}
