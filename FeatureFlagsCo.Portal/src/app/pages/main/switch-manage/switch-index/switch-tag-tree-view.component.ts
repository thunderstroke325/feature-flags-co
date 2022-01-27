import { Component, OnInit } from '@angular/core';
import { TransferChange, TransferItem, TransferSelectChange } from "ng-zorro-antd/transfer";
import { SelectionModel } from '@angular/cdk/collections';
import { FlatTreeControl } from '@angular/cdk/tree';
import { NzTreeFlatDataSource, NzTreeFlattener } from 'ng-zorro-antd/tree-view';
import { NzMessageService } from "ng-zorro-antd/message";

interface TreeNode {
  name: string;
  key: string;
  children?: TreeNode[];
  isEditing?: boolean;
}

let TREE_DATA: TreeNode[] = [
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
  name: string;
  key: string;
  level: number;
  isEditing?: boolean;
}

@Component({
  selector: 'switch-tag-tree-view',
  template: `
    <div nz-row>
      <div nz-col nzSpan="8">
        <nz-tree-view [nzTreeControl]="treeControl" [nzDataSource]="dataSource" [trackBy]="trackBy">
          <nz-tree-node nzTreeNodePadding *nzTreeNodeDef="let node">
            <nz-tree-node-option
              [nzDisabled]="node.disabled"
              [nzSelected]="selectListSelection.isSelected(node)"
              (nzClick)="selectListSelection.toggle(node)">

              <!-- node name -->
              <div *ngIf="node.isEditing; then editMode else showMode"></div>
              <ng-template #editMode>
                <input nz-input nzSize="small" [(ngModel)]="node.name" style="width: 120px; margin-right: 2px">
              </ng-template>
              <ng-template #showMode>
                <span>{{node.name}}</span>
              </ng-template>
            </nz-tree-node-option>

            <!-- node operations -->
            <div *ngIf="node.isEditing; then saveOperations else editOperations"></div>
            <!-- save operations -->
            <ng-template #saveOperations>
              <button nz-button nzType="text" nzSize="small"
                      nz-tooltip nzTooltipTitle="保存"
                      (click)="saveNode(node)">
                <i nz-icon nzType="save" nzTheme="outline"></i>
              </button>
            </ng-template>
            <!-- edit operations -->
            <ng-template #editOperations>
              <!-- create -->
              <button nz-button nzType="text" nzSize="small"
                      nz-tooltip nzTooltipTitle="创建节点"
                      (click)="newNode(node)">
                <i nz-icon nzType="plus" nzTheme="outline"></i>
              </button>

              <!-- update -->
              <button nz-button nzType="text" nzSize="small"
                      nz-tooltip nzTooltipTitle="修改名称"
                      (click)="editNode(node)">
                <i nz-icon nzType="edit" nzTheme="outline"></i>
              </button>
            </ng-template>

            <!-- caret down -->
            <nz-tree-node-toggle *ngIf="hasChild(node)">
              <i nz-icon nzType="down" nzTreeNodeToggleRotateIcon></i>
            </nz-tree-node-toggle>

            <!-- delete -->
            <button nz-button nzType="text" nzSize="small" nzDanger
                    nz-tooltip nzTooltipTitle="删除节点"
                    (click)="delete(node)">
              <i nz-icon nzType="delete" nzTheme="outline"></i>
            </button>
          </nz-tree-node>
        </nz-tree-view>
      </div>
      <div nz-col nzSpan="16">
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

  constructor(private message: NzMessageService) {

  }

  ngOnInit(): void {
    // init tree
    this.treeData = TREE_DATA;
    this.dataSource.setData(this.treeData);
    this.treeControl.expandAll();

    // init transfer
    for (let i = 0; i < 20; i++) {
      this.list.push({
        key: i.toString(),
        title: `feature flag ${i + 1}`,
        checked: false
      });
    }

    [2, 3].forEach(idx => (this.list[idx].direction = 'right'));
  }

  //#region transfer

  list: TransferItem[] = [];

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
          key: node.key,
          isEditing: node.isEditing
        };
    flatNode.name = node.name;
    this.flatNodeMap.set(flatNode, node);
    this.nestedNodeMap.set(node, flatNode);
    return flatNode;
  };

  treeData: TreeNode[] = [];
  flatNodeMap = new Map<FlatNode, TreeNode>();
  nestedNodeMap = new Map<TreeNode, FlatNode>();
  selectListSelection = new SelectionModel<FlatNode>(false);

  treeControl = new FlatTreeControl<FlatNode>(
    node => node.level,
    _ => true
  );
  treeFlattener = new NzTreeFlattener(
    this.transformer,
    node => node.level,
    _ => true,
    node => node.children
  );

  dataSource = new NzTreeFlatDataSource(this.treeControl, this.treeFlattener);
  trackBy = (_: number, node: FlatNode): string => `${node.key}-${node.name}`;

  hasChild(node: FlatNode) {
    const nestedNode = this.flatNodeMap.get(node);
    if (nestedNode) {
      return !!nestedNode.children && nestedNode.children.length > 0;
    }

    return false;
  }

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
      // has parent node
      parentNode.children = parentNode.children.filter(e => e !== originNode);
    } else {
      // no parent node
      if (this.treeData.length === 1) {
        this.message.warning('标签树需要至少保留一个节点');
        return;
      }

      this.treeData = this.treeData.filter(e => e.key !== originNode.key);
      TREE_DATA = TREE_DATA.filter(e => e.key !== originNode.key);
    }

    this.dataSource.setData(this.treeData);
  }

  newNode(node: FlatNode): void {
    const nestedNode = this.flatNodeMap.get(node);
    if (nestedNode) {
      nestedNode.children = nestedNode.children || [];
      nestedNode.children.push({
        name: '',
        key: `${nestedNode.key}-${nestedNode.children.length}`,
        isEditing: true
      });
      this.dataSource.setData(this.treeData);
      this.treeControl.expand(node);
    }
  }

  editNode(node: FlatNode): void {
    node.isEditing = true;
  }

  saveNode(node: FlatNode): void {
    if (node.name.trim() === '') {
      this.message.warning('节点名称不能为空');
      return;
    }

    node.isEditing = false;

    const nestedNode = this.flatNodeMap.get(node);
    if (nestedNode) {
      nestedNode.isEditing = false;
      nestedNode.name = node.name;
      this.dataSource.setData(this.treeData);
    }
  }

  //#endregion
}
