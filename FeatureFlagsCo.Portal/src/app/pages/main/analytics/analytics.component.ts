import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { NzMessageService } from 'ng-zorro-antd/message';
import { AccountService } from 'src/app/services/account.service';
import { AnalyticsService } from 'src/app/services/analytics.service';
import { uuidv4 } from 'src/app/utils';
import { NewReportComponent } from './components/new-report/new-report.component';
import {
  CalculationType,
  DataCard,
  dataSource,
  IDataCard,
  IDataItem,
  updataReportParam,
  Dimension,
  DimensionModalOptions
} from './types/analytics';
import { DataSourcesComponent } from './components/data-sources/data-sources.component';
import { dataGrouping, sameTimeGroup } from './types/data-grouping';
import { hasSameKeyName } from './types/same-keyname';

@Component({
  selector: 'app-analytics',
  templateUrl: './analytics.component.html',
  styleUrls: ['./analytics.component.less']
})
export class AnalyticsComponent implements OnInit {

  listData: DataCard[] = [];
  isLoading = false;
  dataSourceModalVisible = false;

  public dataSourcesManageModalVisible = false;

  private analyticBoardId: string = "";
  private envID: number = null;

  public dataSourceList: dataSource[] = [];                 // 数据源列表
  public dataSourceBoardType: 'table' | 'form' = 'table';   // 添加数据源弹窗界面类型
  public dataSourceOperatorType: 'new' | 'edit' = 'new';    // 如何操作数据源
  public currentDataSource: dataSource;                     // 当前数据源
  public currentDataItem: IDataItem = null;
  public currentDataCard: DataCard = null;

  public reportsForCurrDataSource: any[] = [];              // 当前将要被删除的数据源，被使用的报表列表
  public willSaveCard: IDataCard;                           // 将要保存的报表

  // analytic dimension modal options
  public dimensionModalOptions = new DimensionModalOptions();

  @ViewChild("addDataSourceTem", {static: false}) addDataSoureTem: TemplateRef<any>;
  @ViewChild("dataSources", {static: false}) dataSourceCom: DataSourcesComponent;
  @ViewChild("newReport", {static: false}) newReportCom: NewReportComponent;

  constructor(
    private message: NzMessageService,
    private accountServe: AccountService,
    private analyticServe: AnalyticsService
  ) { }

  ngOnInit(): void {
    this.initBoardData();
  }

  // 初始化看板数据
  private initBoardData() {
    const { projectEnv: {envId} } = this.accountServe.getCurrentAccountProjectEnv();

    this.analyticServe.getAnalyticBoardData(envId)
      .subscribe((result: {
        id: string;
        envId: number;
        dataSourceDefs: dataSource[],
        dimensions: Dimension[],
        dataGroups: DataCard[]
      }) => {
        this.listData = [];

        this.analyticBoardId = result.id;
        this.envID = result.envId;
        this.dataSourceList = result.dataSourceDefs;
        this.dimensionModalOptions.dimensions = result.dimensions;

        let groups = result.dataGroups;
        groups.forEach((group: DataCard) => {
          group.items.length && group.items.map((item: IDataItem) => {
            item.isLoading = true;
            item.isSetupDataSource = true;
          });
          this.listData[this.listData.length] = new DataCard({...group, isLoading: false, isEditing: false, itemsCount: group.items.length});
        })

        this.requestValueForItems();
      })
  }

  // 设置每个 item 的 value 值
  private requestValueForItems() {
    dataGrouping(this.listData, this.envID).forEach((item: sameTimeGroup) => {
      this.analyticServe.computeResult(item).subscribe(result =>  this.setItemValue(result.items, this.listData));
    })
  }

  // 设置 item 的 value 值
  private setItemValue(result: {id: string, value: number}[], list: IDataCard[]) {
    result.forEach((item: {id: string, value: number}) => {
      next: for(let i = 0; i < list.length; i++) {
        const items = list[i].items;

        if(items.length) {
          for(let j = 0; j < items.length; j++) {
            if(items[j].id === item.id) {
              items[j].isLoading = false;
              items[j].value = item.value;
              break next;
            }
          }
        }
      }
    })
  }

