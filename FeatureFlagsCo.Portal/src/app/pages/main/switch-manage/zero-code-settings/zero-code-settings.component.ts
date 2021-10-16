import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FfcService } from 'src/app/services/ffc.service';
import { NzMessageService } from 'ng-zorro-antd/message';
import { SwitchService } from 'src/app/services/switch.service';
import { CSwitchParams, IVariationOption } from '../types/switch-new';
import { environment } from '../../../../../environments/environment';
import { ZeroCodeService } from 'src/app/services/zero-code.service';
import { IZeroCode } from '../types/zero-code';
import { uuidv4 } from 'src/app/utils';
import { IProjectEnv } from 'src/app/config/types';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';

@Component({
  selector: 'zero-code-settings',
  templateUrl: './zero-code-settings.component.html',
  styleUrls: ['./zero-code-settings.component.less']
})
export class ZeroCodeSettingsComponent implements OnInit, OnDestroy {

  featureFlagId: string;
  currentVariationOptions: IVariationOption[] = [];
  isLoading = true;

  model: IZeroCode;
  constructor(
    private route: ActivatedRoute,
    private switchServe: SwitchService,
    private zeroCodeService: ZeroCodeService,
    private message: NzMessageService,
    private ffcService: FfcService
  ) {
    this.featureFlagId = decodeURIComponent(this.route.snapshot.params['id']);
    const currentProjectEnv: IProjectEnv = JSON.parse(localStorage.getItem('current-project'));
    this.model = {
      envId: currentProjectEnv.envId,
      envSecret: currentProjectEnv.envSecret,
      isActive: true,
      featureFlagId: this.featureFlagId,
      featureFlagKey: this.featureFlagId.split('__')[4],
      items: []
    }

    this.switchServe.getSwitchDetail(this.featureFlagId).subscribe(res => {
      const featureDetail = new CSwitchParams(res);
      this.currentVariationOptions = featureDetail.getVariationOptions();
    });
  }

  ngOnDestroy(): void {

  }

  ngOnInit(): void {
    if(this.switchServe.envId) {
      this.initData();
    }
  }

  drop(event: CdkDragDrop<string[]>): void {
    moveItemInArray(this.model.items, event.previousIndex, event.currentIndex);
  }

  private initData() {
    this.zeroCodeService.getZeroCodes(this.switchServe.envId, this.featureFlagId).subscribe((res: IZeroCode) => {
      if (res) {
        this.model = res;
      }

      this.isLoading = false;
    }, err => {
      this.message.error('数据加载失败');
      this.isLoading = false;
    })
  }

  onAddCssSelectorRow() {
    this.model.items = [
      ...this.model.items,
      {
        id: uuidv4(),
        cssSelector: null,
        description: null,
        variationOptionId: null,
        url: null
      }
    ];
  }

  isSaving: boolean = false;
  doSubmit() {
    this.isSaving = true;

    this.zeroCodeService.upsert(this.model)
      .subscribe(
        res => {
          this.isSaving = false;
          this.message.success('更新成功！');
        },
        err => {
          this.message.error('发生错误，请重试！');
          this.isSaving = false;
        }
      );
  }
}
