import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NzMessageService } from 'ng-zorro-antd/message';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { IProjectEnv } from 'src/app/config/types';
import { ExperimentService } from 'src/app/services/experiment.service';
import { ExperimentStatus, IExperiment } from '../../switch-manage/types/experimentations';

@Component({
  selector: 'experiments-overview',
  templateUrl: './overview.component.html',
  styleUrls: ['./overview.component.less']
})
export class OverviewComponent implements OnInit, OnDestroy {

  private search$ = new Subject<any>();
  isLoading: boolean = false;
  searchText: string = '';
  experimentList: any[] = [];

  currentProjectEnv: IProjectEnv = null;

  constructor(
    private router: Router,
    private message: NzMessageService,
    private experimentService: ExperimentService
  ) {
    this.currentProjectEnv = JSON.parse(localStorage.getItem('current-project'));
  }

  ngOnInit(): void {
    this.init();
    this.search$.next('');
  }

  onSearch() {
    this.search$.next(this.searchText);
  }

  getExptCountByStatus(status: ExperimentStatus): number {
    return this.experimentList.filter(expt => expt.status === status).length;
  }

  private init() {
    this.search$.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(e => {
      this.isLoading = true;
      this.experimentService.getExperiments({envId: this.currentProjectEnv.envId, searchText: e}).subscribe((result: IExperiment[]) => {
        if(result) {
          this.experimentList = result.map(r =>  Object.assign({}, r, { statusName: this.getStatusName(r.status)}));
        }

        this.isLoading = false;
      }, _ => {
        this.message.error("数据加载失败，请重试!");
        this.isLoading = false;
      })
    });
  }

  private getStatusName (status: ExperimentStatus) {
    switch(status){
      case ExperimentStatus.NotStarted:
        return '未开始';
      case ExperimentStatus.NotRecording:
        return '暂停';
      case ExperimentStatus.Recording:
        return '进行中';
      default:
        return '未开始';
    }
  }

  detailViewVisible: boolean = false;
  currentExperiment: IExperiment;
  onCreateClick() {
    this.currentExperiment = { envId: this.currentProjectEnv.envId } as IExperiment;
    this.detailViewVisible = true;
  }

  onDetailViewClosed(data: any) {
    this.detailViewVisible = false;

    if (data.data && data.data.id) {
      const experiment = Object.assign({}, data.data, { statusName: this.getStatusName(data.data.status)})
      this.experimentList = [experiment, ...this.experimentList];
    }
  }

  goToFeatureFlag(featureFlagId: string) {
    this.router.navigateByUrl(`/switch-manage/experimentations/${featureFlagId}`);
  }

  goToMetric(metricId: string) {
    this.router.navigateByUrl(`/experimentations/metrics?id=${metricId}`);
  }

  ngOnDestroy(): void {
    this.search$.complete();
  }
}