  // 切换看板状态
  public toggleEditingCard(card: IDataCard) {
    if(card.isEditing) {

      if(!card.name) {
        this.message.error("必须填写报表名字!");
        return;
      }

      // 表报必须填写开始日期
      const isStartTime = card.startTime;

      if(!isStartTime) {
        this.message.error("必须填写开始日期!");
        return;
      }

      // 筛选是否有没有 name 值和 DataSource 的 选项
      const result = card.items.filter(item => !item.name || !item.dataSource);

      if(result.length) {
        this.willSaveCard = card;
        card.isTooltip = true;
      } else {
        this.setSaveReportParam(card);
      }
    } else {
      card.isEditing = true;
    }
  }

  // 确认保存
  public onSureToSave() {
    this.willSaveCard.items = this.willSaveCard.items.filter(i => i.name !== null && i.name !== '' && i.dataSource !== null);
    this.setSaveReportParam(this.willSaveCard);
  }

  // 设置保存报表参数
  private setSaveReportParam(card: IDataCard) {
    let param: updataReportParam = {
      analyticBoardId: this.analyticBoardId,
      envId: this.envID,
      id: card.id,
      name: card.name || null,
      items: card.items,
      startTime: card.startTime,
      endTime: card.endTime
    }
    this.onSaveReportData(param, card);
  }

  // 保存报表
  private onSaveReportData(param: updataReportParam, card: IDataCard) {
    card.isLoading = true;
    this.analyticServe.saveReport(param)
      .subscribe(() => {
        this.message.success("报表更新成功!");
        card.isEditing = false;
        card.itemsCount = card.items.length;
        card.isTooltip = false;
        card.isLoading = false;

        const param: sameTimeGroup = {
          envId: this.envID,
          startTime: card.startTime,
          endTime: card.endTime,
          items: card.items
        }

        this.analyticServe.computeResult(param)
          .subscribe(result => this.setItemValue(result.items, [card]))
      })
  }

  // 创建报表
  public onCreateCard() {
    const card = new DataCard();
    this.currentDataCard = card;
    this.onAddItem(card);
    this.listData = [card, ...this.listData];
  }

  // 添加 item
  public onAddItem(data: DataCard) {
    data.items = [...data.items, {
        id: uuidv4(),
        name: null,
        value: null,
        dataSource: null,
        unit: null,
        color: null,
        calculationType: CalculationType.Count,
        isSetupDataSource: false
    }]

    data.itemsCount = data.itemsCount + 1;
  }

  // 删除报表
  public removeCard(card: IDataCard) {
    this.analyticServe.deleteReport(this.envID, this.analyticBoardId, card.id)
      .subscribe(() => {
        const idx = this.listData.findIndex(d => d.id === card.id);
        idx > -1 && this.listData.splice(idx, 1);
        this.message.success("表报移除成功!");
      })
  }

  // 删除 item
  public removeDataItem(card: DataCard, item: IDataItem) {
    const idx = card.items.findIndex(i => i.id === item.id);
    if (idx > -1) {
      card.items.splice(idx, 1);
      card.itemsCount = card.itemsCount - 1;
    }
  }

  // 点击设置数据源（打开设置数据源弹窗）
  public onSetDataSource(card: DataCard, item: IDataItem) {
    this.currentDataItem = {...item};
    this.currentDataCard = card;
    this.dataSourceModalVisible = true;
  }

  // 切换数据源界面
  public onOperatorDataSource() {
    if(this.dataSourceBoardType === 'table') {
      this.currentDataSource = {
        id: uuidv4(),
        name: "",
        dataType: "数值",
        keyName: ""
      }

      this.dataSourceOperatorType = 'new';
      this.dataSourceBoardType = 'form';
    } else {
      // 检查是否有重复的 keyName
      const dataSource: dataSource = this.dataSourceCom.setParam();
      const isHas = hasSameKeyName(this.dataSourceList, dataSource);

      if(!isHas) {
        this.onAddDataSource(dataSource);
      } else {
        this.message.error("当前数据源的 keyName 重复，请设置不同的 keyName")
      }
    }
  }

