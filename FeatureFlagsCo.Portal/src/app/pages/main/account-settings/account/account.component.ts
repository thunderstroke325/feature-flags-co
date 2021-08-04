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

  validateOrgForm!: FormGroup;

  auth = getAuth();
  currentAccount: IAccount;
  allAccounts: IAccount[];

  isLoading: boolean = false;

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
  }

  initOrgForm() {
    this.validateOrgForm = new FormGroup({
      organizationName: new FormControl(this.currentAccount.organizationName, [Validators.required]),
    });
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
