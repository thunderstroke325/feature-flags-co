import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Subject } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { IJsonContent } from '../../../types/switch-new';
import { ruleType, ruleValueConfig, findIndex } from '../ruleConfig';

@Component({
  selector: 'app-rule',
  templateUrl: './rule.component.html',
  styleUrls: ['./rule.component.less']
})
export class RuleComponent {

  private inputs = new Subject<any>();

  @Input() isFirst: boolean;
  @Input() isLast: boolean;
  @Input() ruleContent: IJsonContent;
  @Input() properties: string[];

  @Input() selectValueList: string[] = [];

  @Output() addRule = new EventEmitter();                           // 添加条件
  @Output() deleteRule = new EventEmitter();                        // 删除条件
  @Output() ruleChange = new EventEmitter<IJsonContent>();       // 刷新数据

  public ruleValueConfig: ruleType[] = [];

  listOfTagOptions = [];

  constructor() {

    this.ruleValueConfig = ruleValueConfig;

    this.inputs.pipe(
      debounceTime(500)
    ).subscribe(e => {
      this.ruleChange.next(e);
    })
  }

  onSelectOption() {
    let result = findIndex(this.ruleContent.operation);
    this.ruleContent.type = this.ruleValueConfig[result].type;
    this.ruleContent.value = this.ruleValueConfig[result].default;

    this.ruleChange.next(this.ruleContent);
  }

  // 数据改变，触发数据刷新
  public onModelChange() {
    this.ruleChange.next(this.ruleContent);
  }

  // 需要节流的数据
  public onDebounceTimeModelChange() {
    this.inputs.next(this.ruleContent);
  }
}
