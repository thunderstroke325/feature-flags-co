import { Component, OnDestroy, OnInit } from '@angular/core';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzModalService } from 'ng-zorro-antd/modal';
import { Subject } from 'rxjs';
import { SwitchService } from 'src/app/services/switch.service';
import { ISwitchArchive } from './types/switch-archive';
import { AccountService } from 'src/app/services/account.service';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-switch-archive',
  templateUrl: './switch-archive.component.html',
  styleUrls: ['./switch-archive.component.less']
})
export class SwitchArchiveComponent implements OnInit, OnDestroy {

  destory$: Subject<void> = new Subject();
  private search$ = new Subject<any>();
  currentEnvId: number;
  currentAccountId: number;
  searchText: string = '';
  isLoading: boolean = false;
  switchLoading: boolean = false;
  switchLists: ISwitchArchive[] = [];

  constructor(
    private switchService: SwitchService,
    private accountService: AccountService,
    private modal: NzModalService,
    private message: NzMessageService
  ) { }

  ngOnInit(): void {
    const currentAccountProjectEnv = this.accountService.getCurrentAccountProjectEnv();
    this.currentAccountId = currentAccountProjectEnv.account.id;
    this.currentEnvId = currentAccountProjectEnv.projectEnv.envId;
    this.init();
    this.search$.next('');
  }

  onSearch() {
    this.search$.next(this.searchText);
  }

  private init() {
    this.search$.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(e => {
      this.isLoading = true;
      this.switchService.getArchiveSwitch(this.currentEnvId, {searchText: e}).subscribe(res => {
        this.isLoading = false;
        this.switchLists = res;
      }, _ => {
        this.message.error("数据加载失败，请重试!");
        this.isLoading = false;
      })
    });
  }

  ngOnDestroy(): void {
    this.destory$.next();
    this.destory$.complete();
  }

  // 复位开关
  onUnarchiveClick(st: ISwitchArchive) {
    this.switchLoading = true;
    this.modal.create({
      nzContent: '<div>确定复位开关吗？复位后开关状态为关闭， 以避免给线上环境造成影响。</div>',
      nzOkText: '确认复位',
      nzTitle: '切换项目环境',
      nzCentered: true,
      nzWidth: 700,
      nzBodyStyle: {minHeight: '100px'},
      nzOnOk: () => {
        this.switchService.unarchiveEnvFeatureFlag(st.id, st.name)
          .subscribe(
            res => {
              this.switchLists = this.switchLists.filter(s => s.id !== st.id);
              this.message.success('开关复位成功！');
              this.search$.next(this.searchText);
            },
            err => {
              this.message.error('开关复位失败，请稍后重试！');
            }
          );
      }
    });
    this.switchLoading = false;
  }

  // 转换本地时间
  getLocalDate(date: string) {
    if (!date) return '';
    return new Date(date);
  }

}
