import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';
import { getAuth } from 'src/app/utils';
import { IAccount } from 'src/app/config/types';
import { AccountService } from 'src/app/services/account.service';

@Component({
  selector: 'app-account',
  templateUrl: './account.component.html',
  styleUrls: ['./account.component.less']
})
export class AccountComponent implements OnInit {

  creatAccountFormVisible: boolean = false;

  validateOrgForm!: FormGroup;

  auth = getAuth();
  currentAccount: IAccount;
  allAccounts: IAccount[];

  isLoading: boolean = false;

  sdkModeNpm = 'npm';
  sdkModeScript = 'script';
  sdkMode = this.sdkModeScript;

  sdkCodeNpm = '';
  sdkCodeScript = '';
  sdkCodeSetUserInfo = '';
  sdkNpmInstall = `
  npm install ffc-js-client-sdk --save`;

  constructor(
    private accountService: AccountService,
    private message: NzMessageService
  ) {
  }

  ngOnInit(): void {
    const currentAccountProjectEnv = this.accountService.getCurrentAccountProjectEnv();
    this.currentAccount = currentAccountProjectEnv.account;
    this.allAccounts = this.accountService.accounts;

    this.initOrgForm();

    this.sdkCodeScript = `
  <script data-ffc-client="${currentAccountProjectEnv.projectEnv.envSecret}" async src="https://assets.feature-flags.co/sdks/ffc-sdk.js"></script>`;

    this.sdkCodeSetUserInfo = `
  window.onload = (event) => {
    // 初始化用户信息，通常这一步会在登录后被调用
    FFCJsClient.initUserInfo({
        userName: '##{用户名}##',
        email: '##{用户邮箱（选填）}}##',
        key: '##{用户在产品中的唯一Id}##',
        customizeProperties: [ 
            {
                name: "##{自定义属性名称}##",
                value: "##{自定义属性值}##"
            }
        ]
    });
  });`;

    this.sdkCodeNpm = `
  import { FFCJsClient } from 'ffc-js-client-sdk/esm';

  FFCJsClient.initialize('${currentAccountProjectEnv.projectEnv.envSecret}');

  // 初始化用户信息，通常这一步会在登录后被调用
  FFCJsClient.initUserInfo({
      userName: '##{用户名}##',
      email: '##{用户邮箱（选填）}}##',
      key: '##{用户在产品中的唯一Id}##',
      customizeProperties: [ 
          {
              name: "##{自定义属性名称}##",
              value: "##{自定义属性值}##"
          }
      ]
  });`;
  }

  initOrgForm() {
    this.validateOrgForm = new FormGroup({
      organizationName: new FormControl(this.currentAccount.organizationName, [Validators.required]),
    });
  }

  onCreateAccountClick() {
    this.creatAccountFormVisible = true;
  }

  onCreateAccountClosed(account: IAccount) {
    this.creatAccountFormVisible = false;
    if (account) {
      this.accountService.accounts = [...this.accountService.accounts, account];
      this.accountService.changeAccount(account);
    }
  }

  onAccountChange() {
    this.accountService.changeAccount(this.currentAccount);
  }

  submitOrgForm() {
    if (this.validateOrgForm.invalid) {
      for (const i in this.validateOrgForm.controls) {
        this.validateOrgForm.controls[i].markAsDirty();
        this.validateOrgForm.controls[i].updateValueAndValidity();
      }
      return;
    }
    const { organizationName } = this.validateOrgForm.value;
    const { id } = this.currentAccount;

    this.isLoading = true;
    this.accountService.putUpdateAccount({ organizationName, id })
      .pipe()
      .subscribe(
        res => {
          this.isLoading = false;
          this.message.success('更新信息成功！');
          this.accountService.setAccountName({ id, organizationName });
        },
        err => {
          this.isLoading = false;
        }
      );
  }

}
