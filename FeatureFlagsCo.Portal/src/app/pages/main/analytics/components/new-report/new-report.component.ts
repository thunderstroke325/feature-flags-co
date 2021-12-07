import { Component, Input } from '@angular/core';
import { DataCard, dataSource, IDataItem } from '../../types/analytics';
import { countMode } from '../../types/static';

@Component({
  selector: 'app-new-report',
  templateUrl: './new-report.component.html',
  styleUrls: ['./new-report.component.less']
})
export class NewReportComponent {

  @Input() dataSourceList: dataSource[] = [];
  @Input() currentCard: DataCard;
  @Input()
  set currentDataSource(value: IDataItem) {
    this.unit = value.unit || "";
    this.color = value.color || "#000000";
    this.dataSource = value.dataSource || {id: null, name: "", dataType: ""};

    this.calculationType = value.calculationType || countMode[0].id;

    setTimeout(() => {
      this.initNoSelectableLists();
    }, 0)
  }

  public countMode = countMode;                              // 计数方式
  public dataSource: dataSource;                             // 数据源
  public unit: string = "";                                  // 单位
  public calculationType = countMode[0].id;                  // 选择计数方式
  public color: string = "#000000";                          // 文字颜色

  public noSelectableDataSourceIDS: string[] = [];           // 不可选择的数据源 ID 列表
  public noSelectableCalculationTypeIDS: number[] = [];      // 不可选择的计数方式 ID 列表

  private initNoSelectableLists() {
    this.noSelectableDataSourceIDS = [];
    this.noSelectableCalculationTypeIDS = [];
    /**
     * 不能选择跟当前相同计数方式的其他数据源
     * 不能选择跟当前相同数据源的其他计数方式
     */

    // 遍历当前报表内的所有项目
    let items = this.currentCard.items;
    let len = items.length;

    if(len !== 0) {
      items.forEach((item: IDataItem) => {
        // 同计数方式
        if(item.calculationType === this.calculationType && item.dataSource) {
          this.noSelectableDataSourceIDS.push(item.dataSource.id);
        }

        // 同数据源
        if(this.dataSource.id && (item.dataSource && (item.dataSource.id === this.dataSource.id))) {
          this.noSelectableCalculationTypeIDS.push(item.calculationType);
        }
      })
    }
  }

  // 数据源发生改变，更新不可选择的计数方式列表
  public onSelectDataSource() {
    console.log(this.dataSource)
    this.initNoSelectableLists();
  }

  // 计数方式发生改变，更新不可选择的数据源列表
  public onSelectCalculationType() {
    this.initNoSelectableLists();
  }
  
  // 设置参数
  public setParam() {
    return {
      dataSource: this.dataSource,
      unit: this.unit,
      color: this.color,
      calculationType: this.calculationType
    }
  }
}
