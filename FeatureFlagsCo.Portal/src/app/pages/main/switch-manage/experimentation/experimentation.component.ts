import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FfcService } from 'src/app/services/ffc.service';
import { NzMessageService } from 'ng-zorro-antd/message';
import { ExperimentService } from 'src/app/services/experiment.service';
import { SwitchService } from 'src/app/services/switch.service';
import { CSwitchParams, IVariationOption } from '../types/switch-new';
import { environment } from '../../../../../environments/environment';
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
  isInitLoading = true;
  experimentation: string;

  hasInvalidVariation: boolean = false;
  hasWinnerVariation: boolean = false;

  onGoingExperiments: IExperiment[] = [];
  refreshIntervalId;
  refreshInterval: number = 1000 * 30; // 1 minute

  customEventTrackConversion: CustomEventTrackOption = CustomEventTrackOption.Conversion;
  customEventTrackNumeric: CustomEventTrackOption = CustomEventTrackOption.Numeric;
  customEventType: EventType = EventType.Custom;
  pageViewEventType: EventType = EventType.PageView;
  clickEventType: EventType = EventType.Click;

  constructor(
    private route: ActivatedRoute,
    private switchServe: SwitchService,
    private message: NzMessageService,
    private experimentService: ExperimentService,
    private ffcService: FfcService
  ) {
    this.experimentation = environment.name === 'Standalone' ? 'temporary version' : this.ffcService.client.variation('experimentation');
    const ffId: string = decodeURIComponent(this.route.snapshot.params['id']);
    this.switchServe.getSwitchDetail(ffId).subscribe(res => {
      const featureDetail = new CSwitchParams(res);
      this.currentVariationOptions = featureDetail.getVariationOptions();
    });
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
          expt.isLoading = true;
          return {
            experimentId: expt.id,
            iterationId: expt.iterations.find(it => it.endTime === null)?.id
          }
        });

        if (activeExperimentIteration.length > 0) {
          this.experimentService.getIterationResults(this.switchServe.envId, activeExperimentIteration).subscribe(res => {
            if (res && res.length > 0) {
              this.onGoingExperiments.forEach(expt => {
                const iteration = res.find(r => r.id === expt.selectedIteration.id);
                if (iteration) {
                  expt.selectedIteration.results = this.processIteration(iteration, expt.baselineVariation).results;
                  if (iteration.updatedAt) {
                    expt.selectedIteration.updatedAt = iteration.updatedAt;
                    expt.selectedIteration.updatedAtStr = moment(iteration.updatedAt).format('YYYY-MM-DD HH:mm');
                  }

                  if (expt.metric.customEventTrackOption === this.customEventTrackNumeric) {
                    // [min, max, max - min]
                    expt.selectedIteration.numericConfidenceIntervalBoundary = [
                      Math.min(...expt.selectedIteration.results.map(r => r.confidenceInterval[0])),
                      Math.max(...expt.selectedIteration.results.map(r => r.confidenceInterval[1])),
                    ];

                    expt.selectedIteration.numericConfidenceIntervalBoundary.push(expt.selectedIteration.numericConfidenceIntervalBoundary[1] - expt.selectedIteration.numericConfidenceIntervalBoundary[0]);
                  }

                  // update experiment original iterations
                  const selectedIterationIndex = expt.iterations.findIndex(iteration => iteration.id === expt.selectedIteration.id);
                  expt.iterations[selectedIterationIndex] = expt.selectedIteration;
                }
              });
            }

            this.onGoingExperiments.forEach(expt => {
              expt.isLoading = false;
              this.initChartConfig(expt);
            });
          }, _ => {
            this.onGoingExperiments.forEach(expt => expt.isLoading = false);
          });
        }
      }, this.refreshInterval);
    }
  }

  experimentList: IExperiment[] = [];
  private initData() {
    this.experimentService.getExperiments({envId: this.switchServe.envId, featureFlagId: this.featureFlagId}).subscribe(experiments => {
      if (experiments) {
        this.experimentList = experiments.map(experiment => {
          const expt = Object.assign({}, experiment);

          if (expt.iterations.length > 0) {
            expt.iterations = expt.iterations.map(ex => this.processIteration(ex, expt.baselineVariation)).reverse();
            expt.selectedIteration = expt.iterations[0];

            if (expt.selectedIteration.updatedAt) {
              expt.selectedIteration.updatedAtStr = moment(expt.selectedIteration.updatedAt).format('YYYY-MM-DD HH:mm');
            }

            if (experiment.metric.customEventTrackOption === this.customEventTrackNumeric) {
              // [min, max, max - min]
              expt.selectedIteration.numericConfidenceIntervalBoundary = [
                Math.min(...expt.selectedIteration.results.map(r => r.confidenceInterval[0])),
                Math.max(...expt.selectedIteration.results.map(r => r.confidenceInterval[1])),
              ];

              expt.selectedIteration.numericConfidenceIntervalBoundary.push(expt.selectedIteration.numericConfidenceIntervalBoundary[1] - expt.selectedIteration.numericConfidenceIntervalBoundary[0]);
            }
          }

          expt.isLoading = false;
          return expt;
        });

        this.onGoingExperiments = [...this.experimentList.filter(expt => expt.status === ExperimentStatus.Recording)];
        this.experimentList.forEach(experiment => this.initChartConfig(experiment));
      }
      this.isInitLoading = false;
    }, _ => {
      this.message.error("数据加载失败，请重试!");
      this.isInitLoading = false;
    });

    // this.eventInputs.pipe(
    //   debounceTime(300),
    //   distinctUntilChanged()
    // ).subscribe(e => {
    //   this.isEventsLoading = true;
    //   this.experimentService.getCustomEvents(this.switchServe.envId, null, e).subscribe((result: any) => {
    //     if(result) {
    //       if (result.aggregations.keys.buckets.length === 0) {
    //         this.message.warning('目前还没有任何事件，请耐心等待事件被终端用户激发后再试！');
    //       } else {
    //         this.eventList = result.aggregations.keys.buckets.map(b => b.key.EventName);
    //       }
    //     }
    //     this.isEventsLoading = false;
    //   }, _ => {
    //     this.message.error("数据加载失败，请重试!");
    //     this.isEventsLoading = false;
    //   })
    // });
  }

  onStartIterationClick(expt: IExperiment) {
    expt.isLoading  = true;
    this.experimentService.startIteration(this.switchServe.envId, expt.id).subscribe(res => {
      if (res) {
        expt.iterations.unshift(this.processIteration(res, expt.baselineVariation));
        expt.selectedIteration = expt.iterations[0];
        expt.status = ExperimentStatus.Recording;

        this.onGoingExperiments = [...this.onGoingExperiments, expt];
      }
      expt.isLoading  = false;
    }, _ => {
      this.message.error("操作失败，请重试!");
      expt.isLoading  = false;
    });
  }

  onStopIterationClick(expt: IExperiment) {
    expt.isLoading  = true;
    this.experimentService.stopIteration(this.switchServe.envId, expt.id, expt.selectedIteration.id).subscribe(res => {
      if (res) {
        expt.selectedIteration.endTime = res.endTime;
        expt.selectedIteration.dateTimeInterval = `${moment(expt.selectedIteration.startTime).format('YYYY-MM-DD HH:mm')} - ${moment(expt.selectedIteration.endTime).format('YYYY-MM-DD HH:mm')}`
        expt.status = ExperimentStatus.NotRecording;

        // const idx = this.onGoingExperiments.findIndex(ex => ex.id === expt.id);
        // if (idx > -1) {
        //   this.onGoingExperiments.splice(idx, 1);
        // }
      }

      expt.isLoading  = false;
    }, _ => {
      this.message.error("操作失败，请重试!");
      expt.isLoading  = false;
    });
  }

  onReloadIterationResultsClick(expt: IExperiment) {
    expt.isLoading  = true;
    this.experimentService.getIterationResults(this.switchServe.envId, [{ experimentId: expt.id, iterationId: expt.selectedIteration.id}]).subscribe(res => {
      if (res) {
        expt.selectedIteration.results = this.processIteration(res[0], expt.baselineVariation).results;
        if (res[0].updatedAt) {
          expt.selectedIteration.updatedAt = res[0].updatedAt;
          expt.selectedIteration.updatedAtStr = moment(res[0].updatedAt).format('YYYY-MM-DD HH:mm');
        }
      }

      expt.isLoading  = false;
    }, _ => {
      //this.message.error("数据加载失败，请重试!");
      expt.isLoading  = false;
    });
  }

  onDeleteExptClick(expt: IExperiment) {
    expt.isLoading  = true;
    this.experimentService.archiveExperiment(this.switchServe.envId, expt.id).subscribe(res => {
      this.experimentList = this.experimentList.filter(ex => ex.id !== expt.id);
      expt.isLoading  = false;
    }, _ => {
      this.message.error("操作失败，请重试!");
      expt.isLoading  = false;
    });
  }

  onDeleteExptDataClick(expt: IExperiment) {
    expt.isLoading  = true;
    this.experimentService.archiveExperimentData(this.switchServe.envId, expt.id).subscribe(res => {
      expt.selectedIteration = null;
      expt.iterations = [];
      expt.status = ExperimentStatus.NotStarted;
      expt.isLoading  = false;
    }, _ => {
      this.message.error("操作失败，请重试!");
      expt.isLoading  = false;
    });
  }

  private processIteration(iteration: any, baselineVariation: string) {
    const iterationResults = this.currentVariationOptions.map((option) => {
        const found = iteration.results.find(r => r.variation == option.localId);

        return !found ? this.createEmptyIterationResult(option, baselineVariation) : Object.assign({}, found, {
          conversion: found.conversion === -1 ? '--' : found.conversion,
          conversionRate: found.conversionRate === -1 ? '--' : found.conversionRate,
          uniqueUsers: found.uniqueUsers === -1 ? '--' : found.uniqueUsers,
          totalEvents: found.totalEvents === -1 ? '--' : found.totalEvents,
          average: found.average === -1 ? '--' : found.average,
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
      confidenceInterval: [-1, -1],
      isBaseline: baselineVariation === `${option.localId}`
     };
  }

  private initChartConfig(experiment: IExperiment) {
    const iterations = experiment.iterations;
    if (!iterations || !iterations.length) {
      return;
    }

    const xAxisName = '时间';
    const trackOption = iterations[0].customEventTrackOption;
    const valueUnit = trackOption === CustomEventTrackOption.Conversion
      ? '%' : (iterations[0].customEventUnit ? iterations[0].customEventUnit : '');
    const yAxisName = trackOption === CustomEventTrackOption.Conversion
      ? `转换率（${valueUnit}）`
      : iterations[0].customEventUnit ? `平均值（${valueUnit}）` : '平均值';

    let source = [];
    let yAxisFormatter;
    iterations.forEach(iteration => {
      const xAxisValue = iteration.endTime ? iteration.endTime : iteration.updatedAt;
      iteration.results.forEach(result => {
        if (!result.variation ||
          !this.currentVariationOptions.find(option => option.localId.toString() == result.variation)
        ) {
          return;
        }

        // see function **processIteration**
        // conversionRate average 这两个值为 -1 时 会被修改为 '--' 代表 '没有值' 显示为 0
        let yAxisValue;
        if (trackOption === CustomEventTrackOption.Conversion) {
          const conversionRate = Number(result.conversionRate);
          yAxisValue = conversionRate ? conversionRate * 100 : 0;
          yAxisFormatter = val => `${val} %`;
        } else {
          const average = Number(result.average);
          yAxisValue = average ? average : 0;
        }

        source.push({variation: result.variationValue, time: xAxisValue, value: yAxisValue});
      });
    });

    experiment.chartConfig = ({
      containerId: experiment.id,
      xAxis: {
        name: xAxisName,
        position: 'end',
        field: 'time',
        scale: {type: "timeCat", nice: true, range: [0.05, 0.95], mask: 'YYYY-MM-DD HH:mm'}
      },
      yAxis: {
        name: yAxisName,
        position: 'end',
        field: 'value',
        formatter: yAxisFormatter,
        scale: {nice: true}
      },
      source: source,
      dataGroupBy: 'variation',
      padding: [50, 50, 50, 70],
      height: 400,
      toolTip: { valueTplFormatter: tpl => tpl.replace("{value}", `{value} ${valueUnit}`) }
    });
  }

  /************************** above are for new experiment ****************************************/

  // 搜索 events
  // private eventInputs = new Subject<any>();
  // public isEventsLoading = false;
  // public onSearchEvents(value: string = '') {
  //   this.eventInputs.next(value);
  // }

  // disabledDate = (current: Date): boolean =>
  //   // Can not select days before today and today
  //   differenceInCalendarDays(current, new Date()) > 0;

  // eventList: string[] = [];
  // selectedEvent: string;
  // dateRange:Date[] = [];
  // lastSearchText = null;

  // onDateRangeChange(result: Date[]): void {
  //   this.dateRange = [...result];
  // }

  //experimentResult = [];
  //isLoadingResult = false;
  // onSubmit() {

  //   if (this.selectedBaseline === undefined || this.selectedBaseline === null) {
  //     this.message.warning('请选择基准特性！');
  //     return false;
  //   }

  //   if (this.selectedEvent === undefined || this.selectedEvent === null || this.selectedEvent === '') {
  //     this.message.warning('请选择事件！');
  //     return false;
  //   }

  //   if (this.dateRange.length !== 2) {
  //     this.message.warning('请设置起止时间！');
  //     return false;
  //   }

  //   this.dateRange[0].setSeconds(0);
  //   this.dateRange[1].setSeconds(0);

  //   const param = {
  //     eventName: this.selectedEvent,
  //     startExptTime: this.dateRange[0].toISOString().slice(0, 19),
  //     endExptTime: this.dateRange[1].toISOString().slice(0, 19),
  //     flag: {
  //       id: this.featureFlagId,
  //       baselineVariation: `${this.selectedBaseline.localId}`,
  //       variations: this.currentVariationOptions.map(o => `${o.localId}`)
  //     }
  //   };

  //   this.isLoadingResult = true;
  //   this.experimentResult = [];
  //   this.experimentService.getExperimentResult(this.switchServe.envId, param).subscribe((result) => {
  //     if(result && result.length > 0) {
  //       this.experimentResult = this.currentVariationOptions.map((option) => {
  //         const found = result.find(r => r.variation == option.localId);

  //         return !found ? this.createEmptyExperimentResult(option) : Object.assign({}, found, {
  //           variationValue: option.variationValue,
  //           pValue: found.pValue === -1 ? '--' : found.pValue,
  //           isEmpty: false
  //         })
  //       });

  //       this.hasInvalidVariation = this.experimentResult.findIndex(e => e.isInvalid && !e.isBaseline) > -1;
  //       this.hasWinnerVariation = this.experimentResult.findIndex(e => e.isWinner) > -1;
  //     } else {
  //       this.message.warning("暂时还没有实验数据，请修改时间区间或稍后再试!");
  //     }

  //     this.isLoadingResult = false;
  //   }, _ => {
  //     this.message.error("数据加载失败，请重试!");
  //     this.isLoadingResult = false;
  //   });
  // }

  // private createEmptyExperimentResult(option: IVariationOption) {
  //   return {
  //     isEmpty: true,
  //     variationValue: option.variationValue,
  //     isBaseline: this.selectedBaseline.localId === option.localId
  //    };
  // }
}