  // 操作取消按钮
  public onOperatorCancel() {
    if(this.dataSourceBoardType === 'table') {
      this.dataSourcesManageModalVisible = false;
    } else {
      this.dataSourceBoardType = "table";
    }
  }

  // 设置数据源
  public onApplyDataSource () {
    this.currentDataItem = {...this.currentDataItem, ...this.newReportCom.setParam(), isSetupDataSource: true};
    this.currentDataCard.updateItem(this.currentDataItem);
    this.dataSourceModalVisible = false;
  }

  // 刷新页面
  public onRefreshPage() {
    this.initBoardData();
  }

  // 打开添加数据源弹窗
  public onOpenDataSoureModal() {
    this.dataSourcesManageModalVisible = true;
  }

  // 关闭添加数据源弹窗
  public onCloseAddDataSoureModal() {
    this.dataSourcesManageModalVisible = false;
  }

  // 关闭/返回 分析维度弹窗
  public onCloseDimensionModal() {
    this.dimensionModalOptions.handleCancel();
  }

  // 新建/提交 分析维度弹窗
  public onSaveDimensionModal() {
    if (this.dimensionModalOptions.mode === 'show-list') {
      // set current dimension
      const initialDimension = {
        id: uuidv4(),
        key: "",
        value: ""
      };
      this.dimensionModalOptions.handleOk(initialDimension);
    } else {
      let currentDimension = this.dimensionModalOptions.currentDimension;
      const param = {
        analyticBoardId: this.analyticBoardId,
        envID: this.envID,
        id: currentDimension.id,
        key: currentDimension.key,
        value: currentDimension.value
      };

      this.analyticServe.upsertDimension(param)
        .subscribe(() => {
          this.dimensionModalOptions.upsertDimension();
        });
    }
  }

  // 删除分析维度
  public onDeleteDimension(dimension: Dimension) {
    this.analyticServe.deleteDimension(this.envID, this.analyticBoardId, dimension.id)
      .subscribe(() => {
        this.dimensionModalOptions.deleteDimension(dimension);
      })
  }

  // 确认添加数据源
  private onAddDataSource = (dataSource: dataSource) => {

    const newDataSource = {
      analyticBoardId: this.analyticBoardId,
      envID: this.envID,
      id: dataSource.id,
      name: dataSource.name,
      keyName: dataSource.keyName,
      dataType: dataSource.dataType
    }
    this.analyticServe.addDataSourece(newDataSource)
      .subscribe(() => {
        this.dataSourceOperatorType === 'new' && (this.dataSourceList[this.dataSourceList.length] = dataSource);
        this.dataSourceBoardType = "table";
      })
  }

  // 删除数据源
  public onDeleteDataSource(dataSource: dataSource) {
    // 检测是否有使用过该数据源的报表
    const isHas = this.findReportsForCurrentDataSource(dataSource);

    if(isHas) {
      this.onApplyDelete(dataSource);
    } else {
      // 显示使用当前数据源的报表
      this.reportsForCurrDataSource.forEach(report => {
        this.message.warning(`当前数据源正在被报表 “${report.name}” 的 “${report.itemName}” 项目使用！`);
      })
    }
  }

  // 查找正在使用当前数据源的报表
  private findReportsForCurrentDataSource(dataSource: dataSource): boolean {
    this.reportsForCurrDataSource = [];

    this.listData.forEach((data: DataCard) => {
      if(data.items.length) {
        data.items.forEach((item: IDataItem) => {
          if(item.dataSource.id === dataSource.id) {
            this.reportsForCurrDataSource[this.reportsForCurrDataSource.length] = {
              name: data.name,
              itemName: item.name
            };
          }
        })
      }
    })

    return this.reportsForCurrDataSource.length === 0;
  }

  // 执行删除
  private onApplyDelete(dataSource: dataSource) {
    this.analyticServe.deleteDateSource(this.envID, this.analyticBoardId, dataSource.id)
      .subscribe(() => {
        const index = this.dataSourceList.findIndex(data => data.id === dataSource.id);
        if(index !== -1) {
          this.dataSourceList.splice(index, 1);
          this.dataSourceList = [...this.dataSourceList];
        }
      })
  }
}

