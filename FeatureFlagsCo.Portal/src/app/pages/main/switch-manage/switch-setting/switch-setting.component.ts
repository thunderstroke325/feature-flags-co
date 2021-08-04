import { Component, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzModalService } from 'ng-zorro-antd/modal';
import { Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import { SwitchService } from 'src/app/services/switch.service';
import { CSwitchParams, IFfParams } from '../types/switch-new';

@Component({
  selector: 'setting',
  templateUrl: './switch-setting.component.html',
  styleUrls: ['./switch-setting.component.less']
})
export class SwitchSettingComponent implements OnDestroy {

  private destory$: Subject<void> = new Subject();
  public currentSwitch: IFfParams = null;

  constructor(
    private route: ActivatedRoute,
    private switchServe: SwitchService,
    private msg: NzMessageService,
    private modal: NzModalService,
    private router: Router
  ) {
    this.route.data.pipe(map(res => res.switchInfo))
      .subscribe((result: CSwitchParams) => {
        this.currentSwitch = (new CSwitchParams(result)).getSwicthDetail();
      })
  }

  ngOnDestroy(): void {
    this.destory$.next();
    this.destory$.complete();
  }

  // 更新开关名字
  onCreateSwitch() {
    this.switchServe.updateSwitchName(this.currentSwitch.id, this.currentSwitch.name)
      .subscribe((result: IFfParams) => {
        this.currentSwitch = result;
        this.switchServe.setCurrentSwitch(result);
        this.msg.success("开关信息更新成功!");
      }, _ => {
        this.msg.error("开关信息修改失败，请查看是否有相同名字的开关!");
      })
  }

  // 存档
  onArchiveClick() {
    this.modal.create({
      nzContent: '确定存档（软删除）开关吗？存档后开关将从开关列表中移出，并且调用SDK的返回值将为关闭开关后设定的默认值。',
      nzOkText: '确认存档',
      nzOnOk: () => {
        this.switchServe.archiveEnvFeatureFlag(this.currentSwitch.id, this.currentSwitch.name)
          .subscribe(
            res => {
              this.msg.success('开关存档成功！');
              this.router.navigateByUrl('/switch-archive');
            },
            err => {
              this.msg.error('开关存档失败，请稍后重试！');
            }
          );
      }
    });

  }
}
