import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';
import { BehaviorSubject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { IAccount } from 'src/app/config/types';
import { CustomEventTrackOption, EventType, IMetric } from 'src/app/pages/main/switch-manage/types/experimentations';
import { MetricService } from 'src/app/services/metric.service';
import { TeamService } from 'src/app/services/team.service';


@Component({
  selector: 'app-metric-drawer',
  templateUrl: './metric-drawer.component.html',
  styleUrls: ['./metric-drawer.component.less']
})
export class MetricDrawerComponent implements OnInit {

  private _metric: IMetric;

  metricForm: FormGroup;
  isEditing: boolean = false;
  isLoading: boolean = false;
  selectedMaintainer: any;

  customEventType: EventType = EventType.Custom;
  pageViewEventType: EventType = EventType.PageView;
  clickEventType: EventType = EventType.Click;

  customEventTrackConversion: CustomEventTrackOption = CustomEventTrackOption.Conversion;
  customEventTrackNumeric: CustomEventTrackOption = CustomEventTrackOption.Numeric;

  @Input()
  set metric(metric: IMetric) {
    this.isEditing = metric && !!metric.id;
    if (metric) {
      metric.customEventTrackOption = metric.customEventTrackOption || this.customEventTrackConversion;
      metric.eventType = metric.eventType || this.customEventType;
      this.patchForm(metric);
    } else {
      this.resetForm();
    }
    this._metric = metric;
  }

  get metric() {
    return this._metric;
  }

  @Input() visible: boolean = false;
  @Output() close: EventEmitter<any> = new EventEmitter();

  constructor(
    private fb: FormBuilder,
    private teamService: TeamService,
    private metricService: MetricService,
    private message: NzMessageService
  ) {
    const currentAccount: IAccount = JSON.parse(localStorage.getItem('current-account'));

    this.maintainerSearchChange$.pipe(
      debounceTime(500),
      distinctUntilChanged()
    ).subscribe(searchText => {
      this.teamService.searchMembers(currentAccount.id, searchText).subscribe((result) => {
        this.maintainerList = result;
        this.isMaintainersLoading = false;
      }, error => {
        console.log("search error");
        this.isMaintainersLoading = false;
      });
    });
   }

  ngOnInit(): void {
    this.initForm();
  }

  initForm() {
    this.metricForm = this.fb.group({
      name: [null, [Validators.required]],
      description: [null],
      eventType: [EventType.Custom, [Validators.required]],
      maintainerUserId: [null, [Validators.required]],
      customEventTrackOption: [CustomEventTrackOption.Conversion, [Validators.required]],
      eventName: [null, [Validators.required]],
    });
  }

  patchForm(metric: Partial<IMetric>) {
    this.metricForm.patchValue({
      name: metric.name,
      description: metric.description,
      eventType: metric.eventType,
      maintainerUserId: metric.maintainerUserId,
      customEventTrackOption: metric.customEventTrackOption,
      eventName: metric.eventName
    });
  }

  resetForm() {
    this.metricForm && this.metricForm.reset();
  }

  maintainerSearchChange$ = new BehaviorSubject('');
  isMaintainersLoading = false;
  maintainerList: any[];
  onSearchMaintainer(value: string) {
    if (value.length > 0) {
      this.isMaintainersLoading = true;
      this.maintainerSearchChange$.next(value);
    }
  }

  onClose() {
    this.close.emit({ isEditing: false, data: this.metric });
  }

  doSubmit() {
    if (this.metricForm.invalid) {
      for (const i in this.metricForm.controls) {
        this.metricForm.controls[i].markAsDirty();
        this.metricForm.controls[i].updateValueAndValidity();
      }
      return;
    }

    this.isLoading = true;

    const { name, description, eventType, maintainerUserId, customEventTrackOption, eventName } = this.metricForm.value;

    if (this.isEditing) {
      this.metricService.updateMetric({id: this.metric.id, envId: this.metric.envId, name, description, eventType, maintainerUserId, customEventTrackOption, eventName }).pipe()
        .subscribe(
          res => {
            this.isLoading = false;
            const maintainer = this.maintainerList.find(m => m.userId === res.maintainerUserId);
            this.close.emit({isEditing: true, data: {...res, maintainerName: maintainer.userName }});
            this.message.success('更新成功！');
          },
          err => {
            this.message.error('发生错误，请重试！');
            this.isLoading = false;
          }
        );
    } else {
      this.metricService.createMetric({ envId: this.metric.envId, name, description, eventType, maintainerUserId, customEventTrackOption, eventName })
        .pipe()
        .subscribe(
          res => {
            this.isLoading = false;
            const maintainer = this.maintainerList.find(m => m.userId === res.maintainerUserId);
            this.close.emit({isEditing: false, data: {...res, maintainerName: maintainer.userName }});
            this.message.success('创建成功！');
          },
          err => {
            this.message.error('发生错误，请重试！');
            this.isLoading = false;
          }
        );
    }
  }
}
