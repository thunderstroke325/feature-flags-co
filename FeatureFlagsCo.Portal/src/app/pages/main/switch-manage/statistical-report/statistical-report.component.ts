import { AfterViewInit, Component, ElementRef, OnDestroy, OnInit, ViewChild, HostListener } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { SwitchService } from 'src/app/services/switch.service';
import * as echarts from 'echarts';
import { map } from 'rxjs/operators';
import { environment } from './../../../../../environments/environment';
import { AccountService } from 'src/app/services/account.service';
import * as moment from 'moment';

@Component({
  selector: 'report',
  templateUrl: './statistical-report.component.html',
  styleUrls: ['./statistical-report.component.less']
})
export class StatisticalReportComponent implements OnInit, OnDestroy, AfterViewInit {

  @ViewChild('reportEcharts', { static: false }) echartsRef: ElementRef;

  private destory$: Subject<void> = new Subject();
  private myChart: any;
  public switchId: string = '';
  public height: number = 0;
  public width: number = 0;
  public timeSpan: string = 'P7D';

  public totalUsers: number = 0;
  public hitUsers: number = 0;
  public userUsageStr: string = '';
  public hideContent: boolean = false;

  private xname: string = '时间';
  private yname: string = '';

  public isLoading: boolean = false;
  public navReportConfig: object[] = [
    {keyValue: 'P7D', labelValue: '查看最近7天'},
    {keyValue: 'P1D', labelValue: '查看最近24小时'},
    {keyValue: 'PT2H', labelValue: '查看最近2小时'},
    {keyValue: 'PT30M', labelValue: '查看最近30分钟'},
  ];

  constructor(
    private route: ActivatedRoute,
    private switchServe: SwitchService,
    private accountService: AccountService
  ) {
    this.switchId = this.route.snapshot.params['id'];
  }

  @HostListener('window:resize', ['$event'])
  onResize(evt) {
    this.width = evt.target.innerWidth;
    this.height = this.echartsRef.nativeElement.offsetHeight;
  }

  ngOnInit(): void {
    // if (environment.name === 'Standalone') {
    //   this.hideContent = true;
    //   const currentAccountProjectEnv = this.accountService.getCurrentAccountProjectEnv();

    //   if (environment.statisticUrl[environment.statisticUrl.length - 1] === '/') {
    //     environment.statisticUrl = environment.statisticUrl.slice(0, environment.statisticUrl.length - 1);
    //   }

    //   const url = environment.statisticUrl + `/explore?left=["now-2d","now","Loki",{"expr":"{EnvId=\\"${currentAccountProjectEnv.projectEnv.envId}\\",FeatureFlagKeyName=\\"${this.switchServe.getCurrentSwitch().keyName}\\"}"}]`;
    //   window.open(url, "_blank");
    // }
  }

  ngAfterViewInit(): void {
    if (!this.hideContent) {
      window.setTimeout(() => {
        this.height = this.echartsRef.nativeElement.offsetHeight;
        this.width = this.echartsRef.nativeElement.offsetWidth;
      });
      this.myChart = echarts.init(this.echartsRef.nativeElement);
      this.getFeatureFlagUsage();
    }
  }

  ngOnDestroy(): void {
    this.destory$.next();
    this.destory$.complete();
  }

