import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FfcAngularSdkService } from 'ffc-angular-sdk';
import { NzMessageService } from 'ng-zorro-antd/message';
import { ExperimentService } from 'src/app/services/experiment.service';
import { SwitchService } from 'src/app/services/switch.service';
import { CSwitchParams, IVariationOption } from '../types/switch-new';
import { differenceInCalendarDays } from 'date-fns';
import { debounceTime, distinctUntilChanged, map } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { environment } from './../../../../../environments/environment';

@Component({
  selector: 'experimentation',
  templateUrl: './experimentation.component.html',
  styleUrls: ['./experimentation.component.less']
})
export class ExperimentationComponent implements OnInit {

  featureFlagId: string;
  currentVariationOptions: IVariationOption[] = [];
  selectedBaseline: IVariationOption = null;

  experimentation: string;

  hasInvalidVariation: boolean = false;
  hasWinnerVariation: boolean = false;
  constructor(
    private route: ActivatedRoute,
    private switchServe: SwitchService,
    private message: NzMessageService,
    private experimentService: ExperimentService,
    private ffcAngularSdkService: FfcAngularSdkService
  ) {
    this.experimentation = environment.name === 'Standalone' ? 'temporary version' : this.ffcAngularSdkService.variation('experimentation');

    this.route.data.pipe(map(res => res.switchInfo))
    .subscribe((result: CSwitchParams) => {
      const featureDetail = new CSwitchParams(result);
      this.currentVariationOptions = featureDetail.getVariationOptions();
    })
  }

  ngOnInit(): void {
    this.featureFlagId = this.route.snapshot.paramMap.get('id');
    if(this.switchServe.envId) {
      this.initData();
    }
  }

   // 搜索 events
   private eventInputs = new Subject<any>();
   public isEventsLoading = false;
   public onSearchEvents(value: string = '') {
    this.eventInputs.next(value);
  }

  disabledDate = (current: Date): boolean =>
    // Can not select days before today and today
    differenceInCalendarDays(current, new Date()) > 0;

  conversionTableTitle = '';
  eventList: string[] = [];
  selectedEvent: string;
  dateRange:Date[] = [];
  lastSearchText = null;

  private initData() {
    this.eventInputs.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(e => {
      this.isEventsLoading = true;
      this.experimentService.getCustomEvents(this.switchServe.envId, null, e).subscribe((result: any) => {
        if(result) {
          if (result.aggregations.keys.buckets.length === 0) {
            this.message.warning('目前还没有任何事件，请耐心等待事件被终端用户激发后再试！');
          } else {
            //this.lastEventItem = result.aggregations.keys.after_key.EventName;
            this.eventList = result.aggregations.keys.buckets.map(b => b.key.EventName);
          }
        }
        this.isEventsLoading = false;
      }, _ => {
        this.message.error("数据加载失败，请重试!");
        this.isEventsLoading = false;
      })
    });
  }

  onDateRangeChange(result: Date[]): void {
    this.dateRange = [...result];
  }

  experimentResult = [];
  isLoadingResult = false;
  onSubmit() {

    if (this.selectedBaseline === undefined || this.selectedBaseline === null) {
      this.message.warning('请选择基准特性！');
      return false;
    }

    if (this.selectedEvent === undefined || this.selectedEvent === null || this.selectedEvent === '') {
      this.message.warning('请选择事件！');
      return false;
    }

    if (this.dateRange.length !== 2) {
      this.message.warning('请设置起止时间！');
      return false;
    }

    this.dateRange[0].setSeconds(0);
    this.dateRange[1].setSeconds(0);

    const param = {
      eventName: this.selectedEvent,
      startExptTime: this.dateRange[0].toISOString().slice(0, 19),
      endExptTime: this.dateRange[1].toISOString().slice(0, 19),
      flag: {
        id: this.featureFlagId,
        baselineVariation: `${this.selectedBaseline.localId}`,
        variations: this.currentVariationOptions.map(o => `${o.localId}`)
      }
    };

    this.isLoadingResult = true;
    this.experimentResult = [];
    this.experimentService.getExperimentResult(this.switchServe.envId, param).subscribe((result) => {
      if(result && result.length > 0) {
        this.experimentResult = this.currentVariationOptions.map((option) => {
          const found = result.find(r => r.variation == option.localId);

          return !found ? this.createEmptyExperimentResult(option) : Object.assign({}, found, {
            variationValue: option.variationValue,
            pValue: found.pValue === -1 ? '--' : found.pValue,
            isEmpty: false
          })
        });

        this.hasInvalidVariation = this.experimentResult.findIndex(e => e.isInvalid && !e.isBaseline) > -1;
        this.hasWinnerVariation = this.experimentResult.findIndex(e => e.isWinner) > -1;
      } else {
        this.message.warning("暂时还没有实验数据，请修改时间区间或稍后再试!");
      }

      this.isLoadingResult = false;
    }, _ => {
      this.message.error("数据加载失败，请重试!");
      this.isLoadingResult = false;
    });
  }

  private createEmptyExperimentResult(option: IVariationOption) {
    return {
      isEmpty: true,
      variationValue: option.variationValue,
      isBaseline: this.selectedBaseline.localId === option.localId
     };
  }

  dataSet = [
    {
        "changeToBaseline": 0.19,
        "confidenceInterval": [
            0.61,
            0.90
        ],
        "conversion": 502,
        "conversionRate": 0.86,
        "isInvalid": false,
        "isWinner": true,
        "pValue": 0.46,
        "uniqueUsers": 601,
        "variation": "1"
    },
    {
        "changeToBaseline": 0,
        "confidenceInterval": [
            0,
            1
        ],
        "conversion": 57,
        "conversionRate": 0.67,
        "isInvalid": false,
        "isWinner": false,
        "pValue": 0,
        "uniqueUsers": 302,
        "variation": "2"
    },
    {
        "changeToBaseline": 0,
        "confidenceInterval": [
            0.60,
            0.82
        ],
        "conversion": 100,
        "conversionRate": 0.67,
        "isInvalid": false,
        "isWinner": false,
        "pValue": 0,
        "uniqueUsers": 500,
        "variation": "Blue"
    },
    {
        "changeToBaseline": 0,
        "confidenceInterval": [
            0.60,
            0.82
        ],
        "conversion": 100,
        "conversionRate": 0.67,
        "isInvalid": false,
        "isWinner": false,
        "pValue": 0,
        "uniqueUsers": 500,
        "variation": "Yellow"
    },
    {
        "changeToBaseline": 0,
        "confidenceInterval": [
            0.60,
            0.82
        ],
        "conversion": 100,
        "conversionRate": 0.67,
        "isInvalid": false,
        "isWinner": false,
        "pValue": 0,
        "uniqueUsers": 500,
        "variation": "Purple"
    }
  ]
}
