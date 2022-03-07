import { Component, EventEmitter, Input, OnInit, Output, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { phoneNumberValidator } from "../../../utils/form-validators";
import { UserService } from "../../../services/user.service";
import { NzMessageService } from "ng-zorro-antd/message";

@Component({
  selector: 'phone-code-form',
  template: `
    <form nz-form [formGroup]="form" (ngSubmit)="submit()">
      <nz-form-item>
        <nz-form-control nzErrorTip="请输入有效的手机号">
          <nz-input-group nzPrefixIcon="user">
            <input maxlength="11" type="tel" nz-input formControlName="phoneNumber" placeholder="请输入手机号"/>
          </nz-input-group>
        </nz-form-control>
      </nz-form-item>
      <nz-form-item>
        <nz-form-control nzErrorTip="请输入有效的验证码">
          <nz-input-group>
            <div nz-row nzJustify="space-between">
              <div nz-col nzSpan="14">
                <nz-input-group nzPrefixIcon="safety">
                  <input nz-row type="text" nz-input formControlName="code" placeholder="请输入6位验证码"/>
                </nz-input-group>
              </div>
              <div nz-col>
                <button nzAlign="middle" type="button" nz-row nz-button nzType="default" (click)="requestPhoneCode()"
                        [disabled]="getPhoneCodeInterval !== 0" [nzLoading]="isGettingPhoneCode">
                  <i style="padding-top: 3px" nz-icon nzType="send" nzTheme="outline"></i>
                  {{getPhoneCodeInterval === 0 ? '获取验证码' : getPhoneCodeInterval + ' s 后重试'}}
                </button>
              </div>
            </div>
          </nz-input-group>
        </nz-form-control>
      </nz-form-item>
      <ng-container [ngTemplateOutlet]="footer"></ng-container>
    </form>
  `,
  styles: [
  ]
})
export class PhoneCodeFormComponent implements OnInit {

  @Input() scene: string = '';
  @Input() footer: TemplateRef<any>;
  @Output() onSubmit = new EventEmitter<any>();

  form: FormGroup;

  constructor(
    private message: NzMessageService,
    private userService: UserService,
    private fb: FormBuilder
  ) { }

  ngOnInit(): void {
    this.form = this.fb.group({
      phoneNumber: ['', [Validators.required, phoneNumberValidator]],
      code: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  submit() {
    if (this.form.valid) {
      this.onSubmit.emit(this.form.value);
      return;
    }

    Object.values(this.form.controls).forEach(control => {
      if (control.invalid) {
        control.markAsDirty();
        control.updateValueAndValidity({ onlySelf: true });
      }
    });
  }

  isGettingPhoneCode: boolean = false;
  getPhoneCodeInterval: number = 0;
  requestPhoneCode() {
    this.form.get('code').reset();

    let phoneNumber = this.form.get('phoneNumber');
    if (phoneNumber.invalid) {
      phoneNumber.markAsDirty();
      phoneNumber.updateValueAndValidity();
      return;
    }

    this.isGettingPhoneCode = true;
    this.sendCode(phoneNumber.value);
  }

  sendCode(phoneNumber: string) {
    this.userService.sendIdentityCode(phoneNumber, this.scene)
      .subscribe(
      _ => {
        this.message.success('验证码已发送, 请注意查收');
        this.isGettingPhoneCode = false;

        this.getPhoneCodeInterval = 60;
        const phoneCodeInterval = setInterval(() => {
          if (this.getPhoneCodeInterval === 0) {
            clearInterval(phoneCodeInterval);
            return;
          }

          this.getPhoneCodeInterval--;
        }, 1000);

      },
      err => {
        this.isGettingPhoneCode = false;

        if (err.status === 403) {
          this.message.warning(err.error);
        } else {
          this.message.error('发送验证码失败, 请联系运营人员');
        }
      }
    );
  }
}
