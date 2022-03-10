import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NzMessageService } from 'ng-zorro-antd/message';
import { Subject } from 'rxjs';
import { encodeURIComponentFfc } from 'src/app/utils';
import { SegmentListFilter, SegmentListItem, SegmentListModel } from "../types/segments-index";
import { SegmentService } from "../../../../services/segment.service";
import { debounceTime, first, map, switchMap } from 'rxjs/operators';
import { FormBuilder, FormControl, FormGroup, Validators } from "@angular/forms";

@Component({
  selector: 'segments-index',
  templateUrl: './segments-index.component.html',
  styleUrls: ['./segments-index.component.less']
})
export class SegmentsIndexComponent implements OnInit, OnDestroy {

  private destory$: Subject<void> = new Subject();

  public createModalVisible: boolean = false;             // 新建的弹窗显示
  public isOkLoading: boolean = false;                    // 新建加载中动画

  public segmentName: string = '';

  public isIntoing: boolean = false;                      // 是否点击了一个segment，防止路由切换慢的双击效果

  segmentListModel: SegmentListModel = {
    items: [],
    totalCount: 0
  };

  loading: boolean = true;

  loadSegmentList() {
    this.loading = true;
    this.segmentService
      .getSegmentList(this.segmentFilter)
      .subscribe((segments: SegmentListModel) => {
        this.segmentListModel = segments;
        this.loading = false;
      });
  }

  segmentFilter: SegmentListFilter = new SegmentListFilter();

  $search: Subject<void> = new Subject();

  onSearch() {
    this.$search.next();
  }

  subscribeSearch() {
    this.$search.pipe(
      debounceTime(400)
    ).subscribe(() => {
      this.loadSegmentList();
    });
  }

  segmentForm: FormGroup;

  segmentNameValidator = (control: FormControl) => {
    const name = control.value;
    if (!name) {
      return {error: true, required: true};
    }

    if (name.includes('__')) {
      return {error: true, invalid_character: true};
    }
  }

  segmentNameAsyncValidator = (control: FormControl) => control.valueChanges.pipe(
    debounceTime(300),
    switchMap(value => this.segmentService.isNameUsed(value as string)),
    map(isNameUsed => {
      switch (isNameUsed) {
        case true:
          return {error: true, duplicated: true};
        case undefined:
          return {error: true, unknown: true};
        default:
          return null;
      }
    }),
    first()
  );

  creating: boolean = false;

  createSegment() {
    this.creating = true;

    const { name, description } = this.segmentForm.value;
    return;
    this.segmentService.create(name, description)
      .subscribe((result: SegmentListItem) => {
        this.segmentService.setCurrent(result);
        this.toRouter(result.id);
        this.creating = false;
      }, err => {
        this.msg.error(err.error);
        this.creating = false;
      });
  }

  closeCreateModal() {
    this.createModalVisible = false;
    this.segmentForm.reset({
      name: '',
      description: ''
    });
  }

  //#endregion

  constructor(
    private router: Router,
    private segmentService: SegmentService,
    private msg: NzMessageService,
    private fb: FormBuilder
  ) {
    this.segmentForm = this.fb.group({
      name: ['', [this.segmentNameValidator], [this.segmentNameAsyncValidator], 'change'],
      keyName: [{value: '', disabled: true}, [Validators.required]],
      description: [null]
    });
  }

  ngOnInit(): void {
    this.subscribeSearch();
    this.$search.next();
  }

  // 添加开关
  addSwitch() {
    this.createModalVisible = true;
  }

  ngOnDestroy(): void {
    this.destory$.next();
    this.destory$.complete();
  }

  // 关闭弹窗
  public handleCancel() {
    this.createModalVisible = false;
  }

  public handleOk() {
    if(!this.segmentName.length) {
      this.msg.error("请输入名称!");
      return;
    }
    this.isOkLoading = true;

    this.segmentService.create(this.segmentName, null)
      .subscribe((result: SegmentListItem) => {
        this.segmentService.setCurrent(result);
        this.toRouter(result.id);
        this.isOkLoading = false;
      }, errResponse => {
        this.msg.error(errResponse.error);
        this.isOkLoading = false;
      });
  }

  // 点击进入对应开关详情
  public onIntoSegmentDetail(data: SegmentListItem) {
    if(this.isIntoing) return;
    this.isIntoing = true;
    this.segmentService.setCurrent(data);
    this.toRouter(data.id);
  }

  // 路由跳转
  private toRouter(id: string) {
    this.router.navigateByUrl("/switch-manage/condition/" + encodeURIComponentFfc(id));
  }

  // 转换本地时间
  getLocalDate(date: string) {
    if (!date) return '';
    return new Date(date);
  }
}
