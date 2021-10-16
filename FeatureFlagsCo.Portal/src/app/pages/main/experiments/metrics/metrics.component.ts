import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NzMessageService } from 'ng-zorro-antd/message';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { IAccount, IAccountUser, IProjectEnv } from 'src/app/config/types';
import { MetricService } from 'src/app/services/metric.service';
import { TeamService } from 'src/app/services/team.service';
import { CustomEventSuccessCriteria, CustomEventTrackOption, EventType, IMetric } from '../../switch-manage/types/experimentations';

@Component({
  selector: 'experiments-metrics',
  templateUrl: './metrics.component.html',
  styleUrls: ['./metrics.component.less']
})
export class MetricsComponent implements OnInit, OnDestroy {

  private search$ = new Subject<any>();
  isLoading: boolean = false;
  detailViewVisible: boolean = false;
  searchText: string = '';
  metricList: IMetric[] = [];
  accountMemberList: IAccountUser[] = [];

  currentProjectEnv: IProjectEnv = null;
  currentAccount: IAccount = null;

  currentMetric: IMetric;

  customEventType: EventType = EventType.Custom;
  customEventTrackConversion: CustomEventTrackOption = CustomEventTrackOption.Conversion;

  showType: '' | EventType = '';

  constructor(
    private route: ActivatedRoute,
    private message: NzMessageService,
    private teamService: TeamService,
    private metricService: MetricService
  ) {
    this.currentProjectEnv = JSON.parse(localStorage.getItem('current-project'));
    this.currentAccount = JSON.parse(localStorage.getItem('current-account'));

  //   const metricId = this.route.snapshot.queryParams['id'];
  //   if (metricId) {
  //     this.metricService.getMetric(this.currentProjectEnv.envId, metricId).subscribe(res => {
  //       if (res) {
  //         this.onCreateOrEditClick(res);
  //       }
  //     });
  //   }
  }

  ngOnInit(): void {
    this.init();
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
      this.metricService.getMetrics({envId: this.currentProjectEnv.envId, searchText: e}).subscribe((result: any) => {
        if(result) {
          this.metricList = result;
          this.setMaintainerNames();
        } else {
          this.isLoading = false;
        }
      }, _ => {
        this.message.error("数据加载失败，请重试!");
        this.isLoading = false;
      })
    });
    this.search$.next('');
  }

  private setMaintainerNames() {
    const unMatchedUserIds: string[] = [];
    this.metricList = this.metricList.map(m => {
      const match = this.accountMemberList.find(r => r.userId === m.maintainerUserId);
      if (match) {
        return Object.assign({}, m, { maintainerName: match.userName});
      } else {
        unMatchedUserIds.push(m.maintainerUserId);
        return Object.assign({}, m);
      }
    });

    if (unMatchedUserIds.length > 0) {
      this.teamService.getMembers(this.currentAccount.id).subscribe((result) => {
        this.accountMemberList = result;
        this.metricList = this.metricList.map(m => {
          return Object.assign({}, m, { maintainerName: result.find(r => r.userId === m.maintainerUserId)?.userName});
        });
        this.isLoading = false;
      }, _ => {
        this.isLoading = false;
      });
    } else {
      this.isLoading = false;
    }
  }

  onCreateOrEditClick(metric?: IMetric) {
    this.currentMetric = metric || { envId: this.currentProjectEnv.envId, eventType: EventType.Custom, customEventTrackOption: CustomEventTrackOption.Conversion, customEventSuccessCriteria: CustomEventSuccessCriteria.Higher } as IMetric;
    this.detailViewVisible = true;
  }

  onDetailViewClosed(data: any) {
    this.detailViewVisible = false;

    if (!data.isEditing && data.data && data.data.id) {
      this.metricList = [data.data, ...this.metricList];
    }

    if (data.isEditing && data.data) {
      this.metricList = this.metricList.map(m => {
        return m.id === data.data.id ? data.data : m;
      })
    }
  }

  ngOnDestroy(): void {
    this.search$.complete();
  }
}
