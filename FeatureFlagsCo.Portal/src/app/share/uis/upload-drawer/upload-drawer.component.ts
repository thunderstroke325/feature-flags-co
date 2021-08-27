import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzUploadChangeParam, NzUploadFile } from 'ng-zorro-antd/upload';
import { AccountService } from 'src/app/services/account.service';
import { DataSyncService } from 'src/app/services/data-sync.service';

@Component({
  selector: 'app-upload-drawer',
  templateUrl: './upload-drawer.component.html',
  styleUrls: ['./upload-drawer.component.less']
})
export class UploadDrawerComponent implements OnInit {

  uploadUrl: string = null;
  isUploading: boolean = false;
  currentEnvId: number;
  currentProjectId: number;
  currentAccountId: number;
  fileList: NzUploadFile[] = [];

  @Input() visible: boolean = false;
  @Output() close: EventEmitter<any> = new EventEmitter();

  constructor(
    private accountService: AccountService,
    private message: NzMessageService,
    private dataSyncService: DataSyncService
  ) { }

  ngOnInit(): void {
    const currentAccountProjectEnv = this.accountService.getCurrentAccountProjectEnv();
    this.currentAccountId = currentAccountProjectEnv.account.id;
    this.currentEnvId = currentAccountProjectEnv.projectEnv.envId;
    this.uploadUrl = this.dataSyncService.getUploadUrl(this.currentEnvId);
  }

  onClose() {
    this.close.emit();
  }

  handleChange(info: NzUploadChangeParam): void {
    let fileList = [...info.fileList];

    // 1. Limit the number of uploaded files
    // Only to show the latest uploaded files, and old ones will be replaced by the new
    fileList = fileList.slice(-1);

    // 2. Read from response and show file link
    fileList = fileList.map(file => {
      if (file.response) {
        // Component will show file.url as link
        file.url = file.response.url;
      }
      return file;
    });

    this.fileList = fileList;

    if (info.file.status === 'uploading') {
      this.isUploading = true;
    } else {
      this.isUploading = false;
      console.log(info.file, info.fileList);

      if (info.file.status === 'done') {
        this.message.success(`${info.file.name} 文件上传成功`);
      } else if (info.file.status === 'error') {
        this.message.error(`${info.file.name} 文件上传失败.`);
      }
    }
  }
}
