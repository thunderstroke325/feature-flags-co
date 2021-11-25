import { Component, OnInit } from '@angular/core';
import { NzMessageService } from 'ng-zorro-antd/message';
import { uuidv4 } from 'src/app/utils';

enum CalculationType {
  Count = 1,
  Sum = 2,
  Average = 3
}

interface IDataCard {
  id: string,
  name: string,
  startTime: Date,
  endTime: Date,
  items: IDataItem[],
  isLoading?: boolean, // only for UI
  isEditing?: boolean
}

class DataCard {
  id: string;
  name: string;
  startTime: Date;
  endTime: Date;
  items: IDataItem[];
  isLoading?: boolean; // only for UI
  isEditing?: boolean;

  constructor(data?: IDataCard) {
    if (data) {
      this.id = data.id;
      this.name = data.name;
      this.startTime = data.startTime;
      this.endTime = data.endTime;
      this.isLoading = data.isLoading;
      this.isEditing = data.isEditing;
      this.items = [...data.items];
    } else {
      this.id = uuidv4();
      this.isLoading = false;
      this.isEditing = true;
      this.items = [];
      // new Array(6).fill({}).map((_i, index) => ({
      //   id: uuidv4(),
      //   name: null,
      //   value: null,
      //   unit: null,
      //   color: null,
      //   calculationType: CalculationType.Count
      // }))
    }
  }

  disabledStartDate = (startTime: Date): boolean => {
    if (!startTime || !this.endTime) {
      return false;
    }
    return startTime.getTime() > this.endTime.getTime();
  }
 
  disabledEndDate = (endTime: Date): boolean => {
    if (!endTime || !this.startTime) {
      return false;
    }
    return endTime.getTime() <= this.startTime.getTime();
  }

  clearStartDate = (): void => {
    this.startTime = null;
  }
 
  clearEndDate = (): void => {
    this.endTime = null;
  }
}

interface IDataItem {
  id: string,
  name: string,
  value: number,
  dataSource: string,
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

  //ranges = { 今天: [new Date(), new Date()], '本月': [new Date(), endOfMonth(new Date())] };
  listData: DataCard[] = [];
  isLoading = false;
  dataSourceModalVisible = false;
  constructor(
    private message: NzMessageService
  ) {
    this.listData = new Array(3).fill({}).map((_, index) => new DataCard({
        id: `${index}`,
        name: `我的第 ${index} 个看板`,
        startTime: new Date(),
        endTime: new Date(),
        isLoading: true,
        items: new Array(6).fill({}).map((_i, index) => ({
          id: `${index}`,
          name: `Data item  ${index}`,
          value: parseFloat((Math.random() * 100).toFixed(2)),
          dataSource: 'sdf',
          unit: 'EUR',
          color: 'red',
          calculationType: CalculationType.Count
        }))
      })
    );

    // TODO to be removed
    setTimeout(() => this.listData.forEach(d => d.isLoading = false), 200);
  }

  ngOnInit(): void {
  }

  onDateChange(data: any){
    console.log('data changed');
  }

  toggleEditingCard(card: IDataCard) {
    card.isEditing = !card.isEditing;
    card.items = card.items.filter(i => i.name !== null && i.name !== '' && i.dataSource !== null && i.dataSource !== '');
  }

  onCreateCard() {
    const card = new DataCard();
    this.onAddItem(card);
    this.listData = [card, ...this.listData];
  }

  onAddItem(data: DataCard) {
    data.items = [...data.items, {
        id: uuidv4(),
        name: null,
        value: null,
        dataSource: null,
        unit: null,
        color: null,
        calculationType: CalculationType.Count
    }]
  }

  removeCard(card: IDataCard) {
    const idx = this.listData.findIndex(d => d.id === card.id);
    if (idx > -1) {
      this.listData.splice(idx, 1);
    }
  }

  removeDataItem(card: DataCard, item: IDataItem) {
    const idx = card.items.findIndex(i => i.id === item.id);
    if (idx > -1) {
      card.items.splice(idx, 1)
    }
  }

  currentDataItem: IDataItem = null;
  currentDataCard: DataCard = null;
  onSetDataSource(card: DataCard, item: IDataItem) {
    this.currentDataItem = Object.assign({}, item);
    this.currentDataCard = card;
    this.dataSourceModalVisible = true;
  }

  onApplyDataSource () {
    let item = this.currentDataCard.items.find(i => i.id === this.currentDataItem.id);
    if (item) {
      item = Object.assign({}, this.currentDataItem);
    }

    this.dataSourceModalVisible = false;
  }
}

