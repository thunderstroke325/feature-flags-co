import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NzMessageService } from 'ng-zorro-antd/message';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { LoginService } from 'src/app/services/login.service';
import { repeatPasswordValidator } from 'src/app/utils/validators';

@Component({
  selector: 'app-reset',
  templateUrl: './reset.component.html',
  styleUrls: ['./reset.component.less', '../login.component.less']
})
export class ResetComponent implements OnInit, OnDestroy {

  destory$: Subject<void> = new Subject();

  resetForm!: FormGroup;

  resetToken: string = '';

  isLoading: boolean = false;

  constructor(
    private loginService: LoginService,
    private route: ActivatedRoute,
    private router: Router,
    private message: NzMessageService
  ) { }

  ngOnInit(): void {
    this.initForm();
    this.route.queryParams
      .pipe(
        takeUntil(this.destory$)
      )
      .subscribe(
        res => {
          this.resetToken = decodeURIComponent(res.tokenid.replace(/ /g, '+'));
        }
      )
  }

  ngOnDestroy(): void {
    this.destory$.next();
    this.destory$.complete();
  }

  initForm() {
    this.resetForm = new FormGroup({
      email: new FormControl(null, [Validators.required]),
      password: new FormControl(null, [Validators.required, Validators.minLength(5)]),
      _password: new FormControl(null, [Validators.required]),
    }, {
      validators: repeatPasswordValidator
    })
  }

  doReset() {
    if (this.resetForm.invalid) {
      for (const i in this.resetForm.controls) {
        this.resetForm.controls[i].markAsDirty();
        this.resetForm.controls[i].updateValueAndValidity();
      }
      return;
    }
    this.isLoading = true;
    const { _password, ...params } = this.resetForm.value
    this.loginService.resetPassword({ ...params, confirmPassword: _password, token: this.resetToken })
      .subscribe(
        res => {
          this.isLoading = false;
          localStorage.clear();
          this.message.success('重置密码成功，请重新登录！');
          this.router.navigateByUrl('/login');
        },
        err => {
          this.isLoading = false;
          this.message.error('无效的 token');
        }
      )
  }

}
