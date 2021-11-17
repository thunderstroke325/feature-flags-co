import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NzMessageService } from 'ng-zorro-antd/message';
import { forkJoin } from 'rxjs';
import { map } from 'rxjs/operators';
import { SwitchService } from 'src/app/services/switch.service';
import { CSwitchParams, IFfParams, IFfpParams, IJsonContent, IUserType, IVariationOption, IFftiuParams, IRulePercentageRollout, IPrequisiteFeatureFlag, FeatureFlagType } from '../types/switch-new';
import { FfcService } from 'src/app/services/ffc.service';
import { PendingChange } from '../types/pending-changes';
import { TeamService } from 'src/app/services/team.service';
import { IAccount, IProjectEnv } from 'src/app/config/types';
import { environment } from './../../../../../environments/environment';

@Component({
  selector: 'conditions',
  templateUrl: './target-conditions.component.html',
  styleUrls: ['./target-conditions.component.less']
})
export class TargetConditionsComponent implements OnInit {

  public switchStatus: 'Enabled' | 'Disabled' = 'Enabled';  // 开关状态
  public propertiesList: string[] = [];                     // 用户配置列表
  public featureList: IPrequisiteFeatureFlag[] = [];                     // 开关列表
  public featureDetail: CSwitchParams;                      // 开关详情
  public upperFeatures: IFfpParams[] = [];                  // 上游开关列表
  public userList: IUserType[] = [];                        // 用户列表
  public targetUserSelectedListTrue: IUserType[] = [];       // 状态为 true 的目标用户
  public targetUserSelectedListFalse: IUserType[] = [];      // 状态为 false 的目标用户
  public switchId: string;
  public isLoading: boolean = true;
  public variationOptions: IVariationOption[] = [];                         // multi state
  public targetIndividuals: {[key: string]: IUserType[]}  = {}; // multi state
  public targetIndividualsActive: boolean = false;

  currentAccount: IAccount = null;
  currentProjectEnv: IProjectEnv = null;

  approvalRequestEnabled: boolean = false;

  classicFeatureType: FeatureFlagType = FeatureFlagType.Classic;
  pretargetedFeatureType: FeatureFlagType = FeatureFlagType.Pretargeted;

  exptRulesVisible = false;

  onSetExptRulesClick() {
    this.exptRulesVisible = true;
  }

  onSetExptRulesClosed(data: any) {
    if (data.isSaved) {
      this.isLoading = true;
      this.switchServe.getSwitchDetail(this.featureDetail.getSwicthDetail().id).subscribe((result: CSwitchParams) => {
        this.loadFeatureFlag(result);
        this.isLoading = false;
      }, () => {
        this.isLoading = false;
      });
    }

    this.exptRulesVisible = false;
  }

