import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NzMessageService } from 'ng-zorro-antd/message';
import { LoginService } from 'src/app/services/login.service';
import { repeatPasswordValidator } from 'src/app/utils/validators';


@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.less', '../login.component.less']
})
export class RegisterComponent implements OnInit {

  registerForm!: FormGroup;

  isLoading: boolean = false;
  phoneNumber: string = null;
  orgName: string = null;
  inviteCode: string = null;

  get password() {
    if (!this.registerForm || !this.registerForm.value) return '';
    return this.registerForm.value['password'];
  }

  get _password() {
    if (!this.registerForm || !this.registerForm.value) return '';
    return this.registerForm.value['_password'];
  }

  constructor(
    private loginService: LoginService,
    private router: Router,
    private route: ActivatedRoute,
    private message: NzMessageService
  ) {
    this.route.queryParams.subscribe(params => {
      this.phoneNumber = params['tel'];
    });
  }

  ngOnInit(): void {
    this.initForm();
  }

  initForm() {
    this.registerForm = new FormGroup({
      email: new FormControl(null, [Validators.required, Validators.email]),
      password: new FormControl(null, [Validators.required, Validators.minLength(5)]),
      _password: new FormControl(null, [Validators.required]),
      orgName: new FormControl(this.orgName, [Validators.required]),
      phoneNumber: new FormControl(this.phoneNumber, [Validators.required]),
      inviteCode: new FormControl(this.inviteCode, [Validators.required])
    }, {
      validators: repeatPasswordValidator
    })
  }

  getPassword = (key: string = 'password') => this[key];

  doRegister() {
    if (this.registerForm.invalid) {
      for (const i in this.registerForm.controls) {
        this.registerForm.controls[i].markAsDirty();
        this.registerForm.controls[i].updateValueAndValidity();
      }
      return;
    }
    this.isLoading = true;
    const { _password, ...params } = this.registerForm.value
    params.phoneNumber = params.phoneNumber.toString()
    this.loginService.register(params)
      .subscribe(
        res => {
          this.isLoading = false;
          this.message.success('注册成功！');
          this.router.navigateByUrl('/login');
        }, err => {
          this.isLoading = false;
          this.message.error(err.error.message);
        }
      );
  }

  changeRoute(url) {
    this.router.navigateByUrl(url);
  }

}
