import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { NzTableQueryParams } from 'ng-zorro-antd/table';
import { Subject } from 'rxjs';
import { UserService } from 'src/app/services/user.service';
import { AccountService } from 'src/app/services/account.service';


@Component({
  selector: 'app-user-list',
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.less']
})
export class UserListComponent implements OnInit, OnDestroy {

  destory$: Subject<void> = new Subject();

  currentEnvId: number;
  currentAccountId: number;

  list = [];
  totalCount: number;

  pageSize: number = 10;
  pageIndex: number = 0;

  searchValue: string = '';

  isLoading: boolean = false;
  visible: boolean = false;

  constructor(
    private userService: UserService,
    private accountService: AccountService,
    private router: Router
  ) {
  }

  ngOnInit(): void {

    const currentAccountProjectEnv = this.accountService.getCurrentAccountProjectEnv();
    this.currentAccountId = currentAccountProjectEnv.account.id;
    this.currentEnvId = currentAccountProjectEnv.projectEnv.envId;
    this.fetchUserList();
  }

  ngOnDestroy(): void {
    this.destory$.next();
    this.destory$.complete();
  }

  // initFFC(): void {

  // }

  fetchUserList() {
    this.isLoading = true;
    this.userService.getEnvUsers({ pageIndex: this.pageIndex, pageSize: this.pageSize, environmentId: this.currentEnvId, searchText: this.searchValue })
      .pipe()
      .subscribe(
        res => {
          this.isLoading = false;
          this.list = res.users;
          this.totalCount = res.count;
        },
        err => {
          this.isLoading = false;
        }
      );
  }

  onSearchClick() {
    this.fetchUserList();
  }

  onQueryChange(params: NzTableQueryParams) {
    if (!this.currentEnvId) return;
    const { pageIndex, pageSize } = params;
    this.pageSize = pageSize;
    this.pageIndex = pageIndex - 1;
    this.fetchUserList();
  }

  onRowClick(user) {
    this.router.navigateByUrl(`/switch-user/${encodeURIComponent(user.id)}`)
  }

  onPropsSettingClick() {
    this.visible = true;
  }

  onClose() {
    this.visible = false;
  }
}
