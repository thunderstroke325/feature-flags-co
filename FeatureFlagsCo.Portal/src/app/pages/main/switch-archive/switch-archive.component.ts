import { Component, OnDestroy, OnInit } from '@angular/core';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzModalService } from 'ng-zorro-antd/modal';
import { Subject } from 'rxjs';
import { SwitchService } from 'src/app/services/switch.service';
import { ISwitchArchive } from './types/switch-archive';
import { ProjectService } from 'src/app/services/project.service';
import { AccountService } from 'src/app/services/account.service';

@Component({
  selector: 'app-switch-archive',
  templateUrl: './switch-archive.component.html',
  styleUrls: ['./switch-archive.component.less']
})
export class SwitchArchiveComponent implements OnInit, OnDestroy {

  destory$: Subject<void> = new Subject();
  currentEnvId: number;
  currentAccountId: number;
  searchValue: string = '';
  isLoading: boolean = false;
  switchLists: ISwitchArchive[] = [];

  get switchs() {
    return this.switchLists.filter((st: ISwitchArchive) => st.name.indexOf(this.searchValue) >= 0);
  }

  constructor(
    private switchService: SwitchService,
    private accountService: AccountService,
    private modal: NzModalService,
    private msg: NzMessageService
  ) { }

  ngOnInit(): void {
    const currentAccountProjectEnv = this.accountService.getCurrentAccountProjectEnv();
    this.currentAccountId = currentAccountProjectEnv.account.id;
    this.currentEnvId = currentAccountProjectEnv.projectEnv.envId;
    this.fetchArchiveSwitchs(this.currentEnvId);
  }

  ngOnDestroy(): void {
    this.destory$.next();
    this.destory$.complete();
  }

  fetchArchiveSwitchs(id: number) {
    this.isLoading = true;
    this.switchService.getArchiveSwitch(id)
      .pipe()
      .subscribe(
        res => {
          this.isLoading = false;
          this.switchLists = res;
        },
        _ => {
          this.isLoading = false;
        }
      );
  }

  // 复位开关
  onUnarchiveClick(st: ISwitchArchive) {
    this.modal.create({
      nzContent: '确定复位开关吗？复位后开关状态为关闭， 以避免给线上环境造成影响。',
      nzOkText: '确认复位',
      nzOnOk: () => {
        this.switchService.unarchiveEnvFeatureFlag(st.id, st.name)
          .subscribe(
            res => {
              this.msg.success('开关复位成功！');
              this.fetchArchiveSwitchs(this.currentEnvId);
            },
            err => {
              this.msg.error('开关复位失败，请稍后重试！');
            }
          );
      }
    });
  }

  // 转换本地时间
  getLocalDate(date: string) {
    if (!date) return '';
    return new Date(date);
  }

}
