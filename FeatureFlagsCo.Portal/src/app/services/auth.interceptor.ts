import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { NzMessageService } from 'ng-zorro-antd/message';
import { Router } from '@angular/router';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  constructor(
    private message: NzMessageService,
    private router: Router
  ) { }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<unknown>> {

    const authReq = request.clone({
      headers: request.headers
        .set('Authorization', 'Bearer ' + localStorage.getItem('token') || '')
    });


    return next.handle(authReq)
      .pipe(
        catchError(err => {
          if (err.status === 401) {
            localStorage.clear();
            this.router.navigateByUrl('/login');
          }

          throw err;
        })
      );
  }
}

