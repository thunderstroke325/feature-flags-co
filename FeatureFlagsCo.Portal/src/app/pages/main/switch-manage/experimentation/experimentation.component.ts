import { Component, OnDestroy, OnInit } from '@angular/core';
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
import { CustomEventTrackOption, EventType, ExperimentStatus, IExperiment } from '../types/experimentations';
import * as moment from 'moment';

@Component({
  selector: 'experimentation',
  templateUrl: './experimentation.component.html',
  styleUrls: ['./experimentation.component.less']
})
export class ExperimentationComponent implements OnInit, OnDestroy {

  featureFlagId: string;
  currentVariationOptions: IVariationOption[] = [];
  selectedBaseline: IVariationOption = null;

  experimentation: string;

  hasInvalidVariation: boolean = false;
  hasWinnerVariation: boolean = false;

  onGoingExperiments: IExperiment[] = [];
  refreshIntervalId;
  refreshInterval: number = 1000 * 60; // 1 minute
  constructor(
    private route: ActivatedRoute,
    private switchServe: SwitchService,
    private message: NzMessageService,
    private experimentService: ExperimentService,
    private ffcAngularSdkService: FfcAngularSdkService
  ) {
    this.experimentation = environment.name === 'Standalone' ? 'temporary version' : this.ffcAngularSdkService.variation('experimentation');

    this.route.data.pipe(map(res => res.switchInfo)).subscribe((result: CSwitchParams) => {
      const featureDetail = new CSwitchParams(result);
      this.currentVariationOptions = featureDetail.getVariationOptions();
    })
  }

  ngOnDestroy(): void {
    clearInterval(this.refreshIntervalId);
  }

  ngOnInit(): void {
    this.featureFlagId = this.route.snapshot.paramMap.get('id');
    if(this.switchServe.envId) {
      this.initData();

      this.refreshIntervalId = setInterval(() => {
        const activeExperimentIteration = this.onGoingExperiments.map(expt => {
          return {
            experimentId: expt.id,
            iterationId: expt.iterations.find(it => it.endTime === null)?.id
          }
        }).filter(expt => !!expt.iterationId);

        this.experimentService.getIterationResults(this.switchServe.envId, activeExperimentIteration).subscribe(res => {
          if (res && res.length > 0) {
            this.onGoingExperiments.forEach(expt => {
              const iteration = res.find(r => r.id === expt.selectedIteration.id);
              if (iteration) {
                expt.selectedIteration.results = this.processIteration(iteration, expt.baselineVariation).results;
                expt.selectedIteration.updatedAt = iteration.updatedAt;
                expt.selectedIteration.updatedAtStr = moment(iteration.updatedAt).format('YYYY-MM-DD HH:mm');
              }
            });
          }
        });
      }, this.refreshInterval);
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

  experimentList: IExperiment[] = [];
  private initData() {
    this.experimentService.getExperiments({envId: this.switchServe.envId, featureFlagId: this.featureFlagId}).subscribe(experiments => {
      if (experiments) {
        this.experimentList = experiments.map(experiment => {
          const expt = Object.assign({}, experiment);

          if (expt.iterations.length > 0) {
            expt.iterations = expt.iterations.map(ex => this.processIteration(ex, expt.baselineVariation));
            expt.selectedIteration = expt.iterations[expt.iterations.length -1];
            if (expt.selectedIteration.updatedAt) {
              expt.selectedIteration.updatedAtStr = moment(expt.selectedIteration.updatedAt).format('YYYY-MM-DD HH:mm');
            }
          }

          expt.isLoading = false;
          return expt;
        });

        this.onGoingExperiments = [...this.experimentList.filter(expt => expt.status === ExperimentStatus.Recording)];
      }
    });

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

  onStartIterationClick(expt: IExperiment) {
    this.experimentService.startIteration(this.switchServe.envId, expt.id).subscribe(res => {
      if (res) {
        expt.iterations.push(this.processIteration(res, expt.baselineVariation));
        expt.selectedIteration = expt.iterations[expt.iterations.length -1];
        expt.status = ExperimentStatus.Recording;

        this.onGoingExperiments = [...this.onGoingExperiments, expt];
      }
    });
  }

  onStopIterationClick(expt: IExperiment) {
    this.experimentService.stopIteration(this.switchServe.envId, expt.id, expt.selectedIteration.id).subscribe(res => {
      if (res) {
        expt.selectedIteration.endTime = res.endTime;
        expt.selectedIteration.dateTimeInterval = `${moment(expt.selectedIteration.startTime).format('YYYY-MM-DD HH:mm')} - ${moment(expt.selectedIteration.endTime).format('YYYY-MM-DD HH:mm')}`
        expt.status = ExperimentStatus.NotRecording;

        const idx = this.onGoingExperiments.findIndex(ex => ex.id === expt.id);
        if (idx > -1) {
          this.onGoingExperiments.splice(idx, 1);
        }
      }
    });
  }

  onReloadIterationResultsClick(expt: IExperiment) {
    expt.isLoading  = true;
    this.experimentService.getIterationResults(this.switchServe.envId, [{ experimentId: expt.id, iterationId: expt.selectedIteration.id}]).subscribe(res => {
      if (res) {
        expt.selectedIteration.results = this.processIteration(res[0], expt.baselineVariation).results;
        expt.selectedIteration.updatedAt = res[0].updatedAt;
        expt.selectedIteration.updatedAtStr = moment(res[0].updatedAt).format('YYYY-MM-DD HH:mm');
      }

      expt.isLoading  = false;
    }, _ => {
      //this.message.error("数据加载失败，请重试!");
      expt.isLoading  = false;
    });
  }

  private processIteration(iteration: any, baselineVariation: string) {
    const iterationResults = this.currentVariationOptions.map((option) => {
        const found = iteration.results.find(r => r.variation == option.localId);

        return !found ? this.createEmptyIterationResult(option, baselineVariation) : Object.assign({}, found, {
          variationValue: option.variationValue,
          pValue: found.pValue === -1 ? '--' : found.pValue,
          isEmpty: false,
        })
      });

    const hasInvalidVariation = iterationResults.findIndex(e => e.isInvalid && !e.isBaseline) > -1;
    const hasWinnerVariation = iterationResults.findIndex(e => e.isWinner) > -1;

    const iterationEndTime = iteration.endTime || new Date();
    return Object.assign({}, iteration, {
      hasInvalidVariation,
      hasWinnerVariation,
      results: iterationResults,
      dateTimeInterval: `${moment(iteration.startTime).format('YYYY-MM-DD HH:mm')} - ${iteration.endTime? moment(iteration.endTime).format('YYYY-MM-DD HH:mm') : moment(new Date()).format('YYYY-MM-DD HH:mm') + ' (现在)'}`
    });
  }

  private createEmptyIterationResult(option: IVariationOption, baselineVariation: string) {
    return {
      isEmpty: true,
      variationValue: option.variationValue,
      isBaseline: baselineVariation === `${option.localId}`
     };
  }

  customEventType: EventType = EventType.Custom;
  customEventTrackConversion: CustomEventTrackOption = CustomEventTrackOption.Conversion;
  /************************** above are for new experiment ****************************************/
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
}
