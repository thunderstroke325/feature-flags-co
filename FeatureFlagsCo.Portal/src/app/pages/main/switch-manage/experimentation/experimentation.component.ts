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
  currentFeatureFlag: CSwitchParams = null;
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

  exptRulesVisible = false;

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
      this.currentFeatureFlag = new CSwitchParams(res);
      this.currentVariationOptions = this.currentFeatureFlag.getVariationOptions();
    });
  }

  ngOnDestroy(): void {
    clearInterval(this.refreshIntervalId);
  }

  onSetExptRulesClick() {
    this.exptRulesVisible = true;
  }

  onSetExptRulesClosed(data: CSwitchParams) {
    this.exptRulesVisible = false;
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
      toolTip: { tplFormatter: tpl => tpl.replace("{value}", `{value} ${valueUnit}`) },
    });
  }
}
