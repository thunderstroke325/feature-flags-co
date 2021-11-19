import { Component, Input, Output, EventEmitter } from '@angular/core';
import { NzMessageService } from 'ng-zorro-antd/message';
import { ruleType, ruleValueConfig } from 'src/app/pages/main/switch-manage/components/find-rule/ruleConfig';
import { CSwitchParams, IRulePercentageRollout } from 'src/app/pages/main/switch-manage/types/switch-new';
import { SwitchService } from 'src/app/services/switch.service';
import { isNotPercentageRollout, isSingleOperator } from 'src/app/utils';

@Component({
  selector: 'app-expt-rules-drawer',
  templateUrl: './expt-rules-drawer.component.html',
  styleUrls: ['./expt-rules-drawer.component.less']
})
export class ExptRulesDrawerComponent {

  @Input() currentAccountId: number;
  @Input() visible: boolean = false;
  @Output() close: EventEmitter<any> = new EventEmitter();

  private _ff: any = null; // CSwitchParams

  // 实验分流类型
  experimentRolloutType: 'default' | 'recommended' = 'default';

  includeAllRules = true;
  customRules = false;

  get featureFlag() {
    return this._ff;
  }

  @Input()
  set featureFlag(ff: any) {
    if (ff) {
      const data = Object.assign({}, ff, {
        fftuwmtr: ff.fftuwmtr.map(f => {
          const result = {
            ruleJsonContent: f.ruleJsonContent.map(item => {
              let result: ruleType = ruleValueConfig.filter((rule: ruleType) => rule.value === item.operation)[0];

              let multipleValue: string[] = [];

              if(result.type === 'multi' && item.multipleValue === undefined) {
                multipleValue = JSON.parse(item.value);
              }

              return Object.assign({ multipleValue: multipleValue, type: result.type, isSingleOperator: isSingleOperator(result.type)}, item);
            }),
            isIncludedInExpt: !!f.isIncludedInExpt,
            isNotPercentageRollout: isNotPercentageRollout(f.valueOptionsVariationRuleValues),
            valueOptionsVariationRuleValues: f.valueOptionsVariationRuleValues.map(item => Object.assign({}, item, {
              percentage: (parseFloat((item.rolloutPercentage[1] - item.rolloutPercentage[0]).toFixed(2))) * 100
            }))
          };

          this.initExperimentRollout(result.valueOptionsVariationRuleValues);
          return Object.assign({}, f, result);
        }),
        ff: Object.assign({}, ff.ff, {
          isDefaultRulePercentageRolloutsIncludedInExpt: !!ff.ff.isDefaultRulePercentageRolloutsIncludedInExpt,
          defaultRuleIsNotPercentageRollout: isNotPercentageRollout(ff.ff.defaultRulePercentageRollouts),
          defaultRulePercentageRollouts: ff.ff.defaultRulePercentageRollouts.map(item => Object.assign({}, item, {
            percentage: (parseFloat((item.rolloutPercentage[1] - item.rolloutPercentage[0]).toFixed(2))) * 100
          }))
        })
      });

      this.initExperimentRollout(data.ff.defaultRulePercentageRollouts);
      this._ff = new CSwitchParams(data);
    }
  }

  // 初始化实验分类比例
  private initExperimentRollout(rules: IRulePercentageRollout[]) {
    const self = this;
    rules.forEach(rule => {
      // 该开关未设置过实验分流比例 则默认其实验分流比例与请求分流比例相同
      if (!rule.exptRollout) {
        self.setDefaultExperimentRollout(rule);
      }
      // 该开关已经设置过实验分流比例
      else {
        rule.exptPercentage = rule.exptRollout * 100;
      }
    });
  }

  // 使用默认的实验分流比例
  useDefaultExperimentRollout() {
    // 条件规则
    const conditions = this.featureFlag.fftuwmtr;
    conditions.forEach(condition =>
      condition.valueOptionsVariationRuleValues.forEach(rule => {
        this.setDefaultExperimentRollout(rule);
      })
    );

    // 默认值
    const ffBasic = this.featureFlag.ff;
    ffBasic.defaultRulePercentageRollouts.forEach(rule => {
      this.setDefaultExperimentRollout(rule);
    });

    this.experimentRolloutType = 'default';
  }

  // 选择要使用的实验分流比例类型
  toggleExperimentRolloutType() {
    this.experimentRolloutType === 'default' ?
      this.useRecommendedExperimentRollout() :
      this.useDefaultExperimentRollout();
  }

  // 使用推荐的实验分流比例
  useRecommendedExperimentRollout() {
    // 条件规则
    const conditions = this.featureFlag.fftuwmtr;
    conditions.forEach(condition =>
      this.setRecommendedExperimentRollout(condition.valueOptionsVariationRuleValues, condition.isNotPercentageRollout)
    );

    // 默认值
    const ffBasic = this.featureFlag.ff;
    this.setRecommendedExperimentRollout(ffBasic.defaultRulePercentageRollouts, ffBasic.defaultRuleIsNotPercentageRollout);

    this.experimentRolloutType = 'recommended';
  }

  // 使用默认的实验分流比例 (保持和请求分流比例一致)
  private setDefaultExperimentRollout(rule: IRulePercentageRollout) {
    rule.exptPercentage = rule.percentage;
    rule.exptRollout = rule.percentage / 100;
  }

  // 根据请求分流比例计算最合适的实验分流比例
  private setRecommendedExperimentRollout(rules: IRulePercentageRollout[], isNotPercentageRollout: boolean) {
    if (isNotPercentageRollout) {
      rules.forEach(rule => {
        rule.exptPercentage = rule.percentage;
        rule.exptRollout = rule.percentage / 100;
      });
      return;
    }

    const has100Percentage = rules.find(rule => rule.percentage == 100);
    if (has100Percentage) {
      rules.forEach(rule => {
        rule.exptPercentage = 0;
        rule.exptRollout = 0;
      });
      return;
    }

    const minPercentage = Math.min(...rules.map(r => r.percentage)) || 0;
    rules.forEach(rule => {
      rule.exptPercentage = minPercentage;
      rule.exptRollout = minPercentage / 100;
    });
  }

  exptPercentageChange(ruleValue: IRulePercentageRollout) {
    if (ruleValue) {
      ruleValue.exptRollout = ruleValue.exptPercentage / 100;
    }
  }

  constructor(
    private switchServe: SwitchService,
    private message: NzMessageService
  ) {
  }

  onClose() {
    this.close.emit({ isSaved: false, data: this.featureFlag });
  }

  doSubmit() {
    this.switchServe.updateSwitch(this.featureFlag)
    .subscribe((result) => {
      this.message.success("保存成功!");
      this.close.emit({ isSaved: true, data: this.featureFlag });
    }, error => {
      this.message.error("保存失败!");
      this.close.emit({ isSaved: false, data: this.featureFlag });
    })
  }
}
