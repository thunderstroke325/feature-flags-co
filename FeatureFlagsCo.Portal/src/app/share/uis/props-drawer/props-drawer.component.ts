import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { NzMessageService } from 'ng-zorro-antd/message';
import { map } from 'rxjs/operators';
import { EnvironmentUserService } from 'src/app/services/environment-user.service';

@Component({
  selector: 'app-props-drawer',
  templateUrl: './props-drawer.component.html',
  styleUrls: ['./props-drawer.component.less']
})
export class PropsDrawerComponent implements OnInit {

  private _visible: boolean = false;

  list: string[] = [];
  editVisible: boolean = false;
  propName: string = '';
  editIndex: number = undefined;
  loading: boolean = false;

  @Input()
  set visible(visible: boolean) {
    if (visible) {
      this.loading = true;
      this.envUserService.getUserProperties({ id: this.envId })
        .pipe()
        .subscribe(res => {
          this.list = res.properties;
          this.loading = false;
        });
    }
    this._visible = visible;
  }

  get visible() {
    return this._visible;
  }
  @Input() envId: number;
  @Output() close: EventEmitter<boolean> = new EventEmitter();


  constructor(
    private envUserService: EnvironmentUserService,
    private message: NzMessageService
  ) { }

  ngOnInit(): void {
  }

  onAddClick() {
    this.propName = '';
    this.editIndex = undefined;
    this.editVisible = true;
  }

  onEditClick(prop: string, index: number) {
    this.propName = prop;
    this.editIndex = index;
    this.editVisible = true;
  }

  onDeleteClick(index: number) {
    const _list = this.list.slice();
    _list.splice(index, 1);
    this.saveProperties(_list)
      .subscribe(
        _ => {
          this.message.success('删除成功！');
        }
      );
  }

  handleOk() {
    if (this.editIndex === undefined) {
      this.saveProperties([...this.list, this.propName])
        .subscribe(
          _ => {
            this.editVisible = false;
            this.message.success('添加成功！');
          }
        );
    } else {
      const _list = this.list.slice();
      _list.splice(this.editIndex, 1, this.propName);
      this.saveProperties(_list)
        .subscribe(
          _ => {
            this.editVisible = false;
            this.message.success('更改成功！');
          }
        );
    }
  }

  saveProperties(list: string[]) {
    return this.envUserService.postUserProperties({
      environmentId: this.envId,
      properties: list
    })
      .pipe(
        map(res => {
          this.list = list;
        })
      )
  }

}
