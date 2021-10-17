import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NzMessageService } from 'ng-zorro-antd/message';
import { AuthService } from 'src/app/services/auth.service';
import { LoginService } from 'src/app/services/login.service';

@Component({
  selector: 'app-do-login',
  templateUrl: './do-login.component.html',
  styleUrls: ['./do-login.component.less', '../login.component.less']
})
export class DoLoginComponent implements OnInit {

  loginForm!: FormGroup;

  isLoading: boolean = false;

  constructor(
    private fb: FormBuilder,
    private loginService: LoginService,
    private router: Router,
    private message: NzMessageService,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    this.initForm();
  }

  initForm() {
    this.loginForm = this.fb.group({
      email: [null, [Validators.required, Validators.email]],
      password: [null, [Validators.required]],
    });
  }

  doLogin() {
    if (this.loginForm.invalid) {
      for (const i in this.loginForm.controls) {
        this.loginForm.controls[i].markAsDirty();
        this.loginForm.controls[i].updateValueAndValidity();
      }
      return;
    }
    this.isLoading = true;
    this.loginService.login(this.loginForm.value)
      .subscribe(
        res => {
          localStorage.setItem('token', res.token);
          this.authService.redirectUrl = '';
          this.authService.getSelfInfo();
        },
        err => {
          this.isLoading = false;
          if (err.status === 401) {
            this.message.warning('用户名或密码错误！');
          }
        }
      )
  }

  changeRoute(url) {
    this.router.navigateByUrl('/login/' + url);
  }

}