  constructor(
    private route:ActivatedRoute,
    private switchServe: SwitchService,
    private msg: NzMessageService,
    private ffcService: FfcService,
    private teamService: TeamService
  ) {
    this.approvalRequestEnabled = environment.name === 'Standalone' ? false : this.ffcService.client.variation('approval-request') === 'true';
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
        this.pendingChanges.setFeatureFlagList(this.featureList);

        this.initSwitchStatus();
        this.initUpperSwitch();

        this.onSearchUser();
        this.onSearchPrequisiteFeatureFlags();

        this.switchServe.setCurrentSwitch( this.featureDetail.getSwicthDetail());
        this.isLoading = false;
      }
    }, _ => {
      this.msg.error("数据加载失败，请重试!");
      this.isLoading = false;
    })
  }

  private loadFeatureFlag(ff: CSwitchParams) {
    this.featureDetail = new CSwitchParams(ff);

    this.variationOptions = this.featureDetail.getVariationOptions();
    this.targetIndividuals = this.variationOptions.reduce((acc, cur) => {
      acc[cur.localId] = this.featureDetail.getTargetIndividuals().filter(t => t.valueOption !== null).find(ti => ti.valueOption.localId === cur.localId)?.individuals || [];
      return acc;
    }, {});
    for(const i in this.targetIndividuals){
      if (this.targetIndividuals[i].length !== 0 ){
        this.targetIndividualsActive = true;
        break;
      }
    }
    if (this.featureDetail.getFFDefaultRulePercentageRollouts().length === 0) {
      this.featureDetail.setFFDefaultRulePercentageRollouts([
        {
          rolloutPercentage: [0, 1],
          valueOption: this.variationOptions[0]
        }
      ]);
    }

    const detail: IFfParams = this.featureDetail.getSwicthDetail();
    this.switchServe.setCurrentSwitch(detail);
    this.switchId = detail.id;

    this.currentProjectEnv = JSON.parse(localStorage.getItem('current-project'));
    this.currentAccount = JSON.parse(localStorage.getItem('current-account'));
    const currentUrl = this.route.snapshot['_routerState'].url;
    this.pendingChanges = new PendingChange(
      this.teamService,
      this.currentAccount.id,
      this.currentProjectEnv,
      detail,
      this.variationOptions,
      currentUrl.substr(0, currentUrl.lastIndexOf('/') + 1)
      );

    this.pendingChanges.initialize(
      this.targetIndividuals,
      this.featureDetail.getFFVariationOptionWhenDisabled(),
      this.featureDetail.getFFDefaultRulePercentageRollouts(),
      this.featureDetail.getFftuwmtr(),
      this.featureDetail.getUpperFeatures(),
      this.featureDetail.getFeatureStatus()
      );
  }

  private ListenerResolveData() {
    this.route.data.pipe(map(res => res.switchInfo))
    .subscribe((result: CSwitchParams) => {
      this.loadFeatureFlag(result);
    });
  }

  // -------------------------------------------------------------------------------------------------

  // 初始化开关状态
  private initSwitchStatus() {
    this.switchStatus = this.featureDetail.getFeatureStatus();
  }

  // 切换开关状态
  public onChangeSwitchStatus(type: 'Enabled' | 'Disabled') {
    this.switchStatus = type;
    if (type === 'Enabled'){
      this.featureDetail.setFeatureStatus('Disabled');
    } else if (type === 'Disabled'){
      this.featureDetail.setFeatureStatus('Enabled');
    }
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

    // 搜索用户
  public onSearchPrequisiteFeatureFlags(value: string = '') {
    this.switchServe.queryPrequisiteFeatureFlags(value)
      .subscribe((result: IPrequisiteFeatureFlag[]) => {
        this.featureList = [...result.filter(r => r.id !== this.featureDetail.getSwicthDetail().id)];
      })
  }


  public onVariationOptionWhenDisabledChange(option: IVariationOption) {
    this.featureDetail.setFFVariationOptionWhenDisabled(option);
  }

  // 删除规则
  public onDeleteRule(index: number) {
    this.featureDetail.deleteFftuwmtr(index);
  }

  // 添加规则
  public onAddRule() {
    this.featureDetail.addFftuwmtr();
  }

  // 规则字段发生改变
  public onRuleConfigChange(value: IJsonContent[], index: number) {
    this.featureDetail.setConditionConfig(value, index);
  }

  /****multi state* */

  public onMultistatesSelectedUserListChange(data: IUserType[], variationOptionId: number) {
    this.targetIndividuals[variationOptionId] = [...data];
  }


  // 默认返回值配置
  public onDefaultRulePercentageRolloutsChange(value: IRulePercentageRollout[]) {
    this.featureDetail.setFFDefaultRulePercentageRollouts(value);
  }

  public onPreSaveConditions() {
    const validationErrs = this.featureDetail.checkMultistatesPercentage();

    if (validationErrs.length > 0) {
      this.msg.error(validationErrs[0]); // TODO display all messages by multiple lines
      return false;
    }

    if (!this.sortoutSubmitData()) {
      return;
    }

    this.pendingChanges.generateInstructions(
      this.targetIndividuals,
      this.featureDetail.getFFVariationOptionWhenDisabled(),
      this.featureDetail.getFFDefaultRulePercentageRollouts(),
      this.featureDetail.getFftuwmtr(),
      this.featureDetail.getUpperFeatures(),
      this.featureDetail.getFeatureStatus()
    );

    this.isApprovalRequestModal = false;
    this.requestApprovalModalVisible = true;
  }

  public onSaveConditionsOld() {
    const validationErrs = this.featureDetail.checkMultistatesPercentage();

    if (validationErrs.length > 0) {
      this.msg.error(validationErrs[0]); // TODO display all messages by multiple lines
      return false;
    }

    if (!this.sortoutSubmitData()) {
      return;
    }

    this.onSaveConditions();
  }

  public onSaveConditions() {
    this.featureDetail.setTargetIndividuals(this.targetIndividuals);

    this.switchServe.updateSwitch(this.featureDetail)
      .subscribe((result) => {
        this.msg.success("修改成功!");

        const featureDetail = new CSwitchParams(result.data);
        const targetIndividuals = this.variationOptions.reduce((acc, cur) => {
          acc[cur.localId] = this.featureDetail.getTargetIndividuals().find(ti => ti.valueOption.localId === cur.localId)?.individuals || [];
          return acc;
        }, {});
        this.pendingChanges.initialize(
          targetIndividuals,
          featureDetail.getFFVariationOptionWhenDisabled(),
          featureDetail.getFFDefaultRulePercentageRollouts(),
          featureDetail.getFftuwmtr(),
          featureDetail.getUpperFeatures(),
          featureDetail.getFeatureStatus()
          );
        this.requestApprovalModalVisible = false;
    }, error => {
      this.msg.error("修改失败!");
      this.requestApprovalModalVisible = true;
    })
  }

  private sortoutSubmitData(): boolean{
    try {
      this.featureDetail.onSortoutSubmitData();
      return true;
    } catch(e) {
      this.msg.warning("请确保所填数据完整!");
      return false;
    }
  }

  public onPercentageChangeMultistates(value: IRulePercentageRollout[], index: number) {
    this.featureDetail.setRuleValueOptionsVariationRuleValues(value, index);
  }

  /***************************Request approval********************************/
  requestApprovalModalVisible: boolean = false;
  pendingChanges: PendingChange;
  isApprovalRequestModal = false;
  public onRequestApproval(){

    if (!this.sortoutSubmitData()) {
      return;
    }

    this.pendingChanges.generateInstructions(
      this.targetIndividuals,
      this.featureDetail.getFFVariationOptionWhenDisabled(),
      this.featureDetail.getFFDefaultRulePercentageRollouts(),
      this.featureDetail.getFftuwmtr(),
      this.featureDetail.getUpperFeatures(),
      this.featureDetail.getFeatureStatus()
    );
    this.requestApprovalModalVisible = true;
    this.isApprovalRequestModal = true;
  }
}
