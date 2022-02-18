import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { NzMessageService } from 'ng-zorro-antd/message';
import { Subject } from 'rxjs';
import { AccountService } from 'src/app/services/account.service';
import { DataSyncService } from 'src/app/services/data-sync.service';
import { FfcService } from 'src/app/services/ffc.service';

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

  // 用户行为数据
  userBehaviorDataDownloadFunc: boolean = false;
  userBehaviorDataDownloading: boolean = false;
  dataDateRange: Date[] = [];

  constructor(
    private accountService: AccountService,
    private dataSyncService: DataSyncService,
    private message: NzMessageService,
    private sanitizer: DomSanitizer,
    private ffcService: FfcService
  ) { }

  ngOnInit(): void {
    const currentAccountProjectEnv = this.accountService.getCurrentAccountProjectEnv();
    this.currentEnvId = currentAccountProjectEnv.projectEnv.envId;

    this.userBehaviorDataDownloadFunc =
      this.ffcService.variation('user-behavior-data-download', "false") !== "false";
  }

  ngOnDestroy(): void {
    this.destory$.next();
    this.destory$.complete();
  }

  onDownload() {
    this.isDownloading = true;
    this.dataSyncService.getEnvironmentData(this.currentEnvId).subscribe(data => this.downloadFile(data), _ => {
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

  onUploadClosed() {
    this.uploadFormVisible = false;
  }

  // 用户行为数据下载
  public onDownloadUserBehaviorData() {
    if (this.dataDateRange.length) {
      this.userBehaviorDataDownloading = true;
      const startTimestamp = (new Date(this.dataDateRange[0])).valueOf();
      const endTimestamp = (new Date(this.dataDateRange[1])).valueOf();

      this.dataSyncService.getUserBehaviorData(
        this.currentEnvId,
        {
          startTimestamp, endTimestamp
        }
      ).subscribe((result) => {

        const data = JSON.stringify(result);

        let blob = new Blob([data], {type: 'text/json'});
        let e = document.createEvent("MouseEvents");
        let a = document.createElement("a");
        a.download = "result.json";
        a.href = window.URL.createObjectURL(blob);
        a.dataset.downloadurl = ['text/json', a.download, a.href].join(':');
        e.initEvent('click', true, true);
        a.dispatchEvent(e);
        a.remove();

        this.userBehaviorDataDownloading = false;
      }, _ => {
        this.userBehaviorDataDownloading = false;
        this.message.error("数据下载失败!");
      })
    } else {
      this.message.error("请选择数据下载的时间范围!");
    }
  }
}
