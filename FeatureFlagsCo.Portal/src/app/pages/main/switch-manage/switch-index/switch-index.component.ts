import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NzMessageService } from 'ng-zorro-antd/message';
import { Subject } from 'rxjs';
import { SwitchService } from 'src/app/services/switch.service';
import { FeatureFlagType, IFfParams } from '../types/switch-new';
import { AccountService } from 'src/app/services/account.service';
import { encodeURIComponentFfc } from 'src/app/utils';
import { FfcService } from "../../../../services/ffc.service";

@Component({
  selector: 'index',
  templateUrl: './switch-index.component.html',
  styleUrls: ['./switch-index.component.less']
})
export class SwitchIndexComponent implements OnInit, OnDestroy {

  // the switch index version
  private _switchIndexVersion: 'v1' | 'v2';
  get switchIndexVersion(): string {
    if (!this._switchIndexVersion) {
      const version = this.ffcService.variation('switch-index-version', 'v1');
      this._switchIndexVersion = version === 'v2' ? 'v2' : 'v1';
    }

    return this._switchIndexVersion;
  }

  private destory$: Subject<void> = new Subject();
  private currentAccountId: number;

  nameSearchValue: string = '';
  showType: '' | 'Enabled' | 'Disabled' = '';
  isLoading: boolean = true;
  pageIndex: number = 0;
  pageSize: number = 10;

  public switchLists: IFfParams[] = [];
  public initSwitchLists: IFfParams[] = [];
  public switchListsShowData: IFfParams[] = [];
  public createModalVisible: boolean = false;             // 创建开关的弹窗显示
  public isOkLoading: boolean = false;                    // 创建开关加载中动画
  public isInitLoading: boolean = true;                  // 数据加载中对话

  public switchName: string = '';
  switchType: FeatureFlagType = FeatureFlagType.Classic;

  ClassicFeatureFlag: FeatureFlagType = FeatureFlagType.Classic;
  public isIntoing: boolean = false;                      // 是否点击了一条开关，防止路由切换慢的双击效果
  public totalCount: number = 0;

  // tag tree modal
  tagTreeModalVisible: boolean = false;

  constructor(
    private router: Router,
    public switchServe: SwitchService,
    private msg: NzMessageService,
    private accountService: AccountService,
    private ffcService: FfcService
  ) {}

  ngOnInit(): void {
    const currentAccountProjectEnv = this.accountService.getCurrentAccountProjectEnv();
    this.currentAccountId = currentAccountProjectEnv.account.id;
    const envId = currentAccountProjectEnv.projectEnv.envId;
    this.switchServe.envId = envId;
    this.initSwitchList(envId);
  }

  private initSwitchList(id: number): void{
    this.isInitLoading = true;
    this.switchServe.getSwitchList(id).subscribe((result: IFfParams[]) => {
        this.isInitLoading = false;
        if (result.length) {
          this.initSwitchLists = result;
          this.switchLists = result;
          this.totalCount = result.length;
          this.switchListsShowData = result.slice(this.pageIndex * this.pageSize, this.pageIndex * this.pageSize + this.pageSize);
        } else {
          // this.msg.info("当前 Project 没有开关，请添加!");
          // this.initSwitchLists = [];
          // this.createModalVisible = true;
        }
        this.isLoading = false;
    });
  }

  // 添加开关
  addSwitch() {
    this.createModalVisible = true;
  }

  // 切换开关状态
  onChangeSwitchStatus(data: IFfParams): void {
    if (data.status === 'Enabled' ){
      this.switchServe.changeSwitchStatus(data.id, 'Disabled')
        .subscribe(_ => {
          this.msg.success('开关状态已切换至 Disabled!');
          data.status = 'Disabled';
        }, _ => {
          this.msg.error('开关状态切换失败!');
        });
    }else if (data.status === 'Disabled'){
      this.switchServe.changeSwitchStatus(data.id, 'Enabled')
        .subscribe(_ => {
          this.msg.success('开关状态已切换至 Enabled!');
          data.status = 'Enabled';
        }, _ => {
          this.msg.error('开关状态切换失败!');
        });
    }
  }

  ngOnDestroy(): void {
    this.destory$.next();
    this.destory$.complete();
  }

  // 关闭弹窗
  public handleCancel() {
    this.createModalVisible = false;
  }

  public handleOk() {
    if(!this.switchName.length) {
      this.msg.error("请输入开关名字!");
      return;
    }
    this.isOkLoading = true;

    this.switchServe.createNewSwitch(this.switchName, this.switchType)
      .subscribe((result: IFfParams) => {
        this.switchServe.setCurrentSwitch(result);
        this.toRouter(result.id);
        this.isOkLoading = false;
      }, errResponse => {
        this.msg.error(errResponse.error);
        this.isOkLoading = false;
      });
  }

  // 点击进入对应开关详情
  public onIntoSwitchDetail(data: IFfParams) {
    if(this.isIntoing) return;
    this.isIntoing = true;
    this.switchServe.setCurrentSwitch(data);
    this.toRouter(data.id);
  }

  // 路由跳转
  private toRouter(id: string) {
    this.router.navigateByUrl("/switch-manage/condition/" + encodeURIComponentFfc(id));
  }

  // 转换本地时间
  getLocalDate(date: string) {
    if (!date) return '';
    return new Date(date);
  }

  // 分页
  onChangePageIndex(params: number): void {
    this.pageIndex = params - 1;
    this.switchListsShowData = this.switchLists.slice( this.pageIndex  * this.pageSize, this.pageIndex * this.pageSize + this.pageSize);
  }
  // 根据开关筛选
  onSearchByOnOff(params: string): void{
    if (params === ''){
      this.switchLists = this.initSwitchLists;
      this.resetSwitchListsShowData();
    }else {
      this.switchLists = this.initSwitchLists.filter(e => e.status === params);
      this.resetSwitchListsShowData();
    }
  }
  // 根据 name 筛选
  onSearchByName(): void{
    this.switchLists = this.initSwitchLists.filter(e => e.name.includes(this.nameSearchValue));
    this.resetSwitchListsShowData();
  }
  resetSwitchListsShowData(): void{
    this.totalCount = this.switchLists.length;
    this.pageIndex = 0;
    this.switchListsShowData = this.switchLists
      .slice( this.pageIndex  * this.pageSize, this.pageIndex * this.pageSize + this.pageSize);
  }
}
