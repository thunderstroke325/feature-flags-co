import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FfcAngularSdkService } from 'ffc-angular-sdk';
import { NzMessageService } from 'ng-zorro-antd/message';
import { forkJoin, Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import { SwitchService } from 'src/app/services/switch.service';
import { CSwitchParams, IFfParams, IPrequisiteFeatureFlag, IVariationOption } from '../types/switch-new';

@Component({
  selector: 'experimentation',
  templateUrl: './experimentation.component.html',
  styleUrls: ['./experimentation.component.less']
})
export class ExperimentationComponent implements OnInit, OnDestroy {

  private destory$: Subject<void> = new Subject();
  featureFlagId: string;

  experimentation: string;
  currentVariationOptions: IVariationOption[] = [];
  currentFeatureFlag: IFfParams;
  constructor(
    private route: ActivatedRoute,
    private switchServe: SwitchService,
    private message: NzMessageService,
    private ffcAngularSdkService: FfcAngularSdkService
  ) {
    this.experimentation = this.ffcAngularSdkService.variation('experimentation');

    this.route.data.pipe(map(res => res.switchInfo))
      .subscribe((result: CSwitchParams) => {
        const featureDetail = new CSwitchParams(result);
        this.currentVariationOptions = featureDetail.getVariationOptions();
        this.currentFeatureFlag = featureDetail.getSwicthDetail();
      })
  }

  ngOnInit(): void {
    this.featureFlagId = this.route.snapshot.paramMap.get('id');
    if(this.switchServe.envId) {
      this.initData();
    }
  }

  conversionTableTitle = '';
  isLoading: boolean = false;
  featureList: IPrequisiteFeatureFlag[] = [];                     // 开关列表
  targetFeatureFlag: IPrequisiteFeatureFlag;
  dateRange:Date[] = [];
  private initData() {
    this.isLoading = true;
    this.switchServe.getSwitchList(this.switchServe.envId).subscribe((result) => {
      if(result) {
        this.featureList = result;
        this.isLoading = false;
      }
    }, _ => {
      this.message.error("数据加载失败，请重试!");
      this.isLoading = false;
    })
  }

  onChange(result: Date[]): void {
    this.dateRange = [...result];
  }

  onSubmit() {
    // if (!this.targetFeatureFlag) {
    //   this.message.warning('请选择目标开关！');
    // }

    if (this.dateRange.length !== 2) {
      this.message.warning('请设置起止时间！');
      return false;
    }

    const baseLine = this.dataSet[0];

    let conversionRate = 0;
    this.resultData = [...this.dataSet.slice(0, Math.min(this.currentVariationOptions.length, this.dataSet.length))]
    .map((d, idx) => {
      const r = d.conversion / d.uniqueUsers * 100;
      if (r > conversionRate) {
        conversionRate = r;
        this.winnerId = idx;
      }

      return Object.assign({}, d, {
        id: idx,
        variation: this.currentVariationOptions[idx]?.variationValue,
        conversionRate: r.toFixed(2),
        confidenceInterval: d.confidenceInterval,
        change: d.isBaseline? 'Baseline' : (d.conversion / d.uniqueUsers * 100 - baseLine.conversion / baseLine.uniqueUsers * 100).toFixed(2)
      });
    });


    console.log(this.resultData);
  }

  winnerId: number = -1;
  resultData = [];

  dataSet = [
    {
      variation: 'VAR',
      conversion: 521,
      uniqueUsers: 999,
      conversionRate: 0,
      confidenceInterval: 94.5,
      change: 0,
      pValue: '-',
      isBaseline: true
    },
    {
      variation: 'VAR',
      conversion: 758,
      uniqueUsers: 1002,
      conversionRate: 0,
      confidenceInterval: 97.5,
      change: 0,
      pValue: 0.30,
      isBaseline: false
    },
    {
      variation: 'VAR',
      conversion: 614,
      uniqueUsers: 804,
      conversionRate: 0,
      confidenceInterval: 95.5,
      change: 0,
      pValue: 0.31,
      isBaseline: false
    },
    {
      variation: 'VAR',
      conversion: 100,
      uniqueUsers: 801,
      conversionRate: 0,
      confidenceInterval: 91.3,
      change: 0,
      pValue: 0.28,
      isBaseline: false
    },
    {
      variation: 'VAR',
      conversion: 721,
      uniqueUsers: 1002,
      conversionRate: 0,
      confidenceInterval: 97.5,
      change: 0,
      pValue: 0.19,
      isBaseline: false
    },
    {
      variation: 'VAR',
      conversion: 54,
      uniqueUsers: 987,
      conversionRate: 0,
      confidenceInterval: 97.5,
      change: 0,
      pValue: 0.12,
      isBaseline: false
    },
    {
      variation: 'VAR',
      conversion: 101,
      uniqueUsers: 963,
      conversionRate: 0,
      confidenceInterval: 90.5,
      change: 0,
      pValue: 0.15,
      isBaseline: false
    },
    {
      variation: 'VAR',
      conversion: 99,
      uniqueUsers: 846,
      conversionRate: 0,
      confidenceInterval: 89.5,
      change: 0,
      pValue: 0.10,
      isBaseline: false
    },
    {
      variation: 'VAR',
      conversion: 653,
      uniqueUsers: 895,
      conversionRate: 0,
      confidenceInterval: 98.5,
      change: 0,
      pValue: 0.11,
      isBaseline: false
    },
    {
      variation: 'VAR',
      conversion: 211,
      uniqueUsers: 703,
      conversionRate: 0,
      confidenceInterval: 86.7,
      change: 0,
      pValue: 0.25,
      isBaseline: false
    }
  ];

  ngOnDestroy(): void {
    this.destory$.next();
    this.destory$.complete();
  }
}
