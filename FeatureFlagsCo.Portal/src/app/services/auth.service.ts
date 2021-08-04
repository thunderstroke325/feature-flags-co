import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  redirectUrl: string;

  constructor(
    private http: HttpClient,
    private router: Router
  ) { }

  getSelfInfo() {
    const url = environment.url + '/api/Authenticate/MyInfo';
    this.http.get(url)
      .pipe()
      .subscribe(
        res => {
          localStorage.setItem('auth', JSON.stringify(res));
          this.router.navigateByUrl(this.redirectUrl || '/');
        }
      );
  }


  logout() {
    const url = environment.url + '/api/Authenticate/logout';
    this.http.post(url, {})
      .pipe()
      .subscribe(
        res => {
          localStorage.clear();
          this.router.navigateByUrl('/login');
        }
      );
  }


  updateSelfInfo(params): Observable<any> {
    const url = environment.url + '/api/Authenticate/updateinfo';
    return this.http.post(url, params);
  }
}