  public getFeatureFlagUsage() {
    window.setTimeout(() => {
      this.isLoading = false;
    });
    this.switchServe.getReport(this.switchId, this.timeSpan)
      .pipe(
        map(res => {
          this.totalUsers = res.totalUsers || 0;
          this.hitUsers = res.hitUsers || 0;
          // let userUsageStr = `共有${res.totalUsers || 0}用户被标记，其中${res.hitUsers || 0}人使用此功能`;
          // if (this.totalUsers === 0 && this.hitUsers === 0)
          //   userUsageStr = "";
          // if (this.totalUsers === 0 && this.hitUsers === 0 && res.userDistribution && res.userDistribution !== null &&
          //   res.userDistribution.tables && res.userDistribution.tables !== null && res.userDistribution.tables.length > 0) {
          //   for (let i = 0; i < res.userDistribution.tables.length; i++) {
          //     let usage = res.userDistribution.tables[0];
          //     if (usage && usage.rows && usage.rows.length > 0) {
          //       userUsageStr = '';
          //       for (let j = 0; j < usage.rows.length; j++) {
          //         userUsageStr += `${usage.rows[j][0]}: ${usage.rows[j][1]}个用户; `
          //       }
          //     }
          //   }
          // }
          let userUsageStr = "";
          let userByVariationValue = JSON.parse(res.userByVariationValue);

          if (userByVariationValue && userByVariationValue.aggregations &&
            userByVariationValue.aggregations.group_by_status &&
            userByVariationValue.aggregations.group_by_status.buckets &&
            userByVariationValue.aggregations.group_by_status.buckets.length > 0) {
            let buckets = userByVariationValue.aggregations.group_by_status.buckets;
            for (let i = 0; i < buckets.length; i++) {
              userUsageStr += `| ${buckets[i].key}: ${buckets[i].doc_count}个用户 `
            }
            userUsageStr += "|";
          }
          this.userUsageStr = userUsageStr;

          let chartData = JSON.parse(res.chartData);
          return chartData || {};
        })
      )
      .subscribe(
        res => {
          let buckets = [];
          if (res && res.aggregations &&
            res.aggregations.range &&
            res.aggregations.range.buckets &&
            res.aggregations.range.buckets.length > 0) {
            buckets = res.aggregations.range.buckets;
          }
          const series = this.formatSeries(buckets);
          const xAxis = this.formatXAxis(buckets)
          this.yname = this.formatYname(res);
          const option = {
            title: {
              // text: '下图为用户触发开关标记判断的次数分布图'
              text: ''
            },
            tooltip: {
              trigger: 'axis',
              formatter: (params: any) => {
                if (!params || !params[0] || !params[0].value) return '';
                return `
                <span>
                  ${this.xname}: <span style="font-weight: bold">${params[0].axisValue.slice(5, -3)}</span>
                  <br/>
                  ${this.yname}: <span style="font-weight: bold">${params[0].value}</span>
                </span>`
              },
            },
            // xAxis: {
            //   name: this.xname,
            //   type: 'time',
            // },
            xAxis: xAxis,
            yAxis: {
              name: this.yname,
              type: 'value',
              boundaryGap: [0, '100%'],
            },
            series
          };
          this.isLoading = false;
          this.myChart.setOption(option);
        },
        _ => {
          this.isLoading = false;
        }
      );
  }

  public onTimeSpanClcik(timeSpan: string) {
    this.timeSpan = timeSpan;
    this.getFeatureFlagUsage();
  }

  private formatXAxis(option: any) {
    let data = [];
    if (option && option.length > 0) {
      for (let i = 0; i < option.length; i++) {
        var date = new Date(option[i].to_as_string.replace('T', ' ') + ' UTC');
        data.push(moment(date).format("YYYY-MM-DD HH:mm:ss"));
      }
    }

    return {
      name: "日期时间",
      // type: 'time',
      data: data
    }
  }

  private formatYname(option: any) {
    // const tables = option.tables || [];
    // return tables[0]?.columns?.[1]?.name || '';
    // const tables = option.tables || [];
    // return tables[0]?.columns?.[1]?.name || '';
    return "被调用次数";
  }

  private formatSeries(option: any) {
    // const tables = option.tables || [];
    let data = [];
    if (option && option.length > 0) {
      for (let i = 0; i < option.length; i++) {
        data.push(option[i].doc_count);
      }
    }
    // return tables.map(item => {
    //   return {
    //     name: item.name,
    //     type: 'line',
    //     data: item.rows.sort((a: string[], b: string[]) => new Date(a[0] || '').getTime() - new Date(b[0] || '').getTime())
    //   }
    // });
    return {
      name: "日期时间",
      type: 'line',
      data: data
    };
  }
}
