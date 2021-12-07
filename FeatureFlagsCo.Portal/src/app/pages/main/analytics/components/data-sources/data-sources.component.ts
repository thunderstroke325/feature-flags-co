import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { dataSource } from '../../types/analytics';

@Component({
  selector: 'data-sources',
  templateUrl: './data-sources.component.html',
  styleUrls: ['./data-sources.component.less']
})
export class DataSourcesComponent implements OnInit {

  @Input() dataSourceList: dataSource[] = [];             // 数据源列表
  @Input() boardType: "table" | "form" = "table";         // 显示与操作
  @Input() currentDataSource: dataSource;                 // 当前数据源

  @Output() deleteDataSource = new EventEmitter<dataSource>();        // 删除数据源
  @Output() boardTypeChange = new EventEmitter<"table" | "form">();   // boardType 双向数据绑定
  @Output() edit = new EventEmitter<void>();                // 编辑数据源

  constructor() { }

  ngOnInit(): void {
  }

  // 设置参数
  public setParam(): dataSource {
    return this.currentDataSource;
  }

  // 删除数据源
  public onDeleteDataSource(dataSource: dataSource) {
    this.deleteDataSource.next(dataSource);
  }

  // 切换修改数据源界面
  public editDataSource(dataSource: dataSource) {
    this.currentDataSource = dataSource;
    this.boardTypeChange.emit("form");
    this.edit.emit();
  }
}
