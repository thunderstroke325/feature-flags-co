import { StringMap } from '@angular/compiler/src/compiler_facade_interface';
import { Component, OnInit } from '@angular/core';
import moment from 'moment';
import { NzMessageService } from 'ng-zorro-antd/message';

enum CalculationType {
  Count = 1,
  Sum = 2,
  Average = 3
}

interface IDataCard {
  id: string,
  name: string,
  startTime: Date,
  startTimeStr?: string,
  endTime: Date,
  endTimeStr?: string,
  items: IDataItem[],
  isLoading?: boolean // only for UI
}

interface IDataItem {
  id: string,
  name: string,
  value: number,
  unit: string,
  color: string,
  calculationType: CalculationType
}

@Component({
  selector: 'app-analytics',
  templateUrl: './analytics.component.html',
  styleUrls: ['./analytics.component.less']
})
export class AnalyticsComponent implements OnInit {

  listData: IDataCard[] = [];
  isLoading = false;
  constructor(
    private message: NzMessageService
  ) {
    this.listData = new Array(3).fill({}).map((_i, index) => ({
      id: `${_i}`,
      name: `我的第 ${index} 个看板`,
      startTime: new Date(),
      endTime: new Date(),
      startTimeStr: moment(new Date()).format('YYYY-MM-DD'),
      endTimeStr:  moment(new Date()).format('YYYY-MM-DD'),
      isLoading: true,
      items: new Array(6).fill({}).map((_i, index) => ({
        id: `${_i}`,
        name: `Date item  ${index}`,
        value: parseFloat((Math.random() * 100).toFixed(2)),
        unit: 'EUR',
        color: 'red',
        calculationType: CalculationType.Count
      }))
    }));

    // TODO to be removed
    setTimeout(() => this.listData.forEach(d => d.isLoading = false), 200);
  }

  ngOnInit(): void {
  }
}
