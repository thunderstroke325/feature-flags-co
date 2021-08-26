import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { NzMessageService } from 'ng-zorro-antd/message';
import { Subject } from 'rxjs';
import { AccountService } from 'src/app/services/account.service';
import { DataSyncService } from 'src/app/services/data-sync.service';

interface IEnvironmentData {
  version: string;
  date: string;
}

@Component({
  selector: 'app-data-sync',
  templateUrl: './data-sync.component.html',
  styleUrls: ['./data-sync.component.less']
})
export class DataSyncComponent implements OnInit, OnDestroy {

  @ViewChild('downloadRef', { static: false }) downloadRef: ElementRef;

  isDownloading: boolean = false;
  destory$: Subject<void> = new Subject();
  uploadFormVisible: boolean = false;
  currentEnvId: number;
  downloadFileName: string = null;

  constructor(
    private accountService: AccountService,
    private dataSyncService: DataSyncService,
    private message: NzMessageService,
    private sanitizer: DomSanitizer
  ) { }

  ngOnInit(): void {
    const currentAccountProjectEnv = this.accountService.getCurrentAccountProjectEnv();
    this.currentEnvId = currentAccountProjectEnv.projectEnv.envId;
  }

  ngOnDestroy(): void {
    this.destory$.next();
    this.destory$.complete();
  }

  onDownload() {
    this.isDownloading = true;
    this.dataSyncService.getEnvironmentData(this.currentEnvId, this.calculateDownloadOptions()).subscribe(data => this.downloadFile(data), err => {
      this.isDownloading = false;
      this.message.error("数据下载失败！");
    });
  }

  downloadUri: SafeUrl = null;
  downloadFile(data: IEnvironmentData) {
    this.downloadFileName = 'feature_flags_' + data.date + ".json";
    var theJSON = JSON.stringify(data);
    this.downloadUri = this.sanitizer.bypassSecurityTrustUrl("data:application/json;charset=UTF-8," + encodeURIComponent(theJSON));

    window.setTimeout(() => {
      this.isDownloading = false;
      this.downloadRef.nativeElement.click();
      this.downloadUri = null;
    }, 0);
  }

  openUploadForm() {
    this.uploadFormVisible = true;
  }

  onUploadClosed(data: any) {
    this.uploadFormVisible = false;
  }


  featureFlagsChecked: boolean = true;
  usersChecked: boolean = true;
  userPropertiesChecked: boolean = true;

  indeterminate: boolean = false;
  downloadAllChecked: boolean = true;
  updateAllChecked(): void {
    this.indeterminate = false;
    if (this.downloadAllChecked) {
      this.featureFlagsChecked = true;
      this.usersChecked = true;
      this.userPropertiesChecked = true;
    } else {
      this.featureFlagsChecked = false;
      this.usersChecked = false;
      this.userPropertiesChecked = false;
    }
  }

  updateSingleChecked(): void {
    if (!this.featureFlagsChecked && !this.usersChecked && !this.userPropertiesChecked) {
      this.downloadAllChecked = false;
      this.indeterminate = false;
    } else if (this.featureFlagsChecked && this.usersChecked && this.userPropertiesChecked) {
      this.downloadAllChecked = true;
      this.indeterminate = false;
    } else {
      this.indeterminate = true;
    }
  }

  private calculateDownloadOptions(): number {
    let result: number = 0;

    if(this.featureFlagsChecked) {
      result = result | 1;
    }

    if(this.usersChecked) {
      result = result | 2;
    }

    if(this.userPropertiesChecked) {
      result = result | 4;
    }

    return result;
  }
}
