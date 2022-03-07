import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NzMessageService } from 'ng-zorro-antd/message';
import { phoneNumberOrEmailValidator } from "../../../utils/form-validators";
import { UserService } from "../../../services/user.service";
import { IDENTITY_TOKEN, LOGIN_REDIRECT_URL, USER_PROFILE } from "../../../utils/localstorage-keys";

@Component({
  selector: 'app-do-login',
  templateUrl: './do-login.component.html',
  styleUrls: ['./do-login.component.less', '../login.component.less']
})
export class DoLoginComponent implements OnInit {

  pwdLoginForm: FormGroup;
  passwordVisible: boolean = false;
  isLogin: boolean = false;

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private router: Router,
    private message: NzMessageService
  ) { }

  ngOnInit(): void {
    this.pwdLoginForm = this.fb.group({
      identity: ['', [Validators.required, phoneNumberOrEmailValidator]],
      password: ['', [Validators.required]]
    });
  }

  passwordLogin() {
    if (this.pwdLoginForm.invalid) {
      Object.values(this.pwdLoginForm.controls).forEach(control => {
        if (control.invalid) {
          control.markAsDirty();
          control.updateValueAndValidity({ onlySelf: true });
        }
      });

      return;
    }

    this.isLogin = true;

    const {identity, password} = this.pwdLoginForm.value;
    this.userService.loginByPassword(identity, password).subscribe(
      response => this.handleResponse(response),
      error => this.handleError(error)
    )
  }

  phoneCodeLogin(data) {
    const {phoneNumber, code} = data;

    this.isLogin = true;

    this.userService.loginByPhoneCode(phoneNumber, code).subscribe(
      response => this.handleResponse(response),
      error => this.handleError(error)
    )
  }

  forgetPassword() {
    this.router.navigateByUrl('/login/forget-password');
  }

  register() {
    this.router.navigateByUrl('/login/register');
  }

  handleResponse(response) {
    this.isLogin = false;

    if (!response.success) {
      this.message.error(response.message);
      return;
    }

    this.message.success('登录成功');

    // store identity token
    localStorage.setItem(IDENTITY_TOKEN, response.token);

    // store user profile
    this.userService.getProfile().subscribe(profile => {
        localStorage.setItem(USER_PROFILE, JSON.stringify(profile));

        const redirectUrl = localStorage.getItem(LOGIN_REDIRECT_URL);
        if (redirectUrl) {
          localStorage.removeItem(LOGIN_REDIRECT_URL);
          this.router.navigateByUrl(redirectUrl);
        } else {
          this.router.navigateByUrl('/');
        }
      }
    );
  }

  handleError(_) {
    this.isLogin = false;

    this.message.error(`服务错误，请联系运营人员。`);
  }
}
