import { Component, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzModalService } from 'ng-zorro-antd/modal';
import { Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import { SwitchService } from 'src/app/services/switch.service';
import { CSwitchParams, IFfParams, IVariationOption, IFfSettingParams } from '../types/switch-new';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { FfcAngularSdkService } from 'ffc-angular-sdk';

@Component({
  selector: 'setting',
  templateUrl: './switch-setting.component.html',
  styleUrls: ['./switch-setting.component.less']
})
export class SwitchSettingComponent implements OnDestroy {

  private destory$: Subject<void> = new Subject();
  public multistateEnabled: boolean = false;
  public currentSwitch: IFfParams = null;
  public variationOptions: IVariationOption[];
  public variationOptionEditId: number | null = null;
  private temporaryStateId: number = -1;
  public featureDetail: CSwitchParams;                      // 开关详情

  constructor(
    private route: ActivatedRoute,
    private switchServe: SwitchService,
    private msg: NzMessageService,
    private modal: NzModalService,
    private router: Router,
    private ffcAngularSdkService: FfcAngularSdkService
  ) {
    this.multistateEnabled = this.ffcAngularSdkService.variation('Multistate-enabled');

    this.route.data.pipe(map(res => res.switchInfo))
      .subscribe((result: CSwitchParams) => {
        this.featureDetail = new CSwitchParams(result);
        this.multistateEnabled = this.multistateEnabled && this.featureDetail.getIsMultiOptionMode();
        if (this.multistateEnabled) {
          this.variationOptions = this.featureDetail.getVariationOptions();
        }
        this.currentSwitch = this.featureDetail.getSwicthDetail();
      })
  }

  ngOnDestroy(): void {
    this.destory$.next();
    this.destory$.complete();
  }

  drop(event: CdkDragDrop<string[]>): void {
    moveItemInArray(this.variationOptions, event.previousIndex, event.currentIndex);
  }

  startEditVariationOption(id: number): void {
    this.variationOptionEditId = id;
  }

  stopEditVariationOption(): void {
    this.variationOptionEditId = null;
  }

  addVariationOptionRow(): void {
    this.variationOptions = [
      ...this.variationOptions,
      {
        localId: this.temporaryStateId,
        displayOrder: null,
        variationValue: null
      }
    ];

    this.temporaryStateId -= 1;
  }

  deleteVariationOptionRow(id: number): void {
    if(this.featureDetail.getTargetIndividuals().find(x => x.valueOption.localId === id).individuals.length > 0) {
      this.msg.warning("该状态已经在目标用户中被使用，移除后方可删除！");
      return;
    }

    if(this.featureDetail.getFftuwmtr().length > 0 && this.featureDetail.getFftuwmtr().find(x => x.valueOptionsVariationRuleValues.find(y => y.valueOption.localId === id))) {
      this.msg.warning("该状态已经在匹配条件的规则中被使用，移除后方可删除！");
      return;
    }

    if(this.featureDetail.getFFDefaultRulePercentageRollouts().length > 0 && this.featureDetail.getFFDefaultRulePercentageRollouts().find(x => x.valueOption.localId === id)) {
      this.msg.warning("该状态已经在默认返回值中被使用，移除后方可删除！");
      return;
    }

    if(this.featureDetail.getFFVariationOptionWhenDisabled() !== null && this.featureDetail.getFFVariationOptionWhenDisabled().localId === id) {
      this.msg.warning("该状态已经在开关关闭后的返回值中被使用，移除后方可删除！");
      return;
    }

    this.variationOptions = this.variationOptions.filter(d => d.localId !== id);
  }

  // 更新开关名字
  onSaveSwitch() {
    const { id, name } = this.currentSwitch;
    const data: IFfSettingParams = {id, name};

    if (this.multistateEnabled) {
      if (this.variationOptions.filter(v => v.variationValue === null || v.variationValue === '').length > 0) { // states with no values exist in the array
        this.msg.warning("请确保所有返回状态都设置了值！");
        return;
      }

      // reset multistate id and order
      let maxId = Math.max(...this.variationOptions.map(x => x.localId));
      this.variationOptions.forEach((e, i) => {
        if (e.localId < 0) {
          maxId += 1;
          e.localId = maxId;
        }

        e.displayOrder = i + 1;
      });

      data.variationOptions = this.variationOptions;
    }

    this.switchServe.updateSwitchSetting(data)
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
