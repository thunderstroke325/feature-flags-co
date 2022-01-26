import { Component, OnInit } from '@angular/core';
import { TransferChange, TransferItem, TransferSelectChange } from "ng-zorro-antd/transfer";
import { SelectionModel } from '@angular/cdk/collections';
import { FlatTreeControl } from '@angular/cdk/tree';
import { NzTreeFlatDataSource, NzTreeFlattener } from 'ng-zorro-antd/tree-view';

interface TreeNode {
  name: string;
  key: string;
  children?: TreeNode[];
}

const TREE_DATA: TreeNode[] = [
  {
    name: 'parent 1',
    key: '1',
    children: [
      {
        name: 'parent 1-0',
        key: '1-0',
        children: [
          {name: 'leaf', key: '1-0-0'},
          {name: 'leaf', key: '1-0-1'}
        ]
      },
      {
        name: 'parent 1-1',
        key: '1-1',
        children: [{name: 'leaf', key: '1-1-0'}]
      }
    ]
  },
  {
    key: '2',
    name: 'parent 2',
    children: [{name: 'leaf', key: '2-0'}]
  }
];

interface FlatNode {
  expandable: boolean;
  name: string;
  key: string;
  level: number;
}

@Component({
  selector: 'switch-tag-tree-view',
  template: `
    <div nz-row>
      <div nz-col nzSpan="7">
        <nz-tree-view [nzTreeControl]="treeControl" [nzDataSource]="dataSource" [trackBy]="trackBy">
          <nz-tree-node *nzTreeNodeDef="let node" nzTreeNodeIndentLine>
            <nz-tree-node-option
              [nzDisabled]="node.disabled"
              [nzSelected]="selectListSelection.isSelected(node)"
              (nzClick)="selectListSelection.toggle(node)"
            >
              {{ node.name }}
            </nz-tree-node-option>
            <button nz-button nzType="text" nzSize="small" (click)="delete(node)">
              <i nz-icon nzType="minus" nzTheme="outline"></i>
            </button>
          </nz-tree-node>

          <nz-tree-node *nzTreeNodeDef="let node; when: hasNoContent" nzTreeNodeIndentLine>
            <input nz-input placeholder="Input node name" nzSize="small" #inputElement/>
            &nbsp;
            <button nz-button nzSize="small" (click)="saveNode(node, inputElement.value)">Add</button>
          </nz-tree-node>

          <nz-tree-node *nzTreeNodeDef="let node; when: hasChild" nzTreeNodeIndentLine>
            <nz-tree-node-toggle>
              <i nz-icon nzType="caret-down" nzTreeNodeToggleRotateIcon></i>
            </nz-tree-node-toggle>
            {{ node.name }}
            <button nz-button nzType="text" nzSize="small" (click)="addNewNode(node)">
              <i nz-icon nzType="plus" nzTheme="outline"></i>
            </button>
          </nz-tree-node>
        </nz-tree-view>
      </div>
      <div nz-col nzSpan="17">
        <nz-transfer
          [nzDataSource]="list"
          [nzShowSearch]="true"
          [nzTitles]="['未选择', '已选择']"
          nzSearchPlaceholder="按名称查找"
          [nzListStyle]="{ 'width.px': 250, 'height.px': 350 }"
          [nzRender]="render"
          (nzSelectChange)="select($event)"
          (nzChange)="change($event)"
        >
          <ng-template #render let-item>{{ item.title }}</ng-template>
        </nz-transfer>
      </div>
    </div>
  `,
  styles: []
})
export class SwitchTagTreeViewComponent implements OnInit {
  //#region transfer

  list: TransferItem[] = [];

  ngOnInit(): void {
    for (let i = 0; i < 20; i++) {
      this.list.push({
        key: i.toString(),
        title: `feature flag ${i + 1}`,
        checked: false
      });
    }

    [2, 3].forEach(idx => (this.list[idx].direction = 'right'));
  }

  select(ret: TransferSelectChange): void {
    console.log('nzSelectChange', ret);
  }

  change(ret: TransferChange): void {
    console.log('nzChange', ret);
    const listKeys = ret.list.map(l => l.key);
    const hasOwnKey = (e: TransferItem): boolean => e.hasOwnProperty('key');
    this.list = this.list.map(e => {
      if (listKeys.includes(e.key) && hasOwnKey(e)) {
        if (ret.to === 'left') {
          delete e.hide;
        } else if (ret.to === 'right') {
          e.hide = false;
        }
      }
      return e;
    });
  }

  //#endregion

  //#region tree view

  private transformer = (node: TreeNode, level: number): FlatNode => {
    const existingNode = this.nestedNodeMap.get(node);
    const flatNode =
      existingNode && existingNode.key === node.key
        ? existingNode
        : {
          expandable: !!node.children && node.children.length > 0,
          name: node.name,
          level,
          key: node.key
        };
    flatNode.name = node.name;
    this.flatNodeMap.set(flatNode, node);
    this.nestedNodeMap.set(node, flatNode);
    return flatNode;
  };

  treeData = TREE_DATA;
  flatNodeMap = new Map<FlatNode, TreeNode>();
  nestedNodeMap = new Map<TreeNode, FlatNode>();
  selectListSelection = new SelectionModel<FlatNode>(true);

  treeControl = new FlatTreeControl<FlatNode>(
    node => node.level,
    node => node.expandable
  );
  treeFlattener = new NzTreeFlattener(
    this.transformer,
    node => node.level,
    node => node.expandable,
    node => node.children
  );

  dataSource = new NzTreeFlatDataSource(this.treeControl, this.treeFlattener);

  constructor() {
    this.dataSource.setData(this.treeData);
    this.treeControl.expandAll();
  }

  hasChild = (_: number, node: FlatNode): boolean => node.expandable;
  hasNoContent = (_: number, node: FlatNode): boolean => node.name === '';
  trackBy = (_: number, node: FlatNode): string => `${node.key}-${node.name}`;

  delete(node: FlatNode): void {
    const originNode = this.flatNodeMap.get(node);

    const dfsParentNode = (): TreeNode | null => {
      const stack = [...this.treeData];
      while (stack.length > 0) {
        const n = stack.pop()!;
        if (n.children) {
          if (n.children.find(e => e === originNode)) {
            return n;
          }

          for (let i = n.children.length - 1; i >= 0; i--) {
            stack.push(n.children[i]);
          }
        }
      }
      return null;
    };

    const parentNode = dfsParentNode();
    if (parentNode && parentNode.children) {
      parentNode.children = parentNode.children.filter(e => e !== originNode);
    }

    this.dataSource.setData(this.treeData);
  }

  addNewNode(node: FlatNode): void {
    const parentNode = this.flatNodeMap.get(node);
    if (parentNode) {
      parentNode.children = parentNode.children || [];
      parentNode.children.push({
        name: '',
        key: `${parentNode.key}-${parentNode.children.length}`
      });
      this.dataSource.setData(this.treeData);
      this.treeControl.expand(node);
    }
  }

  saveNode(node: FlatNode, value: string): void {
    const nestedNode = this.flatNodeMap.get(node);
    if (nestedNode) {
      nestedNode.name = value;
      this.dataSource.setData(this.treeData);
    }
  }

  //#endregion
}
