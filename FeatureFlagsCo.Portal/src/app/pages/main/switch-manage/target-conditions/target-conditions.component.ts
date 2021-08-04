import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NzMessageService } from 'ng-zorro-antd/message';
import { forkJoin, Subject } from 'rxjs';
import { debounceTime, map, takeUntil } from 'rxjs/operators';
import { SwitchService } from 'src/app/services/switch.service';
import { CSwitchParams, IFfParams, IFfpParams, IJsonContent, IUserType } from '../types/switch-new';
import { ProjectService } from 'src/app/services/project.service';
import { AccountService } from 'src/app/services/account.service';
import { IAccount, IProjectEnv } from 'src/app/config/types';

@Component({
  selector: 'conditions',
  templateUrl: './target-conditions.component.html',
  styleUrls: ['./target-conditions.component.less']
})
export class TargetConditionsComponent implements OnInit {

  private destory$: Subject<void> = new Subject();

  public switchStatus: 'Enabled' | 'Disabled' = 'Enabled';  // 开关状态
  public propertiesList: string[] = [];                     // 用户配置列表
  public featureList: IFfParams[] = [];                     // 开关列表
  public featureDetail: CSwitchParams;                      // 开关详情
  public upperFeatures: IFfpParams[] = [];                  // 上游开关列表
  public userList: IUserType[] = [];                        // 用户列表
  public targetUserSelectedListTrue: IUserType[] = [];       // 状态为 true 的目标用户
  public targetUserSelectedListFalse: IUserType[] = [];      // 状态为 false 的目标用户
  public switchId: string;
  public isLoading: boolean = true;                          // 加载数据

  constructor(
    private route:ActivatedRoute,
    private switchServe: SwitchService,
    private msg: NzMessageService,
  ) {
    this.ListenerResolveData();
  }

  ngOnInit(): void {
    if(this.switchServe.envId) {
      this.initData();
    }
  }

  private initData() {
    this.isLoading = true;
    forkJoin([
      this.switchServe.getEnvUserProperties(),
      this.switchServe.getSwitchList(this.switchServe.envId)
    ]).subscribe((result) => {
      if(result) {
        this.propertiesList = result[0];
        this.featureList = result[1];

        this.initSwitchStatus();
        this.initUpperSwitch();
        this.initTargetUserListForTrue();
        this.initTargetUserListForFalse();

        this.onSearchUser();

        this.switchServe.setCurrentSwitch( this.featureDetail.getSwicthDetail());
        this.isLoading = false;
      }
    }, _ => {
      this.msg.error("数据加载失败，请重试!");
      this.isLoading = false;
    })
  }

  private ListenerResolveData() {
    this.route.data.pipe(map(res => res.switchInfo))
    .subscribe((result: CSwitchParams) => {
      this.featureDetail = new CSwitchParams(result);
      const detail: IFfParams = this.featureDetail.getSwicthDetail();
      this.switchServe.setCurrentSwitch(detail);
      this.switchId = detail.id;
    })
  }

  // -------------------------------------------------------------------------------------------------

  // 初始化开关状态
  private initSwitchStatus() {
    this.switchStatus = this.featureDetail.getFeatureStatus();
  }

  // 切换开关状态
  public onChangeSwitchStatus(type: 'Enabled' | 'Disabled') {
    this.switchStatus = type;
    this.featureDetail.setFeatureStatus(type);
  }

  // 初始化上游开关
  private initUpperSwitch() {
    this.upperFeatures = [...this.featureDetail.getUpperFeatures()];
  }

  // 上游开关发生改变
  public onUpperSwicthChange(data: IFfpParams[]) {
    this.upperFeatures = [...data];
    this.featureDetail.setUpperFeatures(this.upperFeatures);
  }

  // 初始化目标用户，状态为 true
  private initTargetUserListForTrue() {
    this.targetUserSelectedListTrue = [...this.featureDetail.getTargetUsers("true") as IUserType[]];
  }

  // 初始化目标用户，状态为 false
  private initTargetUserListForFalse() {
    this.targetUserSelectedListFalse = [...this.featureDetail.getTargetUsers("false") as IUserType[]];
  }

  // 目标用户发生改变
  public onSelectedUserListChange(data: IUserType[], type: 'true' | 'false') {
    if(type === 'true') {
      this.targetUserSelectedListTrue = [...data];
    } else {
      this.targetUserSelectedListFalse = [...data];
    }
  }

  // 搜索用户
  public onSearchUser(value: string = '') {
    this.switchServe.queryUsers(value)
      .subscribe((result) => {
        this.userList = [...result['users']];
      })
  }

  // 切换默认值
  public switchBaseProperty() {
    let baseProperty = this.featureDetail.getFFBasedProperty();
    if(!baseProperty) {
      baseProperty = true;
    } else {
      baseProperty = !baseProperty;
    }
    this.featureDetail.setFFBasedProperty(baseProperty);
  }

  // 默认返回值配置
  public onDefaultValuePercentageChange(value: { serve: boolean | string, F: number, T: number }) {
    this.featureDetail.setFFConfig(value);
  }

  // 删除规则
  public onDeleteCondition(index: number) {
    this.featureDetail.deleteFftuwmtr(index);
  }

  // 添加规则
  public onAddCondition() {
    this.featureDetail.addFftuwmtr();
  }

  // 规则的 serve 配置发生改变
  public onPercentageChange(value: { serve: boolean | string, F: number, T: number }, index: number) {
    this.featureDetail.setConditionServe(value, index);
  }

  // 规则字段发生改变
  public onRuleConfigChange(value: IJsonContent[], index: number) {
    this.featureDetail.setConditionConfig(value, index);
  }


  // 保存设置c
  public onSaveSetting() {

    this.featureDetail.setTargetUsers('true', this.targetUserSelectedListTrue);
    this.featureDetail.setTargetUsers('false', this.targetUserSelectedListFalse);

    this.featureDetail.onSortoutSubmitData();

    this.switchServe.updateSwitch(this.featureDetail)
      .subscribe((result) => {
        this.msg.success("修改成功!");
    }, error => {
      this.msg.error("修改失败!");
    })
  }
}
