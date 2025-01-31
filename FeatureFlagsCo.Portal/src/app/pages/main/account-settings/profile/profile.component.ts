import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';
import { getAuth } from 'src/app/utils';
import { UserService } from "../../../../services/user.service";

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.less']
})
export class ProfileComponent {

  profileForm!: FormGroup;

  auth = getAuth();

  isLoading: boolean = false;

  constructor(
    private userService: UserService,
    private message: NzMessageService,
    private fb: FormBuilder
  ) {
    this.profileForm = this.fb.group({
      email: [{value: this.auth.email ?? '未关联', disabled: true}, [Validators.required, Validators.email]],
      phoneNumber: [{value: this.auth.phoneNumber ?? '未绑定', disabled: true}, [Validators.required]],
    });
  }

  updateProfile() {
    if (this.profileForm.invalid) {
      for (const control in this.profileForm.controls) {
        this.profileForm.controls[control].markAsDirty();
        this.profileForm.controls[control].updateValueAndValidity();
      }

      return;
    }

    this.isLoading = true;
    this.userService.updateProfile('no-value-yet')
      .subscribe(
        _ => {
          this.isLoading = false;
          this.message.success('信息更新成功');
        },
        _ => {
          this.isLoading = false;

          this.message.warning('该功能正在建设中...');
        }
      );
  }
}
