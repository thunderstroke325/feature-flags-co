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
  
  includeAllRules = true;
  customRules = false;

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
    
          result.valueOptionsVariationRuleValues = this.setExptPercentage(result.valueOptionsVariationRuleValues, result.isNotPercentageRollout);
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

      data.ff.defaultRulePercentageRollouts = this.setExptPercentage(data.ff.defaultRulePercentageRollouts, data.ff.defaultRuleIsNotPercentageRollout);
      this._ff = new CSwitchParams(data);
    }
  }

  private setExptPercentage(ruleValues: IRulePercentageRollout[], isNotPercentageRollout: boolean): IRulePercentageRollout[] {
    const minPercentage = Math.min(...ruleValues.map(r => r.percentage)) || 0;
    const has100PercentageOption = ruleValues.find(r => r.percentage == 100);

    return ruleValues.map(r => {
      let exptPercentage = r.exptRollout;
      if (r.exptRollout === null || r.exptRollout === undefined) {
        if (isNotPercentageRollout) {
          exptPercentage = r.percentage;
        } else if (has100PercentageOption) {
          exptPercentage = r.percentage === 100 ? 100 : 0;
        } else {
          exptPercentage = minPercentage;
        }
      } else {
        exptPercentage = exptPercentage * 100;
      }

      return Object.assign({ exptRollout: exptPercentage / 100 }, r, { exptPercentage });
    });
  }

  exptPercentageChange(ruleValue: IRulePercentageRollout) {
    if (ruleValue) {
      ruleValue.exptRollout = ruleValue.exptPercentage / 100;
    }
  }

  get featureFlag() {
    return this._ff;
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
