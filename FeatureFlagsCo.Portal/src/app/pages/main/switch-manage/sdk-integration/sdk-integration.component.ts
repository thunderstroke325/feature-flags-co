import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import { CSwitchParams, IFfParams } from '../types/switch-new';
import { AccountService } from 'src/app/services/account.service';

@Component({
  selector: 'setting',
  templateUrl: './sdk-integration.component.html',
  styleUrls: ['./sdk-integration.component.less']
})
export class SDKIntegrationComponent implements OnInit, OnDestroy {

  private destory$: Subject<void> = new Subject();

  public envSecret: string = '';
  public keyName: string = '';
  public id: string = '';

  constructor(
    private route: ActivatedRoute,
    private accountService: AccountService
  ) {
    this.route.data.pipe(map(res => res.switchInfo))
      .subscribe((result: CSwitchParams) => {
        const currentSwitch = new CSwitchParams(result).getSwicthDetail();
        const currentAccountProjectEnv = this.accountService.getCurrentAccountProjectEnv();
        this.envSecret = currentAccountProjectEnv.projectEnv.envSecret;
        this.keyName = currentSwitch.keyName;
        this.id = currentSwitch.id;
      })
  }

  ngOnInit(): void {
  }

  ngOnDestroy(): void {
    this.destory$.next();
    this.destory$.complete();
  }

}
