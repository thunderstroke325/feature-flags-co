import { Component, OnInit } from '@angular/core';
import { uuidv4 } from "../../../utils";
import { EnvironmentSettingTypes } from "../../../config/types";
import { EnvSettingService } from "../../../services/env-setting.service";
import { NzMessageService } from "ng-zorro-antd/message";
import { DataSyncService } from "../../../services/data-sync.service";
import moment from "moment";
import { catchError } from "rxjs/operators";
import { forkJoin, of } from "rxjs";

interface SyncResult {
  success: boolean;
  time: string;
}

interface SyncUrlSettingRow {
  id: string;
  key: string;
  value: string;
  tag?: string;
  remark?: string;
  isSaving?: boolean;
  isEditing?: boolean;
  isDeleting?: boolean;
  isSyncing?: boolean;
  syncResult?: SyncResult;
}

@Component({
  selector: 'sync-urls-table',
  template: `
    <div class="table-operations">
      <nz-select nzShowSearch nzAllowClear nzPlaceHolder="选择类别进行筛选"
                 [(ngModel)]="filterTag" (ngModelChange)="filterRow()">
        <nz-option *ngFor="let item of tags" [nzLabel]="item" [nzValue]="item"></nz-option>
      </nz-select>
      <button nz-button nzType="default" (click)="syncAll()" [nzLoading]="isSyncingAll"
              nz-tooltip nzTooltipTitle="对表格中现有条目进行数据同步" nzTooltipPlacement="top">
        <i nz-icon [nzType]="'sync'"></i>
        数据同步
      </button>
      <button nz-button nzType="default" (click)="newRow()">
        <i nz-icon nzType="plus" nzTheme="outline"></i>
        新增同步地址
      </button>
    </div>
    <nz-table #editRowTable nzBordered [nzData]="filteredSettings" [nzLoading]="isLoading">
      <thead>
      <tr>
        <th nzWidth="10%">名称</th>
        <th nzWidth="15%">类别</th>
        <th nzWidth="30%">链接</th>
        <th nzWidth="20%">操作</th>
        <th nzWidth="25%">备注</th>
      </tr>
      </thead>
      <tbody>
      <tr *ngFor="let row of editRowTable.data" class="editable-row">

        <div *ngIf="row.isEditing, then editingRow else showRow"></div>
        <ng-template #editingRow>
          <td>
            <input type="text" nz-input [(ngModel)]="row.key" placeholder="地址名称"/>
          </td>
          <td>
            <nz-select nzShowSearch nzAllowClear [nzDropdownRender]="newItemTpl" nzPlaceHolder="选择类别" [(ngModel)]="row.tag">
              <nz-option *ngFor="let item of tags" [nzLabel]="item" [nzValue]="item"></nz-option>
            </nz-select>
            <ng-template #newItemTpl>
              <nz-divider></nz-divider>
              <div class="add-tag-container">
                <input #addTagInput type="text" nz-input/>
                <a class="add-tag-link" (click)="newTag(addTagInput)">
                  <i nz-icon nzType="plus"></i>
                  添加
                </a>
              </div>
            </ng-template>
          </td>
          <td>
            <input type="text" nz-input [(ngModel)]="row.value" placeholder="地址值"/>
          </td>
        </ng-template>
        <ng-template #showRow>
          <td>
            <div class="editable-cell" (click)="edit(row)">
              <span>{{ row.key }}</span>
            </div>
          </td>
          <td>
            <div class="editable-cell" (click)="edit(row)">
              <span>{{ row.tag }}</span>
            </div>
          </td>
          <td>
            <div class="editable-cell" (click)="edit(row)">
              <span>{{ row.value }}</span>
            </div>
          </td>
        </ng-template>

        <td>
          <button nz-button nzSize="default" nzType="link"
                  (click)="edit(row)"
                  *ngIf="!row.isEditing">
            <i nz-icon nzType="edit" nzTheme="outline"></i>
          </button>

          <button nz-button nzSize="default" nzType="link" [nzLoading]="row.isSaving"
                  (click)="save(row)"
                  *ngIf="row.isEditing">
            <i nz-icon nzType="save" nzTheme="outline"></i>
          </button>

          <button nz-button nzSize="default" nzType="link" [nzLoading]="row.isDeleting" nzDanger
                  [nzIcon]="iconTpl" nz-popconfirm nzPopconfirmTitle="确定删除么?" (nzOnConfirm)="deleteRow(row)">
            <i nz-icon nzType="delete" nzTheme="outline"></i>
            <ng-template #iconTpl>
              <i nz-icon nzType="question-circle-o" style="color: red;"></i>
            </ng-template>
          </button>

          <button nz-button nzSize="small" nzType="link" (click)="sync(row)" [nzLoading]="row.isSyncing">
            <i nz-icon [nzType]="'sync'"></i>
            数据同步
          </button>
        </td>

        <td>
          <span style="color: #4CB287" *ngIf="row.syncResult?.success">成功, 同步时间 {{row.syncResult.time}}</span>
          <span style="color: #ff7875"
                *ngIf="!row.syncResult?.success && row.syncResult?.time">失败, 同步时间 {{row.syncResult.time}}</span>
        </td>
      </tr>
      </tbody>
    </nz-table>
  `,
  styles: [`
    .table-operations {
      margin-bottom: 12px;
    }

    .table-operations nz-select {
      width: 180px;
    }

    .table-operations button {
      margin-left: 12px;
    }

    .editable-cell {
      position: relative;
      padding: 5px 12px;
      cursor: pointer;
    }

    .editable-row nz-select {
      width: 100%;
    }

    .editable-cell span {
      display: inline-block;
      height: 15px;
    }

    .editable-cell:hover {
      border: 1px solid #d9d9d9;
      border-radius: 4px;
      padding: 4px 11px;
    }

    .add-tag-container {
      display: flex;
      flex-wrap: nowrap;
      align-items: center;
      padding: 4px 8px;
    }

    .add-tag-link {
      display: block;
      flex: 0 0 auto;
      padding: 0 4px;
    }

    nz-divider {
      margin: 4px 0;
    }

    button {
      margin: 0 0;
    }
  `]
})
export class SyncUrlsTableComponent implements OnInit {

