import { Component } from '@angular/core';

@Component({
  selector: 'switch-tag-tree-select',
  template: `
    <nz-tree-select
      style="width: 100%;"
      [(ngModel)]="value"
      [nzNodes]="nodes"
      (ngModelChange)="onChange($event)"
      nzShowSearch
      nzCheckable
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
  value: string[];
  nodes = [
    {
      title: 'Node1',
      value: '0-0',
      key: '0-0',
      children: [
        {
          title: 'Child Node1',
          value: '0-0-0',
          key: '0-0-0',
          isLeaf: true
        }
      ]
    },
    {
      title: 'Node2',
      value: '0-1',
      key: '0-1',
      children: [
        {
          title: 'Child Node3',
          value: '0-1-0',
          key: '0-1-0',
          isLeaf: true
        },
        {
          title: 'Child Node4',
          value: '0-1-1',
          key: '0-1-1',
          isLeaf: true
        },
        {
          title: 'Child Node5',
          value: '0-1-2',
          key: '0-1-2',
          isLeaf: true
        }
      ]
    }
  ];

  onChange($event: string[]): void {
    console.log($event);
  }
}
