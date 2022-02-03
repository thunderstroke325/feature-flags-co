import { Component, Input, Output, EventEmitter } from '@angular/core';
import { NzTreeNodeOptions } from "ng-zorro-antd/core/tree/nz-tree-base-node";
import { SwitchTagTree } from "../types/switch-index";

@Component({
  selector: 'switch-tag-tree-select',
  template: `
    <nz-tree-select
      style="width: 100%;"
      [(ngModel)]="selectedTagIds"
      [nzNodes]="options"
      nzShowSearch
      nzCheckable
      nzAllowClear
      (ngModelChange)="onSelect.emit(selectedTagIds)"
      nzPlaceHolder="请选择标签"
    ></nz-tree-select>
  `,
  styles: [`
    :host {
      flex: 1;
    }
  `]
})
export class SwitchTagTreeSelectComponent {
  options: NzTreeNodeOptions[] = [];
  selectedTagIds: number[];

  @Input()
  set tagTree(tree: SwitchTagTree) {
    if (tree) {
      this.options = tree.toTreeSelectNodes();
    }
  }

  @Output()
  onSelect = new EventEmitter<number[]>();
}