  constructor(
    private dataSyncService: DataSyncService,
    private settingService: EnvSettingService,
    private message: NzMessageService,
  ) {
  }

  ngOnInit(): void {
    this.isLoading = true;
    // init sync urls
    this.settingService.get(EnvironmentSettingTypes.SyncUrls).subscribe(
      settings => {
        this.allSettings = settings.map(s => Object.assign({}, {syncResult: this.getSyncResult(s.remark)}, s));
        this.filteredSettings = this.allSettings;

        // set tags
        this.refreshTags(settings);

        // load finish
        this.isLoading = false;
      }
    );
  }

  // get sync result from remark
  private getSyncResult(remark: string): SyncResult {
    if (!remark) {
      return;
    }

    const success = remark.split(',')[0] === 'true';
    const timestamp = remark.split(',')[1];
    const time = moment(timestamp, 'x').format('YYYY-MM-DD HH:mm');

    return { success, time };
  }

  isLoading: boolean = false;

  // settings
  allSettings: SyncUrlSettingRow[] = [];
  filteredSettings: SyncUrlSettingRow[] = [];

  // tags
  tags = [];
  filterTag: string = '';

  refreshTags(settings: SyncUrlSettingRow[]) {
    let newTags = [];
    settings.map(s => s.tag).forEach(tag => {
      if (tag && newTags.indexOf(tag) === -1) {
        newTags.push(tag);
      }
    });

    this.tags = newTags;
  }

  edit(row: SyncUrlSettingRow) {
    row.isEditing = true;
  }

  save(row: SyncUrlSettingRow) {
    row.isSaving = true;

    const setting = {
      id: row.id,
      type: EnvironmentSettingTypes.SyncUrls,
      key: row.key,
      value: row.value,
      tag: row.tag
    };

    this.settingService.upsert([setting]).subscribe(() => {
      row.remark = '';
      row.isSaving = false;
      row.isEditing = false;
      this.message.success('保存配置成功');
    });
  }

  isSyncingAll: boolean = false;
  syncAll() {
    this.isSyncingAll = true;
    this.filteredSettings.forEach(row => row.isSyncing = true);

    let tasks = [];
    this.filteredSettings.forEach(row => {
      let task = this.dataSyncService.syncToRemote(row.id)
        .pipe(catchError(error => of(`error: ${error.statusText} for url '${row.value}'`)));

      tasks.push(task);
    });

    const observable = forkJoin(tasks);
    observable.subscribe(results => {
      this.filteredSettings.forEach((row, index) => {
        this.handleSyncResponse(row, results[index] as string);
      });

      this.isSyncingAll = false;
    });
  }

  sync(row: SyncUrlSettingRow): void {
    row.isSyncing = true;

    this.dataSyncService.syncToRemote(row.id).subscribe(response => {
      this.handleSyncResponse(row, response);
    }, error => {
      row.isSyncing = false;
      this.message.error(`${error.status} ${error.statusText}, sync url is '${row.value}'`);
    });
  }

  private handleSyncResponse(row: SyncUrlSettingRow, response: string) {
    row.isSyncing = false;

    if (response.startsWith('error')) {
      this.message.error(response);
      return;
    }

    row.remark = response;
    row.syncResult = this.getSyncResult(response);
  }

  newRow(): void {
    let newRow = {
      id: uuidv4(),
      key: '',
      value: '',
      isEditing: true
    };

    this.allSettings = [...this.allSettings, newRow];
    this.filteredSettings = [...this.filteredSettings, newRow];
  }

  deleteRow(row: SyncUrlSettingRow): void {
    row.isDeleting = true;
    this.settingService.delete(row.id).subscribe(() => {
      this.allSettings = this.allSettings.filter(d => d.id !== row.id);
      this.filteredSettings = this.filteredSettings.filter(d => d.id !== row.id);

      this.message.success('删除配置成功');

      this.refreshTags(this.allSettings);
      row.isDeleting = false;
    });
  }

  filterRow() {
    this.filteredSettings = this.allSettings.filter(s => {
      if (this.filterTag) {
        return s.tag === this.filterTag;
      } else {
        return true;
      }
    });
  }

  newTag(el: HTMLInputElement): void {
    const tag = el.value;
    if (tag) {
      this.tags = [...this.tags, tag];
    }

    el.value = '';
  }
}
