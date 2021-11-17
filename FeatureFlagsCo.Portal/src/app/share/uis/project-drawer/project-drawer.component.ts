import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';
import { IProject } from 'src/app/config/types';
import { ProjectService } from 'src/app/services/project.service';

@Component({
  selector: 'app-project-drawer',
  templateUrl: './project-drawer.component.html',
  styleUrls: ['./project-drawer.component.less']
})
export class ProjectDrawerComponent implements OnInit {

  private _project: IProject;

  projectForm: FormGroup;

  isEditing: boolean = false;

  isLoading: boolean = false;

  @Input()
  set project(project: IProject) {
    this.isEditing = !!project;
    if (project) {
      this.patchForm(project);
    } else {
      this.resetForm();
    }
    this._project = project;
  }

  get project() {
    return this._project;
  }

  @Input() currentAccountId: number;
  @Input() visible: boolean = false;
  @Output() close: EventEmitter<any> = new EventEmitter();

  constructor(
    private fb: FormBuilder,
    private projectService: ProjectService,
    private message: NzMessageService
  ) { }

  ngOnInit(): void {
    this.initForm();
  }

  initForm() {
    this.projectForm = this.fb.group({
      name: [null, [Validators.required]]
    });
  }

  patchForm(project: Partial<IProject>) {
    this.projectForm.patchValue({
      name: project.name
    });
  }

  resetForm() {
    this.projectForm && this.projectForm.reset();
  }

  onClose() {
    this.close.emit({ isEditing: false, project: undefined });
  }

  doSubmit() {
    if (this.projectForm.invalid) {
      for (const i in this.projectForm.controls) {
        this.projectForm.controls[i].markAsDirty();
        this.projectForm.controls[i].updateValueAndValidity();
      }
      return;
    }

    this.isLoading = true;

    const { name } = this.projectForm.value;

    if (this.isEditing) {
      this.projectService
        .putUpdateProject(this.currentAccountId, {name, id: this.project.id})
        .subscribe(
          updatedProject => {
            this.isLoading = false;
            this.close.emit({isEditing: true, project: updatedProject});
            this.message.success('更新成功！');
          },
          err => {
            this.isLoading = false;
          }
        );
    } else {
      this.projectService
        .postCreateProject(this.currentAccountId, {name})
        .subscribe(
          createdProject => {
            this.isLoading = false;
            this.close.emit({isEditing: false, project: createdProject});
            this.message.success('创建成功！');
          },
          err => {
            this.isLoading = false;
          }
        );
    }
  }
}
